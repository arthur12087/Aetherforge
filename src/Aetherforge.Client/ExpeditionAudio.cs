using Aetherforge.Domain;
using Godot;
using System;
using System.Collections.Generic;

/// <summary>Original, synthesized prototype Foley. No sampled third-party material.</summary>
public partial class ExpeditionAudio : Node
{
    private ExpeditionSimulation _simulation = null!;
    private readonly Dictionary<string, AudioStreamWav> _clips = new();
    private readonly List<AudioStreamPlayer> _voices = new();
    private AudioStreamPlayer? _room;
    private int _lastEvent, _voice;
    private double _lastTime, _nextStep;
    public bool Muted { get; private set; }

    public void Bind(ExpeditionSimulation simulation) => _simulation = simulation;

    public override void _Ready()
    {
        if (DisplayServer.GetName() == "headless") { SetProcess(false); SetProcessUnhandledKeyInput(false); return; }
        _clips["rifle"] = Make(.24, (t, n) => n * Math.Exp(-t * 45) * .8 + Math.Sin(t * 510) * Math.Exp(-t * 25) * .3);
        _clips["metal"] = Make(.22, (t, n) => (Math.Sin(t * 2100) + Math.Sin(t * 3390) * .4 + n * .4) * Math.Exp(-t * 26) * .25);
        _clips["heal"] = Make(.64, (t, n) => (Math.Sin(2 * Math.PI * 330 * t) + .3 * Math.Sin(2 * Math.PI * 495 * t)) * Math.Sin(Math.PI * t / .64) * Math.Exp(-t * 3) * .22);
        _clips["shield"] = Make(.45, (t, n) => (Math.Sin(2 * Math.PI * (180 * t - 50 * t * t)) * .5 + n * .13) * Math.Exp(-t * 11));
        _clips["step"] = Make(.09, (t, n) => (n * .3 + Math.Sin(t * 610) * .4) * Math.Exp(-t * 60));
        _clips["steam"] = Make(.32, (t, n) => n * Math.Exp(-t * 14) * .3);
        for (var i = 0; i < 8; i++)
        {
            var player = new AudioStreamPlayer { Name = "Foley" + i, VolumeDb = -18 };
            AddChild(player); _voices.Add(player);
        }
        var room = Make(2, (t, n) => (Math.Sin(2 * Math.PI * 50 * t) * .19 + Math.Sin(2 * Math.PI * 100 * t) * .05 + n * .018));
        room.LoopMode = AudioStreamWav.LoopModeEnum.Forward; room.LoopBegin = 0; room.LoopEnd = 44100;
        _room = new AudioStreamPlayer { Name = "BoilerRoom", Stream = room, VolumeDb = -27 };
        AddChild(_room); _room.Play();
    }

    public override void _Process(double delta)
    {
        if (_simulation == null || _voices.Count == 0) return;
        if (_simulation.Elapsed < _lastTime)
        {
            _lastEvent = 0; _nextStep = 0;
            foreach (var voice in _voices) voice.Stop();
        }
        _lastTime = _simulation.Elapsed;
        if (_room != null) _room.StreamPaused = Muted || _simulation.Paused;
        foreach (var voice in _voices) voice.StreamPaused = Muted || _simulation.Paused;
        if (_simulation.Paused) return;
        foreach (var e in _simulation.Events)
        {
            if (e.Id <= _lastEvent) continue;
            _lastEvent = e.Id;
            if (Muted || _simulation.Elapsed - e.At > .3) continue;
            if (e.Kind == "heal") Play("heal", -19);
            else if (e.Kind == "shield") Play("shield", -16);
            else if (e.Kind == "attack" && e.Actor == 1) Play("rifle", -15);
            else if (e.Kind == "attack" && e.Actor == 0) Play("metal", -20);
            else if (e.Kind == "attack" && e.Actor >= 3) Play("metal", -25);
            else if (e.Kind == "kill") Play("steam", -20);
        }
        if (_simulation.Phase == ExpeditionPhase.March && _simulation.Elapsed >= _nextStep)
        {
            _nextStep = _simulation.Elapsed + .42;
            if (!Muted) Play("step", -27);
        }
    }

    public override void _UnhandledKeyInput(InputEvent e)
    {
        if (e is InputEventKey { Pressed: true, Echo: false, Keycode: Key.M })
        {
            Muted = !Muted;
            foreach (var voice in _voices) voice.Stop();
            GD.Print("EXPEDITION audio=" + (Muted ? "muted" : "on"));
            GetViewport().SetInputAsHandled();
        }
    }

    private void Play(string clip, float db)
    {
        var voice = _voices[_voice++ % _voices.Count];
        voice.Stream = _clips[clip]; voice.VolumeDb = db;
        voice.PitchScale = 1 + (float)Math.Sin(_voice * 2.3) * .045f; voice.Play();
    }

    private static AudioStreamWav Make(double seconds, Func<double, double, double> sample)
    {
        const int rate = 22050;
        var count = (int)(seconds * rate); var data = new byte[count * 2];
        uint seed = 127;
        for (var i = 0; i < count; i++)
        {
            seed = seed * 1664525 + 1013904223;
            var noise = ((seed >> 8) / 8388607.5) - 1;
            var fade = Math.Min(1, i / 32.0) * Math.Min(1, (count - i) / 64.0);
            var value = (short)(Math.Clamp(sample(i / (double)rate, noise) * fade, -1, 1) * 24000);
            data[i * 2] = (byte)(value & 255); data[i * 2 + 1] = (byte)((value >> 8) & 255);
        }
        return new AudioStreamWav { Format = AudioStreamWav.FormatEnum.Format16Bits, MixRate = rate, Stereo = false, Data = data };
    }
}
