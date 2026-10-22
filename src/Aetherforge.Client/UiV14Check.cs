using Aetherforge.Domain;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>Opt-in QA: injects mouse/key events through Godot's GUI path, never emits Pressed.</summary>
public partial class UiV14Check : Node
{
    public string EvidencePath { get; set; } = "";
    public string CaptureDirectory { get; set; } = "";
    private ExpeditionView _view = null!;
    private readonly List<string> _checks = new();
    private readonly HashSet<Window> _entered = new();
    [StructLayout(LayoutKind.Sequential)] private struct Pointer { public int X, Y; }
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out Pointer point);
    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);
    private bool _nativePointer;
    private Pointer _originalPointer;
    public void Bind(ExpeditionView view) => _view = view;

    public override async void _Ready()
    {
        try
        {
            _nativePointer = OperatingSystem.IsWindows() && DisplayServer.GetName() != "headless" && !_view.Overview;
            if (_nativePointer) GetCursorPos(out _originalPointer);
            await Frames(4);
            await _view.VerifyLayouts();
            var textServer = TextServerManager.GetPrimaryInterface();
            var actualWeights = new List<int>();
            foreach (var face in new[] { BrassUi.BodyFont, BrassUi.StrongFont })
                foreach (var rid in face.GetRids())
                {
                    var coordinates = textServer.FontGetVariationCoordinates(rid);
                    var expected = ReferenceEquals(face, BrassUi.BodyFont) ? 400 : 600;
                    Require(coordinates.ContainsKey(BrassTypography.WeightAxis) && coordinates[BrassTypography.WeightAxis].AsInt32() == expected, "peso solicitado nao chegou ao cache nativo do renderizador");
                    actualWeights.Add(coordinates[BrassTypography.WeightAxis].AsInt32());
                }
            Require(actualWeights.Count >= 2, "caches de fonte ausentes");
            const string glyphs = "Alquimista do Éter Pressão çã 0123456789 +−-/ %";
            Require(glyphs.All(c => BrassUi.BodyFont.HasChar(c) && BrassUi.StrongFont.HasChar(c)), "glifos portugueses/numericos ausentes");
            Require(BrassUi.BodyFont.GetFontName().Contains("Source", StringComparison.OrdinalIgnoreCase), "fonte funcional nao e Source Sans 3");
            Require(BrassUi.StrongFont is FontVariation strong && strong.VariationOpentype[BrassTypography.WeightAxis].AsInt32() == 600 && strong.VariationEmbolden == 0, "semibold deve usar peso real 600, nao falso negrito");
            var digitWidths = new[] { "111111", "888888", "000000" }.Select(s => BrassUi.StrongFont.GetStringSize(s, HorizontalAlignment.Left, -1, BrassUi.S(20)).X).ToArray();
            Require(digitWidths.Max() - digitWidths.Min() < 1, "digitos de contadores nao sao tabulares");
            _checks.Add("Source Sans 3 local, glifos PT-BR, peso real, digitos tabulares");

            var windows = Descendants(_view).OfType<Window>().ToArray();
            Require(windows.Length == 3, "quantidade de instrumentos");
            foreach (var name in new[] { "Status", "Herói", "Expedição" })
            {
                var window = windows.First(w => w.Title.Contains(name.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase));
                if (!window.Visible) await Click(Find(name));
                Require(window.Visible, "lancador nao abriu " + name);
            }
            await Frames(3);
            // Hidden native windows do not sort all child containers until shown.
            foreach (var usable in new[] { new Vector2I(1366, 728), new Vector2I(1920, 1040), new Vector2I(2560, 1400) })
                await _view.VerifyMenuBounds(usable);
            _checks.Add("fixtures da area util 1366x728, 1920x1040 e 2560x1400; nao DPI fisico");
            foreach (var window in windows)
            {
                var close = Descendants(window).OfType<Button>().First(b => b.Text == "×");
                Require(!HasScrollAncestor(close, window), "cabecalho esta dentro da rolagem");
                foreach (var action in Descendants(window).OfType<Button>().Where(b => b.Text is "Pausar" or "Pausar expedição" or "Recuar" or "Reiniciar"))
                {
                    Require(!HasScrollAncestor(action, window), "acao fixa esta dentro da rolagem: " + action.Text);
                    Require(new Rect2(Vector2.Zero, window.Size).Encloses(action.GetGlobalRect()), "rodape cortado: " + action.Text);
                }
                var scrolls = Descendants(window).OfType<ScrollContainer>().ToArray();
                Require(scrolls.Length >= 1, "miolo rolavel ausente");
            }
            _checks.Add("cabecalhos/rodapes fixos, miolos rolaveis e acoes dentro das janelas");
            var nativeIds = windows.Select(w => w.GetWindowId()).ToArray();
            if (DisplayServer.GetName() != "headless" && !_view.Overview)
            {
                Require(nativeIds.Distinct().Count() == 3 && nativeIds.All(id => id >= 0 && id != _view.GetWindow().GetWindowId()), "janelas nao sao nativas distintas");
                _checks.Add("tres janelas nativas distintas do palco");
            }

            var details = Find("Detalhes");
            var beforeDetail = _view.Stage.DetailedHud;
            await Click(details);
            Require(_view.Stage.DetailedHud != beforeDetail, "toggle Detalhes nao chegou ao palco");
            await Click(details);
            Require(_view.Stage.DetailedHud == beforeDetail, "Detalhes nao retornou ao modo conciso");
            Require(_view.Stage.Hud.VisibleAllyCount == 3, "HUD perdeu barra de vida de aliado vivo");
            _checks.Add("toggle HUD conciso/detalhado por mouse; tres vidas visiveis no palco");

            var pause = Find("Pausar");
            await Click(pause);
            Require(_view.Simulation.Paused, "clique nao pausou");
            var elapsed = _view.Simulation.Elapsed; var distance = _view.Simulation.Distance;
            var feedback = _view.Stage.Hud.ActiveFeedbackCount;
            await Frames(3);
            Require(_view.Simulation.Elapsed == elapsed && _view.Simulation.Distance == distance && _view.Stage.Hud.ActiveFeedbackCount == feedback, "pausa nao congelou regras e feedback");
            await Key(pause.GetWindow(), Godot.Key.Space);
            Require(!_view.Simulation.Paused, "Space nao retomou pela janela");
            _checks.Add("mouse pausa e Space retoma, sem avancar durante pausa");

            var healer = Find("CURA");
            await Reveal(healer);
            healer.GrabFocus();
            var pressed = 0; healer.Pressed += () => pressed++;
            await Key(healer.GetWindow(), Godot.Key.Enter);
            Require(pressed > 0 && _view.SelectedMemberIndex == 2 && _view.Stage.SelectedMemberIndex == 2, "Enter no seletor nao selecionou healer/HUD");
            var focused = healer.GetWindow().GuiGetFocusOwner();
            await Key(healer.GetWindow(), Godot.Key.Tab);
            Require(healer.GetWindow().GuiGetFocusOwner() != null && healer.GetWindow().GuiGetFocusOwner() != focused, "Tab nao moveu foco");
            await Click(Find("DPS")); Require(_view.SelectedMemberIndex == 1, "mouse DPS nao selecionou");
            await Click(Find("TANK")); Require(_view.SelectedMemberIndex == 0, "mouse TANK nao selecionou");
            _checks.Add("Enter seleciona, Tab navega, mouse seleciona os tres membros e HUD acompanha");

            var status = windows.First(w => w.Title.Contains("STATUS"));
            foreach (var memberIndex in new[] { 2, 1, 0 })
            {
                var slip = Descendants(status).OfType<BrassPartyRow>().First(row => row.MemberIndex == memberIndex);
                await Click(slip);
                Require(_view.SelectedMemberIndex == memberIndex && _view.Stage.SelectedMemberIndex == memberIndex, "ficha Status nao sincronizou selecao");
                var ally = _view.Simulation.Party[memberIndex];
                Require(Math.Abs(slip.Meter.HealthRatio - ally.Health/ally.MaxHealth) < .001, "vida da ficha difere da regra");
                Require(slip.GetChildren().OfType<BrassButtonFeedback>().Count() == 1, "feedback duplicado na ficha");
                Require(slip.GetChildren().OfType<BrassButtonFeedback>().Single().TransitionCount > 0, "input nao acionou feedback da ficha");
            }
            var reduceMotion = Descendants(status).OfType<CheckButton>().First(button => button.Name == "ReduceUiMotion");
            bool reducedBefore = BrassMotion.Reduced;
            await Click(reduceMotion);
            Require(BrassMotion.Reduced != reducedBefore && Descendants(status).OfType<BrassVitalMeter>().All(meter => !meter.HasTrail), "opcao reduzir movimento nao removeu transicoes");
            await Click(reduceMotion); Require(BrassMotion.Reduced == reducedBefore, "opcao nao restaurou movimento");
            _checks.Add("V15: fichas selecionam por mouse com feedback e vida real; opcao reduzir movimento liga/desliga por mouse");
            await Key(status, Godot.Key.Escape);
            Require(!status.Visible, "Escape nao fechou o instrumento");
            await Click(Find("Status")); Require(status.Visible, "reabertura por mouse falhou");
            _checks.Add("Escape fecha e mouse reabre menu sem parar a rota");

            var deadline = Time.GetTicksMsec() + 15000;
            while (_view.Simulation.Phase == ExpeditionPhase.March && Time.GetTicksMsec() < deadline) await Frames(1);
            Require(_view.Simulation.Phase == ExpeditionPhase.Combat, "primeiro combate nao alcancado");
            await Frames(4);
            await Key(status, Godot.Key.P);
            Require(_view.Simulation.ShieldRemaining > 0 && _view.Simulation.ShieldCooldown > 0, "P nao ativou escudo");
            _checks.Add("P na janela ativa escudo real");
            if (!string.IsNullOrEmpty(CaptureDirectory) && DisplayServer.GetName() != "headless")
            {
                await Key(status, Godot.Key.Space);
                Require(_view.Simulation.Paused, "pausa para captura falhou");
                foreach (var body in Descendants(status).OfType<ScrollContainer>()) body.ScrollVertical = 0;
                await Frames(3);
                Directory.CreateDirectory(CaptureDirectory);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                foreach (var window in windows)
                {
                    var name = window.Title.Contains("STATUS") ? "status" : window.Title.Contains("HERÓI") ? "hero" : "route";
                    Require(window.GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory, name + ".png")) == Error.Ok, "captura de menu falhou");
                }
                Require(GetViewport().GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory, "input-stage.png")) == Error.Ok, "captura de palco falhou");
                await Key(status, Godot.Key.Space);
            }
            await Click(Descendants(status).OfType<Button>().First(b => b.Text == "Recuar"));
            Require(_view.Simulation.Phase == ExpeditionPhase.Retreated, "mouse nao recuou");
            await Click(Descendants(status).OfType<Button>().First(b => b.Text == "Reiniciar" || b.Text.StartsWith("Reiniciar rota")));
            Require(_view.Simulation.Phase == ExpeditionPhase.March && _view.Simulation.Victories == 0 && _view.Simulation.ShieldCooldown == 0, "mouse nao reiniciou");
            Require(_view.Stage.Hud.ActiveFeedbackCount == 0, "reset manteve numeros de combate antigos");
            Require(!Descendants(_view.Stage).OfType<Label3D>().Any(l => l.Name == "EventAmount"), "numero Label3D antigo ainda existe");
            _checks.Add("mouse recua/reinicia e numeros 3D antigos ausentes");

            var report = new { passed = true, scale = BrassUi.Scale, display = DisplayServer.GetName(), overview = _view.Overview,
                inputMethod = _nativePointer ? "Win32 automated mouse with cursor restoration; Godot PushInput keyboard; not human physical input or direct Pressed" : "Godot Viewport.PushInput mouse/key; not direct Pressed or physical input",
                fontFamily = "Source Sans 3", baseFaceMetadata = BrassUi.BodyFont.GetFontName(), actualWeights, digitWidths, nativeIds, checks = _checks,
                windows = windows.Select(w => new { title = w.Title, x = w.Position.X, y = w.Position.Y, width = w.Size.X, height = w.Size.Y }).ToArray() };
            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            if (!string.IsNullOrEmpty(EvidencePath)) { Directory.CreateDirectory(Path.GetDirectoryName(EvidencePath)!); File.WriteAllText(EvidencePath, json); }
            GD.Print("UI V14 INPUT PASS " + json);
            GetTree().Quit(0);
        }
        catch (Exception error)
        {
            GD.PrintErr("UI V14 INPUT FAIL: " + error);
            if (CaptureDirectory.Length > 0 && DisplayServer.GetName() != "headless")
            {
                Directory.CreateDirectory(CaptureDirectory);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                GetViewport().GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory, "input-failure.png"));
                foreach (var menu in Descendants(_view).OfType<Window>().Where(w => w.Visible))
                    menu.GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory, "failure-" + menu.Name + ".png"));
            }
            GetTree().Quit(1);
        }
        finally
        {
            if (_nativePointer)
            {
                mouse_event(0x0004, 0, 0, 0, UIntPtr.Zero); // Never leave a test mouse-down held after failure.
                SetCursorPos(_originalPointer.X, _originalPointer.Y);
            }
        }
    }

    private Button Find(string text) => Descendants(_view).OfType<Button>().First(b => b.Text == text);
    private async Task Frames(int count) { for (var i = 0; i < count; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); }
    private async Task Reveal(Button button)
    {
        for (Node? node = button.GetParent(); node != null && node is not Window; node = node.GetParent())
            if (node is ScrollContainer scroll) scroll.EnsureControlVisible(button);
        await Frames(2);
    }
    private async Task Click(Button button)
    {
        Require(!button.Disabled, "tentativa de clicar desabilitado: " + button.Text);
        await Reveal(button);
        var window = button.GetWindow(); var point = button.GetGlobalRect().GetCenter();
        Require(new Rect2(Vector2.Zero, window.Size).HasPoint(point), "clique fora da janela: " + button.Text);
        // Embedded windows receive mouse events from their embedder, as on a real desktop.
        if (window.IsEmbedded()) { point += window.Position; window = _view.GetWindow(); }
        if (_nativePointer)
        {
            window.GrabFocus();
            var screen = window.Position + point;
            SetCursorPos((int)screen.X, (int)screen.Y);
            await Frames(2);
        }
        if (_entered.Add(window) && window.GuiGetHoveredControl() == null) window.NotifyMouseEntered();
        if (_nativePointer)
        {
            mouse_event(0x0002, 0, 0, 0, UIntPtr.Zero);
            await Frames(1);
            mouse_event(0x0004, 0, 0, 0, UIntPtr.Zero);
            await Frames(2);
            return;
        }
        window.PushInput(new InputEventMouseMotion { Position = point, GlobalPosition = point, WindowId = window.GetWindowId() }, true);
        window.PushInput(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, ButtonMask = MouseButtonMask.Left, Pressed = true, WindowId = window.GetWindowId() }, true);
        // Keep an injected click atomic: native Windows cursor polling between frames
        // otherwise replaces its synthetic hover with the physical pointer location.
        window.PushInput(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = false, WindowId = window.GetWindowId() }, true);
        await Frames(2);
    }
    private async Task Key(Window window, Godot.Key key)
    {
        if (window.IsEmbedded()) { window.GrabFocus(); window = _view.GetWindow(); }
        window.PushInput(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = true, WindowId = window.GetWindowId() }, true);
        await Frames(1);
        window.PushInput(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = false, WindowId = window.GetWindowId() }, true);
        await Frames(2);
    }
    private static bool HasScrollAncestor(Node child, Window window)
    {
        for (var node = child.GetParent(); node != null && node != window; node = node.GetParent()) if (node is ScrollContainer) return true;
        return false;
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static IEnumerable<Node> Descendants(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            yield return child;
            foreach (var node in Descendants(child)) yield return node;
        }
    }
}
