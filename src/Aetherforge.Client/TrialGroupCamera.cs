using Aetherforge.Domain.NavigationTrial;
using Godot;
using System;
using System.Linq;

// Presentation only: no transform or camera result is fed back to the model.
public sealed class TrialGroupCamera
{
    private Vector3 _focus;
    private float _extent=10;
    private bool _bound;
    private const float Pitch=55*Mathf.Pi/180;
    public float Extent => _extent;
    public bool ObjectiveOffscreen { get; private set; }
    public Vector2 ObjectiveDirection { get; private set; }
    public int[] OffscreenAllies { get; private set; }=Array.Empty<int>();
    public int[] OffscreenBodies { get; private set; }=Array.Empty<int>();
    public void Update(Camera3D camera,TrialSimulation model,Vector2I viewport,double dt)
    {
        bool binding=!_bound;
        var points=model.Actors.Select(a=>new Vector3((float)a.At().MetresX,.7f,(float)a.At().MetresZ)).ToArray();
        Vector3 center=new Vector3((points.Min(p=>p.X)+points.Max(p=>p.X))/2,0,(points.Min(p=>p.Z)+points.Max(p=>p.Z))/2);
        var objective=new Vector3(model.Actors[0].Goal.X/1e6f,0,model.Actors[0].Goal.Z/1e6f);
        Vector3 lead=objective-center;if(lead.Length()>1)lead=lead.Normalized();
        center+=lead;
        float desiredX=_focus.X+Mathf.Sign(center.X-_focus.X)*Mathf.Max(0,Mathf.Abs(center.X-_focus.X)-.75f);
        float desiredZ=_focus.Z+Mathf.Sign(center.Z-_focus.Z)*Mathf.Max(0,Mathf.Abs(center.Z-_focus.Z)-.75f);
        if(!_bound){_focus=center;_bound=true;}
        else if(!model.Paused) _focus=_focus.Lerp(new Vector3(desiredX,0,desiredZ),1-Mathf.Exp(-6*(float)dt));
        float aspect=viewport.X/(float)Math.Max(1,viewport.Y);
        float floorRatio=aspect*Mathf.Sin(Pitch);
        // Largest visible ground dimension is capped at 14, including wide/short views.
        float required=8;
        foreach(var p in points)required=Mathf.Max(required,Mathf.Max((Mathf.Abs(p.X-_focus.X)+1.5f)*2*Mathf.Max(1,1/floorRatio),
            (Mathf.Abs(p.Z-_focus.Z)+1.5f)*2*Mathf.Max(1,floorRatio)));
        float objectiveNeed=Mathf.Max(Mathf.Abs(objective.X-_focus.X)*2*Mathf.Max(1,1/floorRatio),Mathf.Abs(objective.Z-_focus.Z)*2*Mathf.Max(1,floorRatio));
        if(objectiveNeed<=14)required=Mathf.Max(required,objectiveNeed);
        float target=Mathf.Clamp(required,8,14);
        if(binding)_extent=target;
        else if(!model.Paused)_extent=Mathf.Clamp(Mathf.Lerp(_extent,target,1-Mathf.Exp(-4*(float)dt)),8,14);
        float width=_extent*Mathf.Min(1,floorRatio),depth=_extent/Mathf.Max(1,floorRatio);
        // Fit the projected silhouettes before optional lead/deadzone. Height projects
        // toward -Z on this fixed 55-degree camera; following feet alone clips heads.
        float minX=points.Min(p=>p.X)-.8f,maxX=points.Max(p=>p.X)+.8f;
        float minZ=points.Min(p=>p.Z)-1.8f/Mathf.Tan(Pitch)-.25f,maxZ=points.Max(p=>p.Z)+.25f;
        _focus.X=maxX-minX<=width?Mathf.Clamp(_focus.X,maxX-width/2,minX+width/2):(minX+maxX)/2;
        _focus.Z=maxZ-minZ<=depth?Mathf.Clamp(_focus.Z,maxZ-depth/2,minZ+depth/2):(minZ+maxZ)/2;
        _focus.X=Mathf.Clamp(_focus.X,width/2,62-width/2);_focus.Z=Mathf.Clamp(_focus.Z,depth/2,28-depth/2);
        camera.Projection=Camera3D.ProjectionType.Orthogonal;camera.KeepAspect=Camera3D.KeepAspectEnum.Height;
        camera.Size=width/aspect;
        camera.Position=_focus+new Vector3(0,Mathf.Sin(Pitch)*20,Mathf.Cos(Pitch)*20);
        camera.LookAt(_focus,Vector3.Up);
        OffscreenAllies=points.Select((p,i)=>new{point=camera.UnprojectPosition(new Vector3(p.X,1.8f,p.Z)),i})
            .Where(x=>x.point.X<0||x.point.X>viewport.X||x.point.Y<0||x.point.Y>viewport.Y).Select(x=>x.i).ToArray();
        bool Outside(Vector2 p)=>p.X<0||p.X>viewport.X||p.Y<0||p.Y>viewport.Y;
        OffscreenBodies=points.Select((p,i)=>new{p,i}).Where(x=>Outside(camera.UnprojectPosition(new Vector3(x.p.X,0,x.p.Z)))||Outside(camera.UnprojectPosition(new Vector3(x.p.X,1.4f,x.p.Z)))).Select(x=>x.i).ToArray();
        var screen=camera.UnprojectPosition(objective);
        ObjectiveOffscreen=screen.X<0||screen.Y<0||screen.X>viewport.X||screen.Y>viewport.Y;
        ObjectiveDirection=(screen-(Vector2)viewport/2).Normalized();
    }
}
