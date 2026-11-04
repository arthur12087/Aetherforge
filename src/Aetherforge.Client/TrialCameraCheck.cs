using Aetherforge.Domain.NavigationTrial;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

// Opt-in camera evidence. Works against the exact 52267cb client as well as the
// visual increment. Domain state is read or driven via existing public commands.
public partial class TrialCameraCheck : Node
{
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    private NavigationTrial _trial=null!;
    private string _output="",_video="",_tag="";
    private bool _natural,_wide,_before;
    private int _assertions,_frame,_bodyFailures,_quadFailures;
    private float _minBody=float.MaxValue,_minQuad=float.MaxValue,_maxGround;
    private readonly List<double> _frames=new(),_cameraMs=new(),_advanceMs=new();
    private readonly List<object> _samples=new(),_commands=new();
    private readonly List<(MeshInstance3D mesh,Vector3[] vertices)> _meshes=new();
    private readonly List<(Label3D label,Vector3[] vertices)> _labels=new();
    private readonly Stopwatch _wall=Stopwatch.StartNew();
    private Process? _encoder;
    private Task<string>? _encoderError;
    private byte[]? _previousVideoFrame;
    private int _videoFrames,_videoWidth,_videoHeight;
    private double _videoStarted;
    private TrialSimulation Model=>Field<TrialSimulation>("_model");
    private Camera3D Camera=>Field<Camera3D>("_camera");
    private SubViewport View=>Field<SubViewport>("_viewport");
    private Node3D[] Actors=>Field<Node3D[]>("_actors");
    private T Field<T>(string name)=>(T)typeof(NavigationTrial).GetField(name,Flags)!.GetValue(_trial)!;
    private void Present(double dt)=>typeof(NavigationTrial).GetMethod("Present",Flags)!.Invoke(_trial,new object[]{dt});
    private void Press(Key key)=>_trial._UnhandledKeyInput(new InputEventKey{Keycode=key,Pressed=true});
    private void Require(bool value,string message){_assertions++;if(!value)throw new InvalidOperationException(message);}
    // SignalAwaiter is awaitable but not a Task in the Godot API.
    private async Task NextFrame(){await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private async Task Draw(){await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);}

