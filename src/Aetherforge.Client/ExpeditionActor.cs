using Godot;
using System;
using System.Collections.Generic;

// Technical, articulated prototype translated from the three Brass v12 paintings.
// No imported character, historical mock-up, gameplay clock or combat rules live here.
public partial class ExpeditionActor : Node3D
{
    public string Kind { get; private set; } = "guard";
    public string CurrentPose { get; private set; } = "idle";
    public float Height { get; private set; } = 2.1f;
    public Vector3 AimPoint => GlobalPosition + Vector3.Up * Height * .64f;
    public Vector3 Muzzle => _weapon != null ? _weapon.GlobalPosition + new Vector3(_enemy ? -.55f : .7f, 0, 0)
        : Kind == "watcher" ? _head.GlobalPosition + Vector3.Left * .16f
        : Kind == "drone" ? _torso.GlobalPosition + new Vector3(-.12f, -.44f, -.06f) : AimPoint;

    private ExpeditionGeometry _g = null!;
    private Node3D _visual = null!, _torso = null!, _head = null!, _weapon = null!, _leftArm = null!, _rightArm = null!, _leftForearm = null!, _rightForearm = null!;
    private readonly List<Node3D> _hips = new();
    private readonly List<Node3D> _knees = new();
    private readonly List<Node3D> _feet = new();
    private readonly List<Node3D> _rotors = new();
    private readonly List<Node3D> _coat = new();
    private readonly List<Node3D> _tail = new();
    private readonly List<MeshInstance3D> _meshes = new();
    private readonly Dictionary<Node3D, Transform3D> _rest = new();
    private bool _enemy, _flashing;
    private double _attackAt = -100, _hurtAt = -100, _healAt = -100, _shieldAt = -100, _deathAt = -1;
    private int _seed;
    private float _attackReach;

    internal void Build(ExpeditionGeometry geometry, string kind, int seed)
    {
        _g = geometry; Kind = kind; _seed = seed;
        _enemy = kind != "guard" && kind != "gunner" && kind != "healer";
        _visual = Pivot(this, "Visual", Vector3.Zero);
        // Enemy models use the same local forward (+X), then face the expedition.
        if (_enemy) _visual.Rotation = new Vector3(0, Mathf.Pi, 0);
        switch (kind)
        {
            case "guard": case "gunner": case "healer": Human(); break;
            case "rat": Rat(); break;
            case "drone": Drone(); break;
            case "watcher": Watcher(); break;
            case "sentry": Sentry(); break;
            default: Worker(kind == "foreman"); break;
        }
        // Cheap contact patch instead of many dynamic shadow maps in the compact viewport.
        _g.Part(this, "sphere", "soot", new Vector3(0, .012f, 0),
            new Vector3(kind == "foreman" ? 1.45f : kind == "rat" ? 1.35f : .80f, .015f, kind == "foreman" ? 1.1f : .52f));
        Remember(this);
    }

    private Node3D Pivot(Node3D parent, string name, Vector3 at)
    {
        var node = new Node3D { Name = name, Position = at };
        parent.AddChild(node); return node;
    }

    private MeshInstance3D Part(Node3D parent, string mesh, string material, Vector3 at, Vector3 size, Vector3 rotation = default)
    {
        var node = _g.Part(parent, mesh, material, at, size, rotation);
        _meshes.Add(node); return node;
    }

    private void Rod(Node3D parent, string material, Vector3 from, Vector3 to, float width)
    {
        var node = _g.Rod(parent, material, from, to, width);
        _meshes.Add(node);
    }

    private void Ball(Node3D parent, string mat, Vector3 at, Vector3 size) => Part(parent, "sphere", mat, at, size);
    private void Box(Node3D parent, string mat, Vector3 at, Vector3 size, Vector3 rotation = default) => Part(parent, "box", mat, at, size, rotation);

