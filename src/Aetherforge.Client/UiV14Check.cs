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
            Require(Descendants(_view).OfType<BrassDossier>().Count()==3, "tres instrumentos Dossie obrigatorios");
            Require(!Descendants(_view).OfType<BrassDossierRuler>().Any() && !Descendants(_view).OfType<BrassStatSocket>().Any() && _view.StatusPanel.IsInsideTree(), "Status deve usar resumo/detalhe do integrante, sem regua ou sockets antigos");
            var heroWindow=windows.First(w=>w.Name=="Instrument1");
            var spread=Descendants(heroWindow).OfType<GridContainer>().First(g=>g.Name=="HeroDossierSpread");
            Require(spread.Columns is 1 or 2, "adaptacao do Heroi invalida");
            var originalHeroSize=heroWindow.Size;
            var heroResizeColumnModes=new List<int>();
            foreach(var (width, expected) in new[] { (BrassUi.S(588),2), (BrassUi.S(330),1) })
            {
                heroWindow.Size=new Vector2I(width, originalHeroSize.Y); await Frames(4);
                Require(spread.Columns==expected, "Heroi nao alternou entre lado a lado e empilhado");
                heroResizeColumnModes.Add(spread.Columns);
            }
            heroWindow.Size=originalHeroSize; await Frames(4);
            var slots=Descendants(heroWindow).OfType<Button>().Where(b=>b.Name.ToString().StartsWith("LootSlot")).ToArray();
            Require(slots.Length==21, "bolsa perdeu os 21 espacos");
            var heroBody=Descendants(heroWindow).OfType<ScrollContainer>().Single();
            var bodyRect=heroBody.GetGlobalRect();
            float rightEdge=bodyRect.End.X-(heroBody.GetVScrollBar().Visible?heroBody.GetVScrollBar().Size.X:0);
            Require(slots.All(b=>b.GetGlobalRect().Position.X>=bodyRect.Position.X-.5f && b.GetGlobalRect().End.X<=rightEdge+.5f), "bolsa cortada horizontalmente");
            _checks.Add("V19: tres folhas, Status de integrante, Heroi responsivo e bolsa de 21 slots");
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
                Require(slip.PortraitView.Texture is AtlasTexture && slip.Meter.Compact, "ficha deve usar recorte v12 e barra editorial");
                Require(slip.GetChildren().OfType<BrassButtonFeedback>().Count() == 1, "feedback duplicado na ficha");
                Require(slip.GetChildren().OfType<BrassButtonFeedback>().Single().TransitionCount > 0, "input nao acionou feedback da ficha");
            }
            var reduceMotion = Descendants(status).OfType<CheckButton>().First(button => button.Name == "ReduceUiMotion");
            bool reducedBefore = BrassMotion.Reduced;
            await Click(reduceMotion);
            Require(BrassMotion.Reduced != reducedBefore && Descendants(status).OfType<BrassVitalMeter>().All(meter => !meter.HasTrail), "opcao reduzir movimento nao removeu transicoes");
            await Click(reduceMotion); Require(BrassMotion.Reduced == reducedBefore, "opcao nao restaurou movimento");
            _checks.Add("V15: fichas selecionam por mouse com feedback e vida real; opcao reduzir movimento liga/desliga por mouse");

            Button StatusButton(string name) => Descendants(status).OfType<Button>().Single(button => button.Name == name);
            Button RouteButton(string name) => Descendants(windows.Single(window=>window.Name=="Instrument2")).OfType<Button>().Single(button=>button.Name==name);
            var previous = StatusButton("StatusPreviousMember"); var next = StatusButton("StatusNextMember");
            await Click(previous); Require(_view.SelectedMemberIndex == 2, "anterior nao circulou do Guarda ao Alquimista");
            await Click(next); Require(_view.SelectedMemberIndex == 0, "proximo nao circulou ao Guarda");
            next.GrabFocus(); bool pausedBeforeNavigation = _view.Simulation.Paused;
            await Key(status, Godot.Key.Space);
            Require(_view.SelectedMemberIndex == 1 && _view.Simulation.Paused == pausedBeforeNavigation, "Espaco deve ativar seletor sem pausar a rota");
            var panel = _view.StatusPanel;
            var statusName = Descendants(status).OfType<Label>().Single(label => label.Name == "StatusMemberName");
            Require(statusName.Text == _view.Simulation.Party[1].Name && panel.SelectedMember == _view.SelectedMemberIndex, "identidade do Status incoerente");
            var portrait = Descendants(panel).OfType<TextureRect>().Single(texture => texture.Name == "StatusPortrait");
            Require(portrait.Texture == panel.MemberRows[1].PortraitView.Texture, "retrato difere do integrante selecionado");
            await Click(StatusButton("OpenStatusDetails")); Require(panel.DetailsVisible, "detalhes nao abriram");
            var interval = StatusButton("StatusAttribute-interval").GetChildren().OfType<MarginContainer>().Single().GetChild<HBoxContainer>(0).GetChild<Label>(1);
            Require(interval.Text == $"{_view.Simulation.GetAttackInterval(1):0.##} s", "intervalo nao veio da simulacao");
            await Click(StatusButton("StatusAttribute-defense"));
            Require(panel.SelectedAttribute == "defense" && panel.DetailsVisible, "atributo nao abriu explicacao");
            var explanation = Descendants(panel).OfType<PanelContainer>().Single(p => p.Name == "StatusAttributeExplanation");
            Require(explanation.Visible && Descendants(explanation).OfType<Label>().Any(label => label.Text.Contains($"{_view.Simulation.Party[1].Defense:0.#}")), "composicao nao corresponde ao Pistoleiro");
            await Click(next); Require(panel.SelectedMember == 2 && panel.SelectedAttribute == "defense" && statusName.Text == _view.Simulation.Party[2].Name, "trocar integrante perdeu contexto do detalhe");
            Require(Descendants(explanation).OfType<Label>().Any(label => label.Text.Contains($"{_view.Simulation.Party[2].Defense:0.#}")), "explicacao nao atualizou ao trocar integrante");
            await Key(status, Godot.Key.Escape); Require(panel.DetailsVisible && panel.SelectedAttribute.Length == 0 && status.Visible, "Escape nao retornou do atributo aos detalhes");
            await Key(status, Godot.Key.Escape); Require(!panel.DetailsVisible && status.Visible, "Escape nao retornou ao resumo");
            var detailTrigger = StatusButton("OpenStatusDetails"); await Reveal(detailTrigger); detailTrigger.GrabFocus();
            await Key(status, Godot.Key.Enter); Require(panel.DetailsVisible, "Enter nao abriu detalhes");
            for (int i=0; i<8; i++)
            {
                await Key(status, Godot.Key.Tab);
                Require(status.GuiGetFocusOwner() is Control focus && focus.IsVisibleInTree(), "Tab focou controle oculto");
            }
            await Click(StatusButton("BackStatusSummary")); Require(!panel.DetailsVisible, "botao voltar nao retornou ao resumo");
            _checks.Add("V18: anterior/proximo circulam; Espaco/Enter ativam sem pausar; resumo/detalhe/atributo usam dados reais; Escape volta em camadas; Tab ignora ocultos");
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
                await Click(RouteButton("RoutePause"));
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
                await Click(StatusButton("OpenStatusDetails"));
                foreach (var body in Descendants(status).OfType<ScrollContainer>()) body.ScrollVertical = 0;
                await Frames(3); await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                Require(status.GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory, "status-details.png")) == Error.Ok, "captura detalhes Status falhou");
                await Click(StatusButton("StatusAttribute-dps"));
                foreach (var body in Descendants(status).OfType<ScrollContainer>()) body.ScrollVertical = 0;
                await Frames(3); await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                Require(status.GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory, "status-attribute.png")) == Error.Ok, "captura atributo Status falhou");
                await Key(status, Godot.Key.Escape); await Key(status, Godot.Key.Escape);
                Require(GetViewport().GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory, "input-stage.png")) == Error.Ok, "captura de palco falhou");
                foreach (var (selector, image) in new[] { ("CURA", "hero-healer"), ("DPS", "hero-gunner"), ("TANK", "hero-guard") })
                {
                    await Click(Find(selector));
                    await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                    Require(heroWindow.GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory, image+".png"))==Error.Ok, "captura do integrante falhou");
                }
                await Click(RouteButton("RoutePause"));
            }
            await Click(RouteButton("RouteRetreat"));
            Require(_view.Simulation.Phase == ExpeditionPhase.Retreated, "mouse nao recuou");
            await Click(RouteButton("ResetRoute"));
            Require(_view.Simulation.Phase == ExpeditionPhase.March && _view.Simulation.Victories == 0 && _view.Simulation.ShieldCooldown == 0, "mouse nao reiniciou");
            Require(_view.Stage.Hud.ActiveFeedbackCount == 0, "reset manteve numeros de combate antigos");
            Require(!Descendants(_view.Stage).OfType<Label3D>().Any(l => l.Name == "EventAmount"), "numero Label3D antigo ainda existe");
            _checks.Add("mouse recua/reinicia e numeros 3D antigos ausentes");

            var report = new { passed = true, scale = BrassUi.Scale, display = DisplayServer.GetName(), overview = _view.Overview,
                inputMethod = _nativePointer ? "Win32 automated mouse with cursor restoration; Godot PushInput keyboard; not human physical input or direct Pressed" : "Godot Viewport.PushInput mouse/key; not direct Pressed or physical input",
                fontFamily = "Source Sans 3", baseFaceMetadata = BrassUi.BodyFont.GetFontName(), actualWeights, digitWidths, nativeIds, checks = _checks,
                dossier = new { heroSpreadColumns = spread.Columns, heroResizeColumnModes, statusPortraits = 3, sharedRulers = 0, bagCapacity = 21, ownedItems = _view.Simulation.Rewards.Inventory.Count },
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
