using Godot;
using Aetherforge.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Opt-in executable proof of actual UI callbacks, not just a layout inspection.</summary>
public partial class ExpeditionInteractionCheck : Node
{
    private ExpeditionView _view = null!;
    public void Bind(ExpeditionView view) => _view = view;
    public override async void _Ready()
    {
        try
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            var buttons = Descendants(_view).OfType<Button>().ToList();
            Button Find(string text) => buttons.First(b => b.Text == text);
            var sim = _view.Simulation;
            var pause = Find("Pausar"); pause.EmitSignal(Button.SignalName.Pressed);
            Require(sim.Paused, "Botão Pausar não pausou");
            var time = sim.Elapsed; var distance = sim.Distance;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Require(sim.Elapsed == time && sim.Distance == distance, "Pausa da UI não congelou a simulação");
            Find("CURA").EmitSignal(Button.SignalName.Pressed);
            Require(_view.SelectedMemberIndex == 2, "Seleção de healer não mudou o retrato/estatísticas");
            Find("DPS").EmitSignal(Button.SignalName.Pressed);
            Require(_view.SelectedMemberIndex == 1, "Seleção DPS falhou");
            Find("TANK").EmitSignal(Button.SignalName.Pressed);
            Require(_view.SelectedMemberIndex == 0, "Seleção tank falhou");
            pause.EmitSignal(Button.SignalName.Pressed);
            Require(!sim.Paused, "Continuar não retomou");
            var windows = Descendants(_view).OfType<Window>().ToList();
            Require(windows.Count == 3, "Três menus devem continuar independentes");
            var statusWindow = windows.First(w => w.Title.Contains("STATUS"));
            Descendants(statusWindow).OfType<Button>().First(b => b.Text == "×").EmitSignal(Button.SignalName.Pressed);
            Require(!statusWindow.Visible, "Fechar Status não ocultou janela");
            Find("Status").EmitSignal(Button.SignalName.Pressed);
            Require(statusWindow.Visible, "Reabrir Status falhou");
            if (DisplayServer.GetName() != "headless" && !_view.Overview)
            {
                Find("Herói").EmitSignal(Button.SignalName.Pressed);
                Find("Expedição").EmitSignal(Button.SignalName.Pressed);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                var ids = windows.Select(window => window.GetWindowId()).ToArray();
                Require(windows.All(window => window.Visible) && ids.Distinct().Count() == 3 &&
                    ids.All(id => id >= 0 && id != _view.GetWindow().GetWindowId()), "Menus não viraram três janelas nativas distintas");
                GD.Print("EXPEDITION NATIVE WINDOWS PASS: three separate OS window IDs: " + string.Join(",", ids));
            }
            var deadline = Time.GetTicksMsec() + 12000;
            while (sim.Phase == ExpeditionPhase.March && Time.GetTicksMsec() < deadline)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Require(sim.Phase == ExpeditionPhase.Combat, "Não chegou ao primeiro encontro");
            // Refresh of cooldown labels is throttled by the view; callbacks are the proof.
            var shield = buttons.First(b => b.Text.Contains("Escudo de Pressão"));
            shield.EmitSignal(Button.SignalName.Pressed);
            Require(sim.ShieldRemaining > 0 && sim.ShieldCooldown > 0, "Escudo da UI não chegou às regras");
            Find("Recuar").EmitSignal(Button.SignalName.Pressed);
            Require(sim.Phase == ExpeditionPhase.Retreated, "Recuo da UI falhou");
            time = sim.Elapsed;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Require(sim.Elapsed == time, "Recuo da UI continuou produzindo");
            var reset = buttons.First(b => b.Text == "Reiniciar" || b.Text.StartsWith("Reiniciar rota"));
            reset.EmitSignal(Button.SignalName.Pressed);
            Require(sim.Phase == ExpeditionPhase.March && sim.StageIndex == 0 && sim.Victories == 0 && sim.ShieldCooldown == 0, "Reset da UI incompleto");
            GD.Print("EXPEDITION INTERACTION PASS: pause/resume, three selectors, close/reopen menu, shield, retreat and reset via Pressed callbacks");
            GetTree().Quit(0);
        }
        catch (Exception error) { GD.PrintErr("EXPEDITION INTERACTION FAIL: " + error); GetTree().Quit(1); }
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static IEnumerable<Node> Descendants(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
}