    private void Human()
    {
        bool guard = Kind == "guard", healer = Kind == "healer";
        string cloth = healer ? "linen" : "tobacco";
        Height = guard || healer ? 2.23f : 2.14f;
        _torso = Pivot(_visual, "Chest", new Vector3(0, 1.34f, 0));
        _torso.Rotation = new Vector3(0, -.28f, 0);
        // Elliptic tapered volumes, collars and separate cloth panels give dressed adult silhouettes.
        Part(_torso, "bodice", cloth, new Vector3(-.015f, .02f, 0), new Vector3(.40f, .66f, guard ? .69f : .52f));
        Ball(_visual, cloth, new Vector3(0, .98f, 0), new Vector3(.36f, .27f, .43f));
        Part(_torso, "cylinder", "leather", new Vector3(.005f, -.23f, 0), new Vector3(.43f, .10f, guard ? .67f : .53f));
        Box(_torso, "brass", new Vector3(.225f, -.23f, .04f), new Vector3(.035f, .105f, .14f));
        Box(_torso, "iron", new Vector3(.247f, -.23f, .04f), new Vector3(.012f, .061f, .085f));
        // Coat split at the front; the two independent hems follow each hip.
        for (int i = 0; i < 2; i++)
        {
            float z = i == 0 ? .19f : -.19f;
            var hem = Pivot(_visual, "Coat" + i, new Vector3(-.025f, 1.08f, z));
            float length = guard ? .39f : .77f;
            Part(hem, "flare", "wine", new Vector3(.015f, -length * .50f, 0), new Vector3(.37f, length, .31f));
            Part(hem, "flare", cloth, new Vector3(-.035f, -length * .50f, 0), new Vector3(.39f, length, .33f));
            _coat.Add(hem);
            HumanLeg(i, z * .86f, guard);
        }
        // Functional pouches, seams and leather shoulder straps.
        Box(_torso, "leather", new Vector3(-.12f, -.32f, .34f), new Vector3(.23f, .29f, .16f));
        Box(_torso, "brass", new Vector3(-.01f, -.29f, .37f), new Vector3(.025f, .07f, .055f));
        if (guard)
        {
            Part(_torso, "bodice", "wine", new Vector3(.165f, .05f, 0), new Vector3(.17f, .54f, .67f));
            for (int side = -1; side <= 1; side += 2)
            {
                Rod(_torso, "brass", new Vector3(.25f, -.18f, side * .29f), new Vector3(.25f, .31f, side * .29f), .025f);
                for (int y = 0; y < 3; y++) Ball(_torso, "brass", new Vector3(.27f, -.14f + y * .20f, side * .29f), Vector3.One * .045f);
                Box(_torso, "leather", new Vector3(.06f, .32f, side * .25f), new Vector3(.32f, .075f, .10f));
            }
            Box(_torso, "iron", new Vector3(-.31f, .03f, 0), new Vector3(.16f, .66f, .51f));
            Box(_torso, "leather", new Vector3(-.40f, .03f, 0), new Vector3(.04f, .57f, .43f));
            Rod(_torso, "brass", new Vector3(-.43f, -.19f, -.20f), new Vector3(-.43f, .27f, -.20f), .028f);
        }
        else if (!healer)
        {
            Box(_torso, "linen", new Vector3(.215f, .035f, 0), new Vector3(.035f, .67f, .13f), new Vector3(.58f, 0, 0));
            Box(_torso, "brass", new Vector3(.24f, -.01f, .02f), new Vector3(.035f, .12f, .15f), new Vector3(.58f, 0, 0));
            Box(_torso, "wine", new Vector3(.22f, .28f, .015f), new Vector3(.04f, .22f, .075f));
        }
        else
        {
            Part(_torso, "flare", "wine", new Vector3(-.11f, .02f, 0), new Vector3(.56f, .72f, .69f));
            Part(_torso, "flare", "linen", new Vector3(-.14f, .045f, 0), new Vector3(.57f, .73f, .71f));
            Box(_torso, "leather", new Vector3(.24f, -.01f, .02f), new Vector3(.045f, .68f, .095f), new Vector3(-.53f, 0, 0));
            Box(_torso, "brass", new Vector3(.29f, .18f, .01f), new Vector3(.035f, .075f, .15f));
        }
        // Neck, ears, jaw, nose, brow and eyes are explicitly human under the helmet/hood.
        Part(_torso, "cylinder", "skin", new Vector3(.015f, .39f, 0), new Vector3(.14f, .17f, .17f));
        _head = Pivot(_torso, "Head", new Vector3(.025f, .57f, 0));
        Ball(_head, "skin", Vector3.Zero, new Vector3(.29f, .38f, .275f));
        Ball(_head, "skinShadow", new Vector3(.075f, -.10f, 0), new Vector3(.19f, .14f, .23f));
        Ball(_head, "skin", new Vector3(.163f, .005f, 0), new Vector3(.105f, .105f, .075f));
        for (int side = -1; side <= 1; side += 2)
        {
            Ball(_head, "skinShadow", new Vector3(0, -.008f, .147f * side), new Vector3(.075f, .11f, .055f));
            Ball(_head, "linen", new Vector3(.126f, .065f, side * .071f), new Vector3(.036f, .032f, .051f));
            Ball(_head, "soot", new Vector3(.147f, .066f, side * .073f), Vector3.One * .019f);
            Box(_head, "hair", new Vector3(.132f, .10f, side * .07f), new Vector3(.037f, .027f, .075f), new Vector3(side * .12f, 0, 0));
            if (guard || healer) Ball(_head, "hairGrey", new Vector3(.154f, -.063f, side * .041f), new Vector3(.04f, .045f, .09f));
        }
        Ball(_head, "hair", new Vector3(-.05f, .05f, 0), new Vector3(.25f, .33f, .30f));
        if (guard)
        {
            Part(_head, "taper", "iron", new Vector3(-.02f, .17f, 0), new Vector3(.38f, .28f, .36f));
            Box(_head, "iron", new Vector3(.07f, .09f, 0), new Vector3(.47f, .045f, .41f));
            Box(_head, "brass", new Vector3(.08f, .28f, 0), new Vector3(.23f, .035f, .045f));
            Box(_head, "leather", new Vector3(-.015f, -.07f, .158f), new Vector3(.06f, .21f, .026f));
        }
        else if (healer)
        {
            Ball(_head, "linen", new Vector3(-.075f, .09f, 0), new Vector3(.36f, .46f, .41f));
            // Open hood: rear shell and two folds, with the face remaining uncovered.
            for (int side = -1; side <= 1; side += 2)
                Rod(_head, "linen", new Vector3(.06f, .25f, side * .09f), new Vector3(.04f, -.22f, side * .21f), .10f);
        }
        else
        {
            for (int j = 0; j < 4; j++) Ball(_head, "hair", new Vector3(-.06f + j * .037f, .17f, .11f), new Vector3(.17f, .14f, .18f));
        }
        HumanArms(guard, healer, cloth);
    }

