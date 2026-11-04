using Aetherforge.Domain.NavigationTrial;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

// Explicit scene only. Space pause, C checkpoint, R restore, Tab switch branch.
public partial class NavigationTrial : Node
{
    private TrialSimulation _model=null!;
    private TrialSimulation? _checkpoint;
    private TrialGrid _grid=null!;
    private readonly Node3D[] _actors=new Node3D[3];
    private readonly TrialGroupCamera _rig=new TrialGroupCamera();
    private Camera3D _camera=null!;
    private SubViewport _viewport=null!;
    private SubViewportContainer _container=null!;
    private Vector2I _windowSize;
    private Label _status=null!,_indicator=null!;
    private readonly List<double> _frames=new(),_navigation=new(),_cameraTimes=new();
    private readonly List<object> _samples=new();
    private string _output="";
    private bool _automated,_compact,_captureBusy,_finished,_checking,_finishing,_checkpointVerified,_interpolated,_transitionsVerified;
    private float _minimumBodyMargin=float.MaxValue,_minimumQuadMargin=float.MaxValue;
    private int _lastPhase=-1,_frame;
    private int _offscreenPlateFrames,_offscreenBodyFrames;
    private double _accumulator;
    private readonly HashSet<string> _captures=new();
    private readonly Stopwatch _wall=Stopwatch.StartNew();
    public override void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();
            _automated=args.Contains("--trial-check");_compact=args.Contains("--compact");
            _interpolated=args.Contains("--interpolated");
            foreach(string a in args)if(a.StartsWith("--evidence="))_output=a.Substring(11);
            GetWindow().Size=_compact?new Vector2I(1000,320):new Vector2I(1160,880);
            _grid=new TrialGrid(new TrialMap());_model=new TrialSimulation(_grid,args.Contains("--south")?TrialBranch.South:TrialBranch.North);
            // A full-width 976x180 frustum cannot retain this group at the 14m cap.
            // Keep the short height and letterbox the isolated trial horizontally.
            _container=new SubViewportContainer{Stretch=false};AddChild(_container);
            _viewport=new SubViewport{OwnWorld3D=true,RenderTargetUpdateMode=SubViewport.UpdateMode.Always,TransparentBg=false};
            _container.AddChild(_viewport);
            var world=new Node3D();_viewport.AddChild(world);
            world.AddChild(new WorldEnvironment{Environment=new Godot.Environment{BackgroundMode=Godot.Environment.BGMode.Color,
                BackgroundColor=new Color(.06f,.075f,.095f),AmbientLightSource=Godot.Environment.AmbientSource.Color,AmbientLightColor=Colors.White,AmbientLightEnergy=.7f}});
            world.AddChild(new DirectionalLight3D{RotationDegrees=new Vector3(-65,-25,0),LightEnergy=1.2f});
            foreach(var r in _grid.Map.Floor)
                Box(world,new Vector3((r.X0+r.X1)/2e6f,-.12f,(r.Z0+r.Z1)/2e6f),new Vector3((r.X1-r.X0)/1e6f,.2f,(r.Z1-r.Z0)/1e6f),new Color(.24f,.23f,.21f));
            // Presentation-only industrial plate seams. No physics bodies or new solids.
            var seams=new ImmediateMesh();seams.SurfaceBegin(Mesh.PrimitiveType.Lines,Material(new Color(.12f,.13f,.13f)));
            foreach(var r in _grid.Map.Floor)
            {
                for(long x=r.X0;x<=r.X1;x+=2000000){seams.SurfaceAddVertex(new Vector3(x/1e6f,0,r.Z0/1e6f));seams.SurfaceAddVertex(new Vector3(x/1e6f,0,r.Z1/1e6f));}
                for(long z=r.Z0;z<=r.Z1;z+=2000000){seams.SurfaceAddVertex(new Vector3(r.X0/1e6f,0,z/1e6f));seams.SurfaceAddVertex(new Vector3(r.X1/1e6f,0,z/1e6f));}
            }
            seams.SurfaceEnd();world.AddChild(new MeshInstance3D{Mesh=seams});
            // Physical boundaries drawn from the same UNION catalog. Low translucent walls expose feet.
            foreach(var e in _grid.Map.Boundary)
                Box(world,new Vector3((e.A.X+e.B.X)/2e6f,.2f,(e.A.Z+e.B.Z)/2e6f),
                    new Vector3(Mathf.Max(.07f,Mathf.Abs(e.A.X-e.B.X)/1e6f),.4f,Mathf.Max(.07f,Mathf.Abs(e.A.Z-e.B.Z)/1e6f)),new Color(.65f,.41f,.24f,.45f));
            // Low casings preserve the exact blockout footprint and expose the actors' feet.
            Box(world,new Vector3(27,.22f,14),new Vector3(8,.45f,12),new Color(.24f,.19f,.13f,.55f));
            for(int i=0;i<4;i++)Box(world,new Vector3(24+i*2,.46f,14),new Vector3(.08f,.04f,11.8f),new Color(.55f,.36f,.16f,.6f));
            var colors=new[]{new Color(.45f,.64f,.68f),new Color(.82f,.57f,.26f),new Color(.45f,.67f,.53f)};
            for(int i=0;i<3;i++)
            {
                var pivot=new Node3D();world.AddChild(pivot);_actors[i]=pivot;
                pivot.AddChild(new MeshInstance3D{Mesh=new CapsuleMesh{Radius=.35f,Height=1.4f},Position=new Vector3(0,.7f,0),MaterialOverride=Material(colors[i])});
                Box(pivot,new Vector3(0,1,-.4f),new Vector3(.18f,.12f,.4f),colors[i]);
                pivot.AddChild(new Label3D{Text=new[]{"Guarda","DPS","Suporte"}[i],Position=new Vector3(0,1.8f,0),FontSize=42,PixelSize=.007f,
                    Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,NoDepthTest=true,Modulate=colors[i]});
            }
            _camera=new Camera3D{Current=true,Near=.1f,Far=100};world.AddChild(_camera);
            _status=new Label{Position=new Vector2(12,8),Size=new Vector2(1130,42)};AddChild(_status);
            _indicator=new Label{Position=new Vector2(12,GetWindow().Size.Y-42)};AddChild(_indicator);
            ResizeViewport();Present(0);
        }
        catch(Exception ex){Fail(ex);}
    }
    private static StandardMaterial3D Material(Color c)=>new StandardMaterial3D{AlbedoColor=c,Roughness=1,
        Transparency=c.A<1?BaseMaterial3D.TransparencyEnum.Alpha:BaseMaterial3D.TransparencyEnum.Disabled};
    private static void Box(Node3D parent,Vector3 pos,Vector3 size,Color color)=>parent.AddChild(new MeshInstance3D{Position=pos,Mesh=new BoxMesh{Size=size},MaterialOverride=Material(color)});
    private void Bind(TrialSimulation model){_model=model;_accumulator=0;_rig.Reset();}
    private void ResizeViewport()
    {
        _windowSize=GetWindow().Size;
        bool shortView=_compact||_windowSize.Y<=400;
        var size=new Vector2I(Math.Max(64,shortView?Math.Min(640,_windowSize.X-24):_windowSize.X-24),Math.Max(64,shortView?Math.Min(180,_windowSize.Y-100):_windowSize.Y-100));
        _container.Position=new Vector2((_windowSize.X-size.X)/2f,52);_container.Size=size;_viewport.Size=size;
        _status.Size=new Vector2(_windowSize.X-24,42);_indicator.Position=new Vector2(12,_windowSize.Y-42);_rig.Reset();
    }
    private void Present(double delta)
    {
        if(GetWindow().Size!=_windowSize)ResizeViewport();
        for(int i=0;i<3;i++)
        {
            var a=_model.Actors[i];var p=_automated&&!_interpolated?a.At():_model.PresentationPosition(i);
            _actors[i].Position=new Vector3((float)p.MetresX,0,(float)p.MetresZ);
            float target=Mathf.Atan2(-a.Heading.X,-a.Heading.Z);
            if(!_model.Paused)_actors[i].Rotation=new Vector3(0,Mathf.LerpAngle(_actors[i].Rotation.Y,target,1-Mathf.Exp(-10*(float)delta)),0);
        }
        _rig.Update(_camera,_model,_actors,_viewport.Size,delta);
    }
    public override void _UnhandledKeyInput(InputEvent e)
    {
        if(e is not InputEventKey k||!k.Pressed||k.Echo)return;
        if(k.Keycode==Key.Space)_model.Paused=!_model.Paused;
        if(k.Keycode==Key.C)_checkpoint=_model.Checkpoint();
        if(k.Keycode==Key.R&&_checkpoint!=null)Bind(TrialSimulation.Restore(_checkpoint,_grid));
        if(k.Keycode==Key.Tab){Bind(new TrialSimulation(_grid,_model.Branch==TrialBranch.North?TrialBranch.South:TrialBranch.North));_checkpoint=null;}
    }
    public override void _Process(double delta)
    {
        if(_finished||_model==null)return;
        try
        {
            _frame++;_frames.Add(delta*1000);
            var watch=Stopwatch.StartNew();
            if(!_captureBusy&&!_checking)
            {
                if(_automated)_model.Advance(_interpolated?16667:50000);
                else { _accumulator+=delta*1000000;long us=(long)_accumulator;_accumulator-=us;_model.Advance(us); }
            }
            _navigation.Add(watch.Elapsed.TotalMilliseconds);
            watch.Restart();Present(delta);_cameraTimes.Add(watch.Elapsed.TotalMilliseconds);
            if(!_rig.GeometryReady)return;
            if(!_finishing){_minimumBodyMargin=Math.Min(_minimumBodyMargin,_rig.MinBodyEdgePixels);_minimumQuadMargin=Math.Min(_minimumQuadMargin,_rig.MinQuadEdgePixels);}
            if(!_finishing){if(_rig.OffscreenAllies.Length>0)_offscreenPlateFrames++;if(_rig.OffscreenBodies.Length>0)_offscreenBodyFrames++;}
            _status.Text=$"C3-A  {_model.Branch}  fornalha-c1/r1  t={_model.TimeUs/1e6:0.00}s  fase={_model.Phase}  {_model.Hold}\n"+
                $"{string.Join(" / ",_model.Actors.Select(a=>a.State))}  reserva=[{string.Join(',',_model.Reservations)}]  Space pausa · C checkpoint · R restore · Tab N/S";
            _indicator.Text=$"viewport {_viewport.Size.X}×{_viewport.Size.Y} · extensão {_rig.Extent:0.00}m · "+
                (_rig.ObjectiveOffscreen?$"Objetivo fora do quadro: {_rig.ObjectiveDirection} · {_model.Actors[0].Goal}":"Objetivo enquadrado")+
                (_rig.OffscreenAllies.Length>0?$" · Placa fora: {string.Join(',',_rig.OffscreenAllies)}":"");
            if(_automated&&!_captureBusy&&!_checking&&!_finishing)
            {
                string? label=null;
                if(_model.Phase!=_lastPhase){_lastPhase=_model.Phase;label="phase-"+_lastPhase;}
                if(_checkpoint==null&&(_model.Reservations.Count>=2||_model.Branch==TrialBranch.South&&_model.Phase==5))
                {_checkpoint=_model.Checkpoint();_ = ExerciseCheckpoint();return;}
                if(label!=null&&_captures.Add(label))_ = Capture(label);
                if(_model.Hold.Length>0)throw new InvalidOperationException(_model.Hold);
                if(_model.Complete)_ = Finish();
                if(_frame>7000)throw new InvalidOperationException("Pilot did not complete");
            }
        }
        catch(Exception ex){Fail(ex);}
    }
    private void Fail(Exception ex)
    {
        GD.PrintErr(ex);_finished=true;
        if(_output.Length>0){Directory.CreateDirectory(_output);File.WriteAllText(Path.Combine(_output,"render.json"),JsonSerializer.Serialize(new{passed=false,error=ex.ToString(),_frame,_offscreenBodyFrames,_offscreenPlateFrames,samples=_samples},new JsonSerializerOptions{WriteIndented=true}));}
        GetTree().Quit(1);
    }
    private async Task ExerciseCheckpoint()
    {
        _checking=true;
        try
        {
            await Capture("corridor-checkpoint");
            _model.Paused=true;string frozen=_model.Fingerprint;
            _model.Advance(2000000);if(_model.Fingerprint!=frozen)throw new InvalidOperationException("Pause changed pilot state");
            await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await Capture("paused");
            var replay=TrialSimulation.Restore(_checkpoint!,_grid);
            var expected=_checkpoint!.Checkpoint();
            for(int i=0;i<20;i++){replay.Advance(50000);expected.Advance(17000);expected.Advance(33000);}
            if(replay.Fingerprint!=expected.Fingerprint)throw new InvalidOperationException("Restore suffix mismatch");
            Bind(TrialSimulation.Restore(_checkpoint,_grid));
            await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await Capture("restored");
            _checkpointVerified=true;
        }
        catch(Exception ex){Fail(ex);}
        finally{_checking=false;}
    }
    private async Task Capture(string label)
    {
        _captureBusy=true;
        try
        {
            if(_output.Length>0)
            {
                Directory.CreateDirectory(_output);
                if(DisplayServer.GetName()!="headless")
                { await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);GetViewport().GetTexture().GetImage().SavePng(Path.Combine(_output,label+".png")); }
            }
            _samples.Add(new{label,_frame,_model.TimeUs,_model.Phase,_model.Paused,_model.Hold,branch=_model.Branch.ToString(),viewport=new[]{_viewport.Size.X,_viewport.Size.Y},extent=_rig.Extent,
                _rig.ObjectiveOffscreen,_rig.OffscreenAllies,_rig.OffscreenBodies,reservations=_model.Reservations.ToArray(),actors=_model.Actors.Select(a=>new{a.Id,x=a.At().MetresX,z=a.At().MetresZ,a.Index,a.State}).ToArray()});
            _samples.Add(new{geometry=label,_rig.MinBodyEdgePixels,_rig.MinQuadEdgePixels,_rig.BodyVertexCount,_rig.QuadVertexCount,_rig.BindCount,
                presented=_actors.Select(a=>new{x=a.Position.X,z=a.Position.Z,yaw=a.Rotation.Y}).ToArray()});
        }
        finally{_captureBusy=false;}
    }
    private async Task Finish()
    {
        try{await FinishRun();}
        catch(Exception ex){Fail(ex);}
    }
    private async Task FinishRun()
    {
        var journeyFrames=_frames.ToList();var journeyNavigation=_navigation.ToList();var journeyCamera=_cameraTimes.ToList();
        if(_offscreenBodyFrames>0||_offscreenPlateFrames>0||_minimumBodyMargin<TrialGroupCamera.MarginPixels-.01f||_minimumQuadMargin<TrialGroupCamera.MarginPixels-.01f)
            throw new InvalidOperationException($"Group framing clipped bodies={_offscreenBodyFrames}, plates={_offscreenPlateFrames}");
        _finishing=true;_checking=true;await Capture("complete");
        // Same bind/resize/input paths as the interactive scene, with paused presentation.
        var final=_model.Checkpoint();var rootSize=GetWindow().Size;
        _model.Paused=true;string paused=_model.Fingerprint;
        GetWindow().Size=_compact?new Vector2I(1160,880):new Vector2I(1000,320);
        await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);Present(0);
        if(_model.Fingerprint!=paused)throw new InvalidOperationException("Resize moved paused model");
        RequireFraming();
        await Capture("transition-resize-paused");
        GetWindow().Size=rootSize;await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);Present(0);
        _UnhandledKeyInput(new InputEventKey{Keycode=Key.Tab,Pressed=true});Present(0);
        if(_model.TimeUs!=0)throw new InvalidOperationException("Tab did not reset");
        RequireFraming();
        await Capture("transition-tab-reset");
        Bind(TrialSimulation.Restore(final,_grid));Present(0);await Capture("transition-restore");
        RequireFraming();
        _transitionsVerified=true;
        // Explicit fault fixtures, isolated from the normal two-contour journey.
        Bind(new TrialSimulation(_grid,TrialBranch.North){Budget=1});_model.Advance(50000);
        await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await Capture("fixture-pending");
        _model.Advance(3000000);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await Capture("fixture-budget-hold");
        if(_model.Hold!="NavigationBudgetExceeded")throw new InvalidOperationException("Missing budget Hold");
        Bind(new TrialSimulation(_grid,TrialBranch.North));_model.Advance(500000);_model.Obstructed=true;_model.Advance(3000000);
        await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await Capture("fixture-block-recovery");
        _model.Advance(7000000);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await Capture("fixture-block-hold");
        if(_model.Hold!="NavigationBlocked")throw new InvalidOperationException("Missing obstruction Hold");
        _model.Obstructed=false;_model.Resume();_model.Advance(1000000);
        await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await Capture("fixture-resumed");
        Bind(new TrialSimulation(_grid,TrialBranch.North));_model.EnableLeash(0,_grid.Map.Floor[0]);_model.Advance(5000000);
        await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await Capture("fixture-leash-return");
        _finished=true;
        double P(List<double> x,double p)=>x.OrderBy(v=>v).ElementAt(Math.Min(x.Count-1,(int)(x.Count*p)));
        object Stats(List<double> x)=>new{p50=P(x,.5),p95=P(x,.95),p99=P(x,.99),max=x.Max()};
        if(_output.Length>0)File.WriteAllText(Path.Combine(_output,"render.json"),JsonSerializer.Serialize(new{passed=true,method=_interpolated?"author fixed 16667us with PresentationPosition; synthetic cadence, not natural 1x or human/DPI test":"author fixed 50ms model step per render frame; not natural 1x or human/DPI test",
            display=DisplayServer.GetName(),renderer=RenderingServer.GetCurrentRenderingMethod().ToString(),engine=Engine.GetVersionInfo().ToString(),
            cpu=OS.GetProcessorName(),logicalProcessors=OS.GetProcessorCount(),gpu=RenderingServer.GetVideoAdapterName(),
            workingSet=Process.GetCurrentProcess().WorkingSet64,managedMemory=GC.GetTotalMemory(false),
            checkpointVerified=_checkpointVerified,transitionsVerified=_transitionsVerified,_offscreenPlateFrames,_offscreenBodyFrames,_minimumBodyMargin,_minimumQuadMargin,requiredMargin=TrialGroupCamera.MarginPixels,
            geometryMethod="rendered mesh vertices and camera-facing Label3D generated quads, including yaw; quad overflow is not glyph-pixel clipping or human legibility",viewport=new[]{_viewport.Size.X,_viewport.Size.Y},frames=Stats(journeyFrames),navigation=Stats(journeyNavigation),camera=Stats(journeyCamera),wallSeconds=_wall.Elapsed.TotalSeconds,
            _model.Grid.Map.Hash,samples=_samples},new JsonSerializerOptions{WriteIndented=true}));
        GD.Print("NAVIGATION TRIAL RENDER PASS");GetTree().Quit();
    }
    private void RequireFraming()
    {
        if(!_rig.GeometryReady||_rig.MinBodyEdgePixels<TrialGroupCamera.MarginPixels-.01f||_rig.MinQuadEdgePixels<TrialGroupCamera.MarginPixels-.01f)
            throw new InvalidOperationException("Transition geometry escaped margin");
    }
}