    public override async void _Ready()
    {
        bool passed=false;string error="";double activeWall=0;long activeInput=0,advanced=0;
        try
        {
            var args=OS.GetCmdlineUserArgs();_natural=args.Contains("--camera-natural");_wide=args.Contains("--camera-wide");_before=args.Contains("--camera-before");
            foreach(string a in args){if(a.StartsWith("--evidence="))_output=a[11..];if(a.StartsWith("--video="))_video=a[8..];if(a.StartsWith("--tag="))_tag=a[6..];}
            Directory.CreateDirectory(_output);
            _trial=new NavigationTrial();AddChild(_trial);_trial.SetProcess(false);
            for(int i=0;i<5;i++){await NextFrame();Present(0);}
            if(_wide)WideViewport();
            ReadGeometry();Present(0);Measure("start");await Draw();Capture("start");
            if(_video.Length>0)StartVideo();
            _wall.Restart();_videoStarted=0;double previousWall=0;bool transitions=false;int lastPhase=-1;
            while(!Model.Complete&&Model.Hold.Length==0&&_frame<9000&&_wall.Elapsed.TotalSeconds<75)
            {
                await NextFrame();double now=_wall.Elapsed.TotalSeconds,dt=now-previousWall;previousWall=now;
                _frames.Add(dt*1000);activeWall+=dt;
                long input=_natural?(long)Math.Round(dt*1000000):50000;
                long at=Model.TimeUs;var watch=Stopwatch.StartNew();Model.Advance(input);_advanceMs.Add(watch.Elapsed.TotalMilliseconds);
                activeInput+=input;advanced+=Model.TimeUs-at;
                watch.Restart();Present(_natural?dt:1.0/60);_cameraMs.Add(watch.Elapsed.TotalMilliseconds);
                if(_wide)WideViewport();
                _frame++;Measure(null);Overlay();await Draw();RecordVideo();
                if(Model.Phase!=lastPhase){lastPhase=Model.Phase;Sample("phase-"+lastPhase);Capture("phase-"+lastPhase);}
                if(!transitions&&Model.Phase>=5)
                {
                    double began=_wall.Elapsed.TotalSeconds;await Transitions();
                    double duration=_wall.Elapsed.TotalSeconds-began;
                    _commands.Add(new{kind="controlled-presentation-pause",wall=began,duration});previousWall=_wall.Elapsed.TotalSeconds;transitions=true;
                }
            }
            Require(Model.Complete,"Camera journey did not reach completion: "+Model.Hold);
            Require(transitions,"Missing presentation transition coverage");
            if(!_before&&!_wide){Require(_bodyFailures==0,"Full mesh vertices escaped framing");Require(_quadFailures==0,"Generated indicator quads escaped framing");}
            Require(_maxGround<=14.002f,"Ground footprint exceeded contracted 14m cap");
            if(_natural)Require(Math.Abs(advanced-activeWall*1000000)<100000,"Natural clock drift exceeded 100ms (terminal remainder included)");
            Measure("complete");await Draw();Capture("complete");RecordVideo();passed=true;
        }
        catch(Exception ex){error=ex.ToString();GD.PrintErr(error);}
        finally
        {
            try{await StopVideo();}catch(Exception ex){passed=false;error+="\nEncoder: "+ex;GD.PrintErr(ex);}
            if(_output.Length>0)
            {
                object Stats(List<double> x)=>new{count=x.Count,p50=P(x,.5),p95=P(x,.95),p99=P(x,.99),max=x.Count>0?x.Max():0};
                File.WriteAllText(Path.Combine(_output,"camera.json"),JsonSerializer.Serialize(new{
                    passed,error,_tag,method=_natural?"real monotonic wall-clock input at natural 1x; controlled transition pause logged; video wall-time resampled at 20fps":"fixed 50ms model / 1/60 camera dt, synthetic comparison, not FPS evidence",
                    _before,_wide,_natural,assertions=_assertions,frames=_frame,activeWallSeconds=activeWall,activeInputUs=activeInput,advancedUs=advanced,
                    videoFrames=_videoFrames,videoDurationSeconds=_videoFrames/20.0,wallSeconds=_wall.Elapsed.TotalSeconds,
                    cpu=OS.GetProcessorName(),gpu=RenderingServer.GetVideoAdapterName(),engine=Engine.GetVersionInfo().ToString(),display=DisplayServer.GetName(),
                    margins=new{body=_minBody,quad=_minQuad,bodyFailedFrames=_bodyFailures,quadFailedFrames=_quadFailures},maximumGroundExtent=_maxGround,
                    frameMs=Stats(_frames),cameraMs=Stats(_cameraMs),advanceMs=Stats(_advanceMs),
                    geometry="independent projection of actual Mesh.GetFaces triangles and Label3D.GenerateTriangleMesh quads with presented transforms; does not certify glyph pixels, effects or perception",
                    samples=_samples,commands=_commands},new JsonSerializerOptions{WriteIndented=true}));
            }
            GD.Print(passed?"TRIAL CAMERA CHECK PASS":"TRIAL CAMERA CHECK FAIL");GetTree().Quit(passed?0:1);
        }
    }
    private static double P(List<double> x,double p)=>x.Count==0?0:x.OrderBy(v=>v).ElementAt(Math.Min(x.Count-1,(int)(p*x.Count)));
    private void ReadGeometry()
    {
        foreach(var actor in Actors)foreach(var child in actor.GetChildren())
        {
            if(child is MeshInstance3D mesh)_meshes.Add((mesh,mesh.Mesh.GetFaces().Distinct().ToArray()));
            if(child is Label3D label)_labels.Add((label,label.GenerateTriangleMesh().GetFaces().Distinct().ToArray()));
        }
        Require(_meshes.Count>=6&&_labels.Count==3,"Missing complete actor/weapon/indicator geometry");
    }
    private void Measure(string? label)
    {
        float body=float.MaxValue,quad=float.MaxValue;
        float Edge(Vector3 world){var p=Camera.UnprojectPosition(world);return Mathf.Min(Mathf.Min(p.X,p.Y),Mathf.Min(View.Size.X-p.X,View.Size.Y-p.Y));}
        foreach(var (mesh,vertices) in _meshes)foreach(var v in vertices)body=Math.Min(body,Edge(mesh.GlobalTransform*v));
        foreach(var (plate,vertices) in _labels)foreach(var v in vertices)quad=Math.Min(quad,Edge(plate.GlobalPosition+Camera.GlobalBasis*v));
        _minBody=Math.Min(_minBody,body);_minQuad=Math.Min(_minQuad,quad);
        if(body<3.98f)_bodyFailures++;if(quad<3.98f)_quadFailures++;
        Require(Camera.Projection==Camera3D.ProjectionType.Orthogonal,"Camera must be truly orthographic");
        // Independently intersect all four camera rays with y=0 to measure the footprint.
        var floor=new List<Vector3>();
        foreach(var uv in new[]{Vector2.Zero,new Vector2(View.Size.X,0),new Vector2(0,View.Size.Y),(Vector2)View.Size})
        {var origin=Camera.ProjectRayOrigin(uv);var direction=Camera.ProjectRayNormal(uv);floor.Add(origin+direction*(-origin.Y/direction.Y));}
        float extent=Math.Max(floor.Max(p=>p.X)-floor.Min(p=>p.X),floor.Max(p=>p.Z)-floor.Min(p=>p.Z));_maxGround=Math.Max(_maxGround,extent);
        if(!_before)Require(Math.Abs(Camera.Rotation.Y-Mathf.Pi/4)<.00001f&&Math.Abs(Camera.Rotation.X+55*Mathf.Pi/180)<.00001f,"Fixed elevated diagonal orientation changed");
        else Require(Math.Abs(Camera.Rotation.Y)<.00001f,"Reference binary does not match the zero-yaw 52267cb camera");
        if(label!=null)Sample(label);
    }
    private void Sample(string label)=>_samples.Add(new{label,_frame,wall=_wall.Elapsed.TotalSeconds,Model.TimeUs,Model.Phase,Model.Paused,branch=Model.Branch.ToString(),root=GetWindow().Size.ToString(),viewport=View.Size.ToString(),
        camera=new{position=Camera.Position.ToString(),rotation=Camera.Rotation.ToString(),Camera.Size},minimumBody=_minBody,minimumQuad=_minQuad,
        poses=Actors.Select(a=>new{position=a.Position.ToString(),yaw=a.Rotation.Y}).ToArray()});
    private async Task Transitions()
    {
        Press(Key.C);var checkpoint=Model.Checkpoint();Press(Key.Space);string frozen=Model.Fingerprint;
        foreach(var size in new[]{new Vector2I(1000,320),new Vector2I(1160,880),new Vector2I(540,280),GetWindow().Size})
        {
            GetWindow().Size=size;await NextFrame();Present(0);if(_wide)WideViewport();
            Require(Model.Fingerprint==frozen,"Paused resize changed logical state");Measure("paused-resize-"+size);await Draw();Capture("paused-resize-"+size);RecordVideo();
        }
        if(_natural){await ToSignal(GetTree().CreateTimer(1),SceneTreeTimer.SignalName.Timeout);RecordVideo();}
        Press(Key.R);Present(0);Require(Model.Fingerprint==checkpoint.Fingerprint,"Presentation restore changed checkpoint identity");Measure("restore-key");await Draw();Capture("restore-key");RecordVideo();
        Press(Key.Tab);Present(0);Require(Model.TimeUs==0&&Model.Branch!=checkpoint.Branch,"Tab did not reset branch");Measure("tab-key");await Draw();Capture("tab-key");RecordVideo();
        string reset=Model.Fingerprint;Press(Key.R);Require(Model.Fingerprint==reset,"Tab should clear stale user checkpoint");
        typeof(NavigationTrial).GetMethod("Bind",Flags)!.Invoke(_trial,new object[]{TrialSimulation.Restore(checkpoint,Field<TrialGrid>("_grid"))});
        Present(0);Measure("restore-after-tab-fixture");_commands.Add(new{kind="pause/resize/R/Tab/stale-R/explicit-bind",wall=_wall.Elapsed.TotalSeconds});
    }
    private void Overlay()
    {
        Field<Label>("_status").Text=$"C3-A · {(_before?"ANTES 52267cb":"CÂMERA DIAGONAL")} · {(_natural?"natural 1×":"passo fixo")} · {Model.Branch} · t={Model.TimeUs/1e6:0.00}s\nTrês atores · navegação isolada · sem dano/combate de produção";
        Field<Label>("_indicator").Text=$"viewport {View.Size.X}×{View.Size.Y} · ortográfica · inclinação {Math.Abs(Camera.RotationDegrees.X):0}° · orientação {Camera.RotationDegrees.Y:0}°";
    }
    private void WideViewport()
    {
        View.Size=new Vector2I(976,180);var container=Field<SubViewportContainer>("_container");container.Size=View.Size;container.Position=new Vector2(12,52);
        // Refit in the actual requested diagnostic viewport; never silently relabel 640.
        var rig=Field<TrialGroupCamera>("_rig");rig.Update(Camera,Model,Actors,View.Size,0);
    }
    private void Capture(string label)
    {
        if(DisplayServer.GetName()=="headless")return;
        var result=GetViewport().GetTexture().GetImage().SavePng(Path.Combine(_output,label+".png"));
        Require(result==Error.Ok,"Capture write failed");
    }
    private void StartVideo()
    {
        Require(DisplayServer.GetName()!="headless","Natural video requires real rendering");
        var image=GetViewport().GetTexture().GetImage();_videoWidth=image.GetWidth();_videoHeight=image.GetHeight();
        var start=new ProcessStartInfo("ffmpeg.exe"){UseShellExecute=false,RedirectStandardInput=true,RedirectStandardError=true,CreateNoWindow=true};
        foreach(string a in new[]{"-y","-hide_banner","-loglevel","error","-f","rawvideo","-pix_fmt","rgba","-s",$"{_videoWidth}x{_videoHeight}","-r","20","-i","pipe:0","-an","-c:v","libx264","-preset","veryfast","-crf","22","-pix_fmt","yuv420p","-movflags","+faststart",_video})start.ArgumentList.Add(a);
        _encoder=Process.Start(start)!;_encoderError=_encoder.StandardError.ReadToEndAsync();
    }
    private void RecordVideo()
    {
        if(_encoder==null)return;
        int wanted=(int)Math.Floor((_wall.Elapsed.TotalSeconds-_videoStarted)*20)+1;
        if(wanted<=_videoFrames)return;
        var image=GetViewport().GetTexture().GetImage();
        Require(image.GetWidth()==_videoWidth&&image.GetHeight()==_videoHeight||Model.Paused,"Video size changed outside controlled paused resize");
        image.Convert(Image.Format.Rgba8);byte[] current=image.GetData();
        if(image.GetWidth()!=_videoWidth||image.GetHeight()!=_videoHeight)
        {
            float scale=Math.Min(_videoWidth/(float)image.GetWidth(),_videoHeight/(float)image.GetHeight());
            image.Resize((int)(image.GetWidth()*scale),(int)(image.GetHeight()*scale));
            var canvas=Image.CreateEmpty(_videoWidth,_videoHeight,false,Image.Format.Rgba8);canvas.Fill(Colors.Black);
            canvas.BlitRect(image,new Rect2I(0,0,image.GetWidth(),image.GetHeight()),new Vector2I((_videoWidth-image.GetWidth())/2,(_videoHeight-image.GetHeight())/2));current=canvas.GetData();
        }
        while(_videoFrames+1<wanted){_encoder.StandardInput.BaseStream.Write(_previousVideoFrame??current);_videoFrames++;}
        _encoder.StandardInput.BaseStream.Write(current);_videoFrames++;
        _previousVideoFrame=current;
    }
    private async Task StopVideo()
    {
        if(_encoder==null)return;
        _encoder.StandardInput.Close();
        if(!_encoder.WaitForExit(10000)){_encoder.Kill();throw new InvalidOperationException("Video encoder did not terminate");}
        string errors=await _encoderError!;int code=_encoder.ExitCode;_encoder.Dispose();_encoder=null;
        if(code!=0)throw new InvalidOperationException("Video encoder failed: "+errors);
    }
}