    private void HumanLeg(int side, float z, bool heavy)
    {
        var hip = Pivot(_visual, "Hip" + side, new Vector3(0, .88f, z));
        Part(hip, "taper", "trousers", new Vector3(0, -.21f, 0), new Vector3(heavy ? .24f : .18f, .43f, .21f));
        var knee = Pivot(hip, "Knee", new Vector3(0, -.43f, 0));
        Ball(knee, "trousers", Vector3.Zero, new Vector3(.18f, .18f, .20f));
        Part(knee, "taper", "trousers", new Vector3(0, -.20f, 0), new Vector3(.16f, .40f, .17f));
        var foot = Pivot(knee, "Ankle", new Vector3(0, -.43f, 0));
        Part(foot, "taper", "leather", new Vector3(0, .10f, 0), new Vector3(.18f, .30f, .20f));
        Ball(foot, "leather", new Vector3(.08f, -.015f, 0), new Vector3(.36f, .17f, .22f));
        Box(foot, "soot", new Vector3(.08f, -.075f, 0), new Vector3(.34f, .035f, .23f));
        if (heavy)
        {
            Box(foot, "brass", new Vector3(.087f, .16f, 0), new Vector3(.025f, .04f, .22f));
            Box(foot, "iron", new Vector3(.20f, .025f, 0), new Vector3(.13f, .07f, .23f));
        }
        _hips.Add(hip); _knees.Add(knee); _feet.Add(foot);
    }

    private void HumanArms(bool guard, bool healer, string cloth)
    {
        for (int i = 0; i < 2; i++)
        {
            float side = i == 0 ? 1 : -1;
            var arm = Pivot(_torso, i == 0 ? "ShieldShoulder" : "WeaponShoulder", new Vector3(0, .25f, side * (guard ? .38f : .29f)));
            Ball(arm, cloth, Vector3.Zero, new Vector3(.26f, .29f, .25f));
            Part(arm, "taper", cloth, new Vector3(0, -.17f, 0), new Vector3(.19f, .36f, .21f));
            var elbow = Pivot(arm, "Elbow", new Vector3(0, -.34f, 0));
            Ball(elbow, cloth, Vector3.Zero, Vector3.One * .18f);
            Part(elbow, "taper", healer ? "linen" : "leather", new Vector3(0, -.15f, 0), new Vector3(.16f, .29f, .17f));
            Ball(elbow, healer ? "skin" : "leather", new Vector3(.015f, -.32f, 0), new Vector3(.14f, .16f, .12f));
            if (i == 0) { _leftArm = arm; _leftForearm = elbow; }
            else { _rightArm = arm; _rightForearm = elbow; }
        }
        _weapon = Pivot(_rightForearm, "Weapon", new Vector3(.05f, -.31f, 0));
        if (guard)
        {
            // The foreground arm carries a long rectangular shield with an ochre stripe.
            var shield = Pivot(_leftForearm, "RectangularShield", new Vector3(.14f, -.17f, .12f));
            shield.Rotation = new Vector3(0, -.55f, 0);
            Box(shield, "brass", Vector3.Zero, new Vector3(.19f, 1.20f, .66f));
            Box(shield, "iron", new Vector3(.105f, 0, 0), new Vector3(.035f, 1.12f, .58f));
            Box(shield, "ochre", new Vector3(.127f, 0, .035f), new Vector3(.012f, 1.10f, .14f));
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                    Ball(shield, "brass", new Vector3(.14f, y * .49f, z * .25f), Vector3.One * .058f);
            Rod(_weapon, "wood", new Vector3(0, -.10f, 0), new Vector3(0, .48f, 0), .065f);
            Box(_weapon, "iron", new Vector3(0, .46f, 0), new Vector3(.29f, .23f, .21f));
            Box(_weapon, "brass", new Vector3(.15f, .46f, 0), new Vector3(.035f, .20f, .18f));
        }
        else if (!healer)
        {
            Box(_weapon, "wood", new Vector3(-.12f, 0, 0), new Vector3(.58f, .13f, .10f));
            Box(_weapon, "brass", new Vector3(.10f, .045f, 0), new Vector3(.22f, .15f, .13f));
            Rod(_weapon, "iron", new Vector3(.13f, .06f, 0), new Vector3(.79f, .06f, 0), .055f);
            Rod(_weapon, "wood", new Vector3(.16f, -.025f, 0), new Vector3(.57f, -.025f, 0), .075f);
            Box(_weapon, "iron", new Vector3(.72f, .105f, 0), new Vector3(.04f, .06f, .025f));
            Box(_weapon, "iron", new Vector3(.03f, -.095f, 0), new Vector3(.12f, .06f, .05f));
        }
        else
        {
            Rod(_weapon, "brass", new Vector3(-.10f, 0, 0), new Vector3(.33f, 0, 0), .12f);
            Rod(_weapon, "iron", new Vector3(.32f, 0, 0), new Vector3(.58f, 0, 0), .027f);
            // Exactly one jade capsule, held in a small brass cradle.
            Part(_weapon, "cylinder", "jade", new Vector3(.10f, .11f, 0), new Vector3(.10f, .23f, .10f), new Vector3(0, 0, Mathf.Pi / 2));
            for (int i = 0; i < 2; i++) Box(_weapon, "brass", new Vector3(.01f + i * .18f, .10f, 0), new Vector3(.035f, .15f, .13f));
            Rod(_weapon, "leather", Vector3.Zero, new Vector3(-.03f, -.17f, 0), .065f);
        }
    }

