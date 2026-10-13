using Godot;
using System;
using System.Threading.Tasks;

public partial class SpriteBaker : Node
{
    public const int Width = 128, Height = 144, Frames = 4;
    public static readonly string[] Models = { "Knight", "Rogue", "Mage" };

    public override async void _Ready()
    {
        try { await Bake(); GetTree().Quit(); }
        catch (Exception error) { GD.PrintErr(error); GetTree().Quit(1); }
    }

    private async Task Bake()
    {
        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath("res://Assets/Generated"));
        var viewport = new SubViewport { Size = new Vector2I(Width, Height), TransparentBg = true, OwnWorld3D = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
        AddChild(viewport);
        var world = new Node3D(); viewport.AddChild(world);
        var environment = new WorldEnvironment { Environment = new Godot.Environment { BackgroundMode = Godot.Environment.BGMode.ClearColor, AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = Colors.White, AmbientLightEnergy = .65f } };
        world.AddChild(environment);
        world.AddChild(new DirectionalLight3D { RotationDegrees = new Vector3(-35, -35, 0), LightEnergy = 1.4f });
        var camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Size = 3.5f, Position = new Vector3(0, 2.6f, 5), Current = true };
        world.AddChild(camera); camera.LookAt(new Vector3(0, 1.1f, 0));
        foreach (var name in Models)
        {
            var packed = GD.Load<PackedScene>($"res://Assets/Originals/KayKit/{name}.glb");
            var model = packed.Instantiate<Node3D>(); world.AddChild(model);
            SelectAccessories(model, name);
            var player = FindAnimation(model);
            GD.Print($"BAKE {name}: {string.Join(",", player?.GetAnimationList() ?? Array.Empty<string>())}");
            var idle = FindClip(player, "Idle"); var walk = FindClip(player, "Walking", "Walk", "Running", "Run");
            var action = FindClip(player, name == "Knight" ? "1H_Melee_Attack_Slice_Horizontal" : name == "Rogue" ? "1H_Ranged_Shooting" : "Spellcast_Raise");
            var atlas = Image.CreateEmpty(Width * Frames * 3, Height * 8, false, Image.Format.Rgba8);
            for (var direction = 0; direction < 8; direction++)
            {
                model.Rotation = new Vector3(0, direction * Mathf.Pi / 4, 0);
                for (var pose = 0; pose < 3; pose++)
                {
                    var clip = pose == 0 ? idle : pose == 1 ? walk : action;
                    var length = .8;
                    if (player != null && clip != null)
                    {
                        player.Play(clip); player.Pause(); length = player.GetAnimation(clip).Length;
                    }
                    for (var frame = 0; frame < Frames; frame++)
                    {
                        if (clip != null) player!.Seek(length * frame / Frames, true);
                        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                        var image = viewport.GetTexture().GetImage();
                        image.Convert(Image.Format.Rgba8);
                        atlas.BlitRect(image, new Rect2I(0, 0, Width, Height), new Vector2I((pose * Frames + frame) * Width, direction * Height));
                    }
                }
            }
            var path = ProjectSettings.GlobalizePath($"res://Assets/Generated/{name}.png");
            if (atlas.SavePng(path) != Error.Ok) throw new InvalidOperationException("Não foi possível salvar " + path);
            model.QueueFree(); await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            GD.Print($"BAKE PASS {name}: 8 directions, idle/walk/action, 4 frames, {atlas.GetWidth()}x{atlas.GetHeight()}");
        }
    }

    private static AnimationPlayer? FindAnimation(Node node)
    {
        if (node is AnimationPlayer player) return player;
        foreach (var child in node.GetChildren()) { var found = FindAnimation(child); if (found != null) return found; }
        return null;
    }
    private static string? FindClip(AnimationPlayer? player, params string[] names)
    {
        if (player == null) return null;
        foreach (var name in names)
            foreach (var clip in player.GetAnimationList())
                if (clip.Equals(name, StringComparison.OrdinalIgnoreCase)) return clip;
        foreach (var name in names)
            foreach (var clip in player.GetAnimationList())
                if (clip.Contains(name, StringComparison.OrdinalIgnoreCase)) return clip;
        throw new InvalidOperationException("Pacote não possui animação necessária: " + string.Join("/", names));
    }

    private static void SelectAccessories(Node node, string model)
    {
        if (node is MeshInstance3D mesh)
        {
            var name = node.Name.ToString();
            mesh.Visible = name.StartsWith(model + "_", StringComparison.Ordinal)
                || (model == "Knight" && name is "1H_Sword" or "Round_Shield")
                || (model == "Rogue" && name == "1H_Crossbow")
                || (model == "Mage" && name == "2H_Staff");
        }
        foreach (var child in node.GetChildren()) SelectAccessories(child, model);
    }
}
