using Godot;

public static class WorldBuilder
{
    public static void Build(Node3D root, bool visuals)
    {
        Box(root, new Vector3(0, -.25f, 0), new Vector3(26, .5f, 22), "354544", visuals);
        Box(root, new Vector3(0, 1.2f, -11), new Vector3(26, 2.4f, .6f), "3b4c49", visuals);
        Box(root, new Vector3(-13, 1.2f, 0), new Vector3(.6f, 2.4f, 22), "3b4c49", visuals);
        Box(root, new Vector3(13, 1.2f, 0), new Vector3(.6f, 2.4f, 22), "3b4c49", visuals);
        Box(root, new Vector3(0, .6f, 11), new Vector3(26, 1.2f, .6f), "3b4c49", visuals);
        foreach (var x in new[] { -7f, 7f })
        {
            Box(root, new Vector3(x, .3f, -4), new Vector3(3, .6f, 3), "6f6147", visuals);
            Box(root, new Vector3(x, 1.6f, -4), new Vector3(2.4f, 3.2f, 2.4f), "a06e43", visuals);
            if (!visuals) continue;
            var boiler = new MeshInstance3D { Mesh = new CylinderMesh { TopRadius = 1.8f, BottomRadius = 1.8f, Height = .3f }, Position = new Vector3(x, 3.35f, -4), MaterialOverride = Material("bd915b") };
            root.AddChild(boiler);
            var stack = new MeshInstance3D { Mesh = new CylinderMesh { TopRadius = .4f, BottomRadius = .4f, Height = 3.2f }, Position = new Vector3(x, 4.9f, -4), MaterialOverride = Material("3f4d4a") };
            root.AddChild(stack);
            var glow = new OmniLight3D { Position = new Vector3(x, 1.3f, -2.6f), LightColor = new Color("efb86d"), LightEnergy = 1.8f, OmniRange = 5 };
            root.AddChild(glow);
        }
        if (!visuals) return;
        for (var x = -12; x <= 12; x += 2)
            root.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = new Vector3(.025f, .01f, 21) }, Position = new Vector3(x, .01f, 0), MaterialOverride = Material("50615a") });
        for (var z = -10; z <= 10; z += 2)
            root.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = new Vector3(25, .01f, .025f) }, Position = new Vector3(0, .01f, z), MaterialOverride = Material("50615a") });
        root.AddChild(new DirectionalLight3D { RotationDegrees = new Vector3(-55, -25, 0), LightColor = new Color("fff1d4"), LightEnergy = 1.25f, ShadowEnabled = true });
        root.AddChild(new WorldEnvironment { Environment = new Godot.Environment { BackgroundMode = Godot.Environment.BGMode.Color, BackgroundColor = new Color("14262a"), AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = new Color("9fbcbb"), AmbientLightEnergy = .65f } });
    }
    private static void Box(Node3D root, Vector3 position, Vector3 size, string color, bool visuals)
    {
        var body = new StaticBody3D { Position = position }; root.AddChild(body);
        body.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
        if (visuals) body.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = size }, MaterialOverride = Material(color) });
    }
    public static StandardMaterial3D Material(string color) => new() { AlbedoColor = new Color(color), Roughness = .82f };
}