    private void Rat()
    {
        Height = .88f;
        _torso = Pivot(_visual, "Spine", new Vector3(0, .46f, 0));
        Ball(_torso, "fur", new Vector3(-.18f, 0, 0), new Vector3(1.10f, .66f, .55f));
        _head = Pivot(_torso, "Snout", new Vector3(.44f, -.09f, 0));
        Part(_head, "taper", "fur", new Vector3(.15f, 0, 0), new Vector3(.31f, .55f, .30f), new Vector3(0, 0, -Mathf.Pi / 2));
        Ball(_head, "skinShadow", new Vector3(.41f, -.02f, 0), new Vector3(.10f, .09f, .12f));
        for (int s = -1; s <= 1; s += 2)
        {
            Ball(_head, "skinShadow", new Vector3(-.025f, .20f, .19f * s), new Vector3(.20f, .26f, .08f));
            Ball(_head, "soot", new Vector3(.16f, .10f, .135f * s), Vector3.One * .045f);
            for (int i = 0; i < 2; i++)
            {
                var leg = Pivot(_visual, "Paw" + s + i, new Vector3(i == 0 ? -.40f : .35f, .30f, s * .21f));
                Rod(leg, "fur", Vector3.Zero, new Vector3(-.08f, -.21f, s * .05f), .10f);
                Rod(leg, "skinShadow", new Vector3(-.08f, -.21f, s * .05f), new Vector3(.12f, -.28f, s * .09f), .055f);
                for (int claw = 0; claw < 3; claw++) Rod(leg, "bone", new Vector3(.11f, -.28f, s * .09f + claw * .025f), new Vector3(.20f, -.29f, s * .09f + claw * .025f), .018f);
                _hips.Add(leg);
            }
        }
        var tail = Pivot(_torso, "TailRoot", new Vector3(-.66f, -.05f, 0));
        for (int i = 0; i < 8; i++)
        {
            var link = Pivot(tail, "Tail" + i, i == 0 ? Vector3.Zero : new Vector3(-.20f, 0, 0));
            Rod(link, "skinShadow", Vector3.Zero, new Vector3(-.20f, .025f, 0), .06f - i * .005f);
            _tail.Add(link); tail = link;
        }
        for (int i = 0; i < 4; i++) Part(_torso, "taper", "jadeDull", new Vector3(-.42f + i * .21f, .29f, .02f), new Vector3(.13f, .23f, .14f), new Vector3(0, 0, .35f));
    }

    private void Drone()
    {
        Height = 2.5f;
        _torso = Pivot(_visual, "Bell", new Vector3(0, 1.77f, 0));
        Part(_torso, "flare", "wine", Vector3.Zero, new Vector3(.62f, .77f, .62f));
        Part(_torso, "ring", "brass", new Vector3(0, -.38f, 0), new Vector3(.73f, .13f, .73f));
        Ball(_torso, "iron", new Vector3(.02f, -.41f, 0), new Vector3(.30f, .22f, .30f));
        Ball(_torso, "jadeDull", new Vector3(.12f, -.44f, .06f), Vector3.One * .18f);
        for (int s = -1; s <= 1; s += 2)
        {
            Rod(_torso, "brass", new Vector3(0, .24f, 0), new Vector3(0, .36f, s * .65f), .09f);
            Part(_torso, "ring", "wine", new Vector3(0, .38f, s * .68f), new Vector3(.93f, .25f, .93f));
            var rotor = Pivot(_torso, "Rotor" + s, new Vector3(0, .38f, s * .68f));
            Box(rotor, "iron", Vector3.Zero, new Vector3(.79f, .025f, .09f));
            Box(rotor, "iron", Vector3.Zero, new Vector3(.09f, .025f, .79f));
            Ball(rotor, "brass", Vector3.Zero, Vector3.One * .12f); _rotors.Add(rotor);
        }
    }

    private void Watcher()
    {
        Height = 2.65f;
        _torso = Pivot(_visual, "OpticMast", new Vector3(0, 1.40f, 0));
        Rod(_torso, "iron", new Vector3(0, -.44f, 0), new Vector3(0, .58f, 0), .11f);
        _head = Pivot(_torso, "PorcelainHead", new Vector3(0, .77f, 0));
        Ball(_head, "linen", Vector3.Zero, new Vector3(.28f, .77f, .29f));
        Box(_head, "soot", new Vector3(.135f, -.02f, 0), new Vector3(.035f, .35f, .10f));
        Box(_head, "ember", new Vector3(.16f, -.05f, 0), new Vector3(.02f, .25f, .033f));
        Part(_torso, "cylinder", "brass", new Vector3(-.13f, .15f, .15f), new Vector3(.18f, .43f, .18f));
        for (int i = 0; i < 3; i++)
        {
            float a = i * Mathf.Tau / 3;
            var leg = Pivot(_visual, "Tripod" + i, new Vector3(0, 1.12f, 0));
            var mid = new Vector3(Mathf.Cos(a) * .29f, -.52f, Mathf.Sin(a) * .29f);
            var foot = new Vector3(Mathf.Cos(a) * .66f, -1.04f, Mathf.Sin(a) * .66f);
            Rod(leg, "linen", Vector3.Zero, mid, .10f); Rod(leg, "iron", mid, foot, .075f);
            Ball(leg, "brass", mid, Vector3.One * .15f);
            Box(leg, "iron", foot, new Vector3(.23f, .12f, .23f)); _hips.Add(leg);
        }
    }

    private void Sentry()
    {
        Height = 1.58f;
        _torso = Pivot(_visual, "PressureVessel", new Vector3(0, .96f, 0));
        Ball(_torso, "wine", Vector3.Zero, new Vector3(.97f, 1.04f, 1.03f));
        Part(_torso, "ring", "brass", Vector3.Zero, new Vector3(1.04f, .07f, 1.08f), new Vector3(0, 0, Mathf.Pi / 2));
        Box(_torso, "brass", new Vector3(.44f, .02f, 0), new Vector3(.11f, .68f, .70f));
        Box(_torso, "soot", new Vector3(.51f, .02f, 0), new Vector3(.04f, .59f, .61f));
        for (int i = 0; i < 6; i++) Box(_torso, "brass", new Vector3(.55f, -.24f + i * .10f, 0), new Vector3(.06f, .04f, .61f));
        Rod(_torso, "iron", new Vector3(-.10f, .41f, -.18f), new Vector3(-.10f, .79f, -.18f), .09f);
        for (int s = -1; s <= 1; s += 2)
        {
            var leg = Pivot(_visual, "PistonFoot" + s, new Vector3(0, .51f, s * .34f));
            Rod(leg, "brass", Vector3.Zero, new Vector3(.08f, -.34f, 0), .19f);
            Box(leg, "iron", new Vector3(.12f, -.40f, 0), new Vector3(.43f, .18f, .34f)); _hips.Add(leg);
            var arm = Pivot(_torso, "Ram" + s, new Vector3(.01f, .12f, s * .63f));
            Rod(arm, "iron", Vector3.Zero, new Vector3(.11f, -.23f, s * .09f), .16f);
            Box(arm, "wine", new Vector3(.24f, -.31f, s * .08f), new Vector3(.43f, .42f, .35f));
            Box(arm, "brass", new Vector3(.46f, -.31f, s * .08f), new Vector3(.05f, .38f, .34f));
            if (s == 1) _leftArm = arm; else _rightArm = arm;
        }
    }

    private void Worker(bool foreman)
    {
        Height = foreman ? 2.87f : 2.25f;
        _torso = Pivot(_visual, "BoilerChest", new Vector3(0, foreman ? 1.87f : 1.48f, 0));
        if (foreman)
        {
            // Broad pressure chest must remain heavy in the side-view silhouette, not only in depth.
            Box(_torso, "wine", Vector3.Zero, new Vector3(1.02f, .87f, 1.30f));
            Box(_torso, "iron", new Vector3(-.38f, .13f, 0), new Vector3(.30f, .66f, 1.39f));
            Part(_torso, "taper", "wine", new Vector3(.44f, -.04f, 0), new Vector3(.26f, .84f, .96f));
            Box(_torso, "soot", new Vector3(.59f, -.10f, 0), new Vector3(.05f, .51f, .39f));
            Box(_torso, "soot", new Vector3(.04f, -.10f, -.675f), new Vector3(.55f, .51f, .035f));
            for (int i = 0; i < 3; i++)
            {
                Rod(_torso, "ember", new Vector3(.63f, -.31f, -.13f + i * .13f), new Vector3(.63f, .10f, -.13f + i * .13f), .044f);
                Rod(_torso, "ember", new Vector3(-.15f + i * .18f, -.31f, -.70f), new Vector3(-.15f + i * .18f, .10f, -.70f), .037f);
            }
            Part(_torso, "cylinder", "brass", new Vector3(0, .57f, 0), new Vector3(.18f, .40f, .18f));
            Part(_torso, "cylinder", "ember", new Vector3(.01f, .58f, 0), new Vector3(.12f, .26f, .12f));
            for (int i = 0; i < 5; i++) Box(_torso, "brass", new Vector3(.10f, .40f, -.51f + i * .25f), new Vector3(.42f, .09f, .065f));
        }
        else
        {
            Part(_torso, "taper", "wine", Vector3.Zero, new Vector3(.36f, .58f, .46f));
            Part(_torso, "flare", "wine", new Vector3(0, -.45f, 0), new Vector3(.44f, .41f, .57f));
            Box(_torso, "brass", new Vector3(.23f, -.45f, .02f), new Vector3(.035f, .37f, .42f));
            _head = Pivot(_torso, "CameraHead", new Vector3(.04f, .49f, 0));
            Box(_head, "ochre", Vector3.Zero, new Vector3(.51f, .27f, .28f));
            Box(_head, "iron", new Vector3(.28f, 0, 0), new Vector3(.065f, .23f, .25f));
            Ball(_head, "jadeDull", new Vector3(.32f, 0, 0), new Vector3(.06f, .11f, .11f));
            Part(_torso, "cylinder", "iron", new Vector3(-.23f, .25f, 0), new Vector3(.23f, .60f, .30f));
        }
        for (int s = -1; s <= 1; s += 2)
        {
            float half = foreman ? .49f : .36f;
            var leg = Pivot(_visual, "PistonHip" + s, new Vector3(0, half * 2 + .13f, s * (foreman ? .39f : .22f)));
            Rod(leg, "iron", Vector3.Zero, new Vector3(-.10f, -half, 0), foreman ? .23f : .13f);
            Ball(leg, "brass", new Vector3(-.10f, -half, 0), Vector3.One * (foreman ? .28f : .19f));
            var knee = Pivot(leg, "PistonKnee", new Vector3(-.10f, -half, 0));
            Rod(knee, "iron", Vector3.Zero, new Vector3(.10f, -half + .08f, 0), foreman ? .19f : .10f);
            Box(knee, "iron", new Vector3(.15f, -half + .06f, 0), new Vector3(foreman ? .60f : .44f, .18f, foreman ? .43f : .29f));
            _hips.Add(leg); _knees.Add(knee);
            var arm = Pivot(_torso, "ToolShoulder" + s, new Vector3(0, .20f, s * (foreman ? .72f : .34f)));
            Ball(arm, "brass", Vector3.Zero, Vector3.One * (foreman ? .32f : .19f));
            Rod(arm, "iron", Vector3.Zero, new Vector3(.05f, -.43f, 0), foreman ? .24f : .12f);
            var elbow = Pivot(arm, "ToolElbow", new Vector3(.05f, -.43f, 0));
            Part(elbow, "taper", "wine", new Vector3(.04f, -.20f, 0), new Vector3(foreman ? .36f : .18f, .43f, foreman ? .36f : .18f));
            if (s == 1)
            {
                if (foreman)
                {
                    for (int finger = -1; finger <= 1; finger += 2)
                        Rod(elbow, "brass", new Vector3(.04f, -.38f, finger * .14f), new Vector3(.24f, -.62f, finger * .10f), .085f);
                }
                else
                {
                    // Open hook assembled as a curved sequence, never a closed ring.
                    Vector3 prev = new Vector3(.02f, -.31f, 0);
                    for (int j = 1; j <= 9; j++)
                    {
                        float a = j / 9f * Mathf.Pi * 1.5f;
                        var next = new Vector3(.02f + .19f * (1 - Mathf.Cos(a)), -.31f - .25f * Mathf.Sin(a), 0);
                        Rod(elbow, "iron", prev, next, .07f * (1 - j / 14f)); prev = next;
                    }
                }
                _leftArm = arm; _leftForearm = elbow;
            }
            else
            {
                if (!foreman)
                    for (int finger = -1; finger <= 1; finger += 2)
                        Rod(elbow, "brass", new Vector3(.04f, -.38f, finger * .09f), new Vector3(.19f, -.56f, finger * .07f), .055f);
                _rightArm = arm; _rightForearm = elbow;
            }
        }
        if (foreman)
        {
            // In an enemy's reversed facing, the foreground arm is the negative-Z arm.
            // Put the hammer there so it does not disappear behind the boiler chest.
            var hammer = Pivot(_rightForearm, "ForegroundHammer", new Vector3(.15f, -.43f, 0));
            Box(hammer, "brass", Vector3.Zero, new Vector3(.83f, .52f, .63f));
            Box(hammer, "wine", new Vector3(.02f, 0, 0), new Vector3(.73f, .40f, .66f));
            Box(hammer, "iron", new Vector3(.44f, 0, 0), new Vector3(.10f, .50f, .64f));
        }
    }

    private void Remember(Node node)
    {
        if (node is Node3D n && node is not MeshInstance3D) _rest[n] = n.Transform;
        foreach (Node child in node.GetChildren()) Remember(child);
    }

    public void OnEvent(string kind, double at)
    {
        switch (kind)
        {
            case "attack": _attackAt = at; break;
            case "hurt": _hurtAt = at; break;
            case "heal": _healAt = at; break;
            case "shield": _shieldAt = at; break;
            case "kill": if (_deathAt < 0) _deathAt = at; break;
        }
    }

    public void AimAttack(Vector3 target)
    {
        _attackReach = Mathf.Clamp(Mathf.Abs(target.X - GlobalPosition.X) - .85f, 0, 4.2f);
    }

    public void ResetPose()
    {
        _attackAt = _hurtAt = _healAt = _shieldAt = -100; _deathAt = -1;
        foreach (var mesh in _meshes) mesh.MaterialOverlay = null;
        _flashing = false; Visible = true;
    }

    /// <summary>Absolute sampled poses: neither wall time nor delta advances an animation.</summary>
    public void Pose(double elapsed, double distance, bool marching, bool alive, bool shielded, bool settled = false)
    {
        foreach (var pair in _rest)
            if (pair.Key != this) pair.Key.Transform = pair.Value;
        if (!alive && _deathAt < 0) _deathAt = elapsed;
        float t = (float)elapsed;
        float attackAge = (float)(elapsed - _attackAt);
        float action = settled ? 0 : Envelope(attackAge, .64f);
        float hurt = settled ? 0 : Envelope((float)(elapsed - _hurtAt), .26f);
        CurrentPose = !alive ? "fall" : action > 0 ? "attack" : marching ? "walk" : "idle";
        float breath = Mathf.Sin(t * 2.4f + _seed) * .009f;
        _torso.Position += new Vector3(0, breath, 0);
        if (!_enemy)
        {
            for (int i = 0; i < 2; i++)
            {
                // 60% stance: foot X decreases at exactly the root's rate of travel.
                // 40% swing: lift and return. This is distance-driven two-bone foot planting.
                float phase = Fract((float)(distance / 1.05) + i * .5f);
                float x = 0, lift = 0;
                if (marching && alive)
                {
                    if (phase < .6f) x = .315f - phase * 1.05f;
                    else { float p = (phase - .6f) / .4f; x = -.315f + .63f * Smooth(p); lift = Mathf.Sin(p * Mathf.Pi) * .15f; }
                }
                SolveLeg(i, x, -.7875f + lift);
                if (i < _coat.Count) _coat[i].Rotation = new Vector3(0, 0, marching && alive ? Mathf.Sin(phase * Mathf.Tau) * .075f : breath * 1.8f);
            }
            float swing = marching && alive ? Mathf.Sin((float)(distance / 1.05) * Mathf.Tau) * .30f : .025f * Mathf.Sin(t * 1.8f);
            _leftArm.Rotation = new Vector3(0, 0, swing * (Kind == "guard" ? .28f : .50f));
            _rightArm.Rotation = new Vector3(0, 0, -swing);
            if (Kind != "guard")
            {
                _rightArm.Rotation = new Vector3(0, -.12f, .46f + swing * .12f);
                _rightForearm.Rotation = new Vector3(0, 0, .94f);
                _leftArm.Rotation = new Vector3(.65f, 0, .62f + swing * .10f);
                _leftForearm.Rotation = new Vector3(.35f, 0, .64f);
                // Counter-rotate weapon so the barrel remains horizontal in world space.
                _weapon.Rotation = new Vector3(0, 0, -(_rightArm.Rotation.Z + _rightForearm.Rotation.Z));
            }
            else _rightForearm.Rotation = new Vector3(0, 0, .15f);
            if (Kind == "guard" && action > 0)
            {
                float wind = attackAge < .20f ? -Smooth(attackAge / .20f) * .70f : Mathf.Lerp(-.70f, 1.20f, Smooth((attackAge - .20f) / .19f));
                float recover = 1 - Smooth((attackAge - .39f) / .25f);
                _rightArm.Rotation = new Vector3(0, -.15f, wind * recover);
                _leftArm.Rotation = new Vector3(0, -.18f * action, .15f * action);
                _torso.Rotation += new Vector3(0, -.13f * action, -.10f * action);
            }
            else if (Kind == "gunner" && action > 0)
            {
                float recoil = Mathf.Exp(-attackAge * 13) * Mathf.Sin(attackAge * 29);
                _weapon.Position += new Vector3(-.16f * Math.Max(0, recoil), .025f * recoil, 0);
                _torso.Rotation += new Vector3(0, 0, .055f * action);
            }
            else if (Kind == "healer")
            {
                float infusion = settled ? 0 : Envelope((float)(elapsed - _healAt), .85f);
                _rightArm.Rotation += new Vector3(0, -.16f * infusion, .23f * infusion);
                _weapon.Rotation += new Vector3(0, 0, -.23f * infusion);
            }
            if (Kind == "guard" && (shielded || Envelope((float)(elapsed - _shieldAt), .7f) > 0))
            {
                _leftArm.Rotation = new Vector3(0, -.20f, .20f);
                _leftForearm.Rotation = new Vector3(0, 0, -.15f);
            }
        }
        else PoseEnemy(t, action, attackAge);

        if (Kind == "guard" || Kind == "scavenger" || Kind == "foreman" || Kind == "rat" || Kind == "sentry")
        {
            float lunge = settled || attackAge < .18f ? 0 : attackAge < .34f ? Smooth((attackAge - .18f) / .16f) : 1 - Smooth((attackAge - .34f) / .30f);
            _visual.Position += new Vector3((_enemy ? -1 : 1) * _attackReach * lunge, 0, 0);
        }

        _visual.Position += new Vector3((_enemy ? 1 : -1) * hurt * .12f, 0, 0);
        _torso.Rotation += new Vector3(0, 0, (_enemy ? -1 : 1) * hurt * .10f);
        bool flash = !settled && elapsed >= _hurtAt && elapsed - _hurtAt < .075 && alive;
        if (flash != _flashing)
        {
            foreach (var mesh in _meshes) mesh.MaterialOverlay = flash ? _g.Material("hit") : null;
            _flashing = flash;
        }
        if (!alive)
        {
            // The simulation stops Elapsed in terminal phases. Show the settled corpse there
            // rather than introducing an independent clock that would also break pause.
            float fall = settled ? 1 : Smooth((float)(elapsed - _deathAt) / .65f);
            _visual.Rotation += new Vector3(0, 0, (_enemy ? -1 : 1) * fall * 1.39f);
            _visual.Position += new Vector3((_enemy ? .23f : -.23f) * fall, -.06f * fall, 0);
            if (Kind == "drone") _visual.Position += Vector3.Down * fall * 1.25f;
            // Keep fallen silhouettes until the encounter is retired by the simulation.
        }
    }

    private void SolveLeg(int i, float x, float y)
    {
        const float length = .43f;
        float d = Mathf.Clamp(Mathf.Sqrt(x * x + y * y), .06f, length * 2 - .001f);
        float a = Mathf.Atan2(x, -y) + Mathf.Acos(d / (2 * length));
        float b = -2 * Mathf.Acos(d / (2 * length));
        _hips[i].Rotation = new Vector3(0, 0, a);
        _knees[i].Rotation = new Vector3(0, 0, b);
        _feet[i].Rotation = new Vector3(0, 0, -a - b);
    }

    private void PoseEnemy(float t, float action, float attackAge)
    {
        float windup = attackAge < .24f ? -Smooth(attackAge / .24f) : 1.4f * (1 - Smooth((attackAge - .24f) / .40f));
        switch (Kind)
        {
            case "rat":
                _torso.Position += new Vector3(.26f * action, .06f * action, 0);
                _head.Rotation = new Vector3(0, 0, -.22f * action);
                for (int i = 0; i < _hips.Count; i++) _hips[i].Rotation = new Vector3(0, 0, (i % 2 == 0 ? 1 : -1) * action * .28f);
                for (int i = 0; i < _tail.Count; i++) _tail[i].Rotation = new Vector3(0, Mathf.Sin(t * 2 + i * .55f) * .10f, -.10f);
                break;
            case "drone":
                _torso.Position += new Vector3(.15f * action, Mathf.Sin(t * 2.6f + _seed) * .07f - .12f * action, 0);
                _torso.Rotation = new Vector3(.035f * Mathf.Sin(t), 0, -.24f * action);
                foreach (var rotor in _rotors) rotor.Rotation = new Vector3(0, t * 25, 0);
                break;
            case "watcher":
                _head.Rotation = new Vector3(0, Mathf.Sin(t * .8f) * .20f, -.16f * action);
                _torso.Rotation = new Vector3(0, 0, -.11f * action);
                break;
            case "sentry":
                _leftArm.Rotation = new Vector3(0, 0, windup * .35f * action);
                _rightArm.Rotation = new Vector3(0, 0, windup * .35f * action);
                _torso.Position += new Vector3(.13f * action, -.035f * action, 0);
                break;
            default:
                _leftArm.Rotation = new Vector3(0, .18f * action, Kind == "foreman" ? -.18f * action : windup * .85f * action);
                _leftForearm.Rotation = new Vector3(0, 0, -.25f * action);
                _torso.Rotation = new Vector3(0, -.13f * action, -.13f * action);
                _rightArm.Rotation = new Vector3(0, 0, Kind == "foreman" ? windup * 1.10f * action : -.18f * action);
                break;
        }
    }

    private static float Fract(float v) => v - Mathf.Floor(v);
    private static float Smooth(float v) { v = Mathf.Clamp(v, 0, 1); return v * v * (3 - 2 * v); }
    private static float Envelope(float age, float duration) => age < 0 || age >= duration ? 0 : Mathf.Sin(Mathf.Pi * age / duration);
}

/// <summary>Stage-owned shared unit meshes and palette. Geometry is built once, never in Pose.</summary>
internal sealed class ExpeditionGeometry
{
    private readonly Dictionary<string, Mesh> _meshes = new();
    private readonly Dictionary<string, StandardMaterial3D> _materials = new();
    internal ExpeditionGeometry()
    {
        _meshes["box"] = new BoxMesh { Size = Vector3.One };
        _meshes["sphere"] = new SphereMesh { Radius = .5f, Height = 1, RadialSegments = 16, Rings = 8 };
        _meshes["cylinder"] = new CylinderMesh { TopRadius = .5f, BottomRadius = .5f, Height = 1, RadialSegments = 12 };
        _meshes["taper"] = new CylinderMesh { TopRadius = .40f, BottomRadius = .5f, Height = 1, RadialSegments = 10 };
        _meshes["bodice"] = new CylinderMesh { TopRadius = .5f, BottomRadius = .36f, Height = 1, RadialSegments = 12 };
        _meshes["flare"] = new CylinderMesh { TopRadius = .30f, BottomRadius = .5f, Height = 1, RadialSegments = 10 };
        _meshes["ring"] = new TorusMesh { InnerRadius = .40f, OuterRadius = .50f, Rings = 20, RingSegments = 6 };
        Add("soot", "241F1A"); Add("iron", "39332D", .65f); Add("steel", "696052", .65f);
        Add("tobacco", "40332B"); Add("trousers", "51483B"); Add("wine", "6C3A32", .15f);
        Add("brass", "B99B63", .70f); Add("ochre", "AA813C", .20f); Add("linen", "E4D8B7");
        Add("leather", "3B2C23"); Add("wood", "65462E"); Add("skin", "B48C6C"); Add("skinShadow", "8A624F");
        Add("hair", "30271F"); Add("hairGrey", "6D6252"); Add("fur", "4E4031"); Add("bone", "B2A487");
        Add("jadeDull", "97A581", .15f); Add("jade", "A5B58A", .20f, .35f); Add("ember", "DF9B4D", .15f, .75f);
        Add("wall", "514737"); Add("stone", "72624B"); Add("floor", "4B4235", .20f);
        Add("lamp", "F2C684", 0, .85f);
        var hit = Add("hit", "F2D7AD", 0, .3f);
        hit.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        hit.AlbedoColor = new Color(1, .79f, .54f, .28f);
        hit.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
    }
    private StandardMaterial3D Add(string key, string hex, float metal = 0, float emission = 0)
    {
        var color = new Color("#" + hex);
        var mat = new StandardMaterial3D { AlbedoColor = color, Metallic = metal, Roughness = metal > .3f ? .62f : .91f };
        if (emission > 0) { mat.EmissionEnabled = true; mat.Emission = color; mat.EmissionEnergyMultiplier = emission; }
        _materials[key] = mat; return mat;
    }
    internal Mesh Mesh(string key) => _meshes[key];
    internal StandardMaterial3D Material(string key) => _materials[key];
    internal MeshInstance3D Part(Node3D parent, string mesh, string material, Vector3 at, Vector3 size, Vector3 rotation = default)
    {
        var n = new MeshInstance3D { Mesh = Mesh(mesh), MaterialOverride = Material(material), Position = at, Scale = size, Rotation = rotation,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
        parent.AddChild(n); return n;
    }
    internal MeshInstance3D Rod(Node3D parent, string material, Vector3 from, Vector3 to, float width)
    {
        var direction = to - from;
        var n = Part(parent, "cylinder", material, (from + to) * .5f, new Vector3(width, direction.Length(), width));
        n.Quaternion = new Quaternion(Vector3.Up, direction.Normalized()); return n;
    }
}
