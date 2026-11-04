using Aetherforge.Domain.NavigationTrial;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Presentation only. Fit the geometry actually drawn, including yaw and billboard quads.
public sealed class TrialGroupCamera
{
    private Vector3 _focus;
    private float _extent=10;
    private bool _bound;
    public const float Pitch=55*Mathf.Pi/180;
    public const float Yaw=45*Mathf.Pi/180;
    public const float MarginPixels=4;
    private readonly List<(Node3D node,Vector3[] vertices,bool plate,int actor)> _geometry=new();
    public float Extent=>_extent;
    public bool ObjectiveOffscreen { get; private set; }
    public Vector2 ObjectiveDirection { get; private set; }
    public int[] OffscreenAllies { get; private set; }=Array.Empty<int>();
    public int[] OffscreenBodies { get; private set; }=Array.Empty<int>();
    public float MinBodyEdgePixels { get; private set; }
    public float MinQuadEdgePixels { get; private set; }
    public int BodyVertexCount { get; private set; }
    public int QuadVertexCount { get; private set; }
    public int BindCount { get; private set; }
    public bool GeometryReady=>_geometry.Count>0;
    public Vector3 Focus=>_focus;
    public float RequiredExtent { get; private set; }
    public void Reset(){_bound=false;}
    public void Update(Camera3D camera,TrialSimulation model,Node3D[] actors,Vector2I viewport,double dt)
    {
        if(_geometry.Count==0)
        {
            var pending=new List<(Node3D node,Vector3[] vertices,bool plate,int actor)>();
            for(int i=0;i<actors.Length;i++)foreach(var child in actors[i].GetChildren())
            {
                // Eight corners enclose the COMPLETE mesh, including the rotating weapon.
                // The directed check independently projects the actual triangle vertices.
                if(child is MeshInstance3D mesh)
                {
                    var box=mesh.Mesh.GetAabb();var corners=new Vector3[8];
                    for(int k=0;k<8;k++)corners[k]=box.Position+new Vector3((k&1)==0?0:box.Size.X,(k&2)==0?0:box.Size.Y,(k&4)==0?0:box.Size.Z);
                    pending.Add((mesh,corners,false,i));
                }
                if(child is Label3D label)
                {
                    var triangle=label.GenerateTriangleMesh();
                    if(triangle==null)return; // Godot generates glyph quads after label layout.
                    pending.Add((label,triangle.GetFaces().Distinct().ToArray(),true,i));
                }
            }
            _geometry.AddRange(pending);
        }
        camera.Rotation=new Vector3(-Pitch,Yaw,0);
        var basis=camera.GlobalBasis;
        var vertices=_geometry.Select(g=>(g.plate,g.actor,points:g.vertices.Select(v=>g.plate?g.node.GlobalPosition+basis*v:g.node.GlobalTransform*v).ToArray())).ToArray();
        var all=vertices.SelectMany(g=>g.points).ToArray();
        float minU=all.Min(p=>basis.X.Dot(p)),maxU=all.Max(p=>basis.X.Dot(p));
        float minV=all.Min(p=>basis.Y.Dot(p)),maxV=all.Max(p=>basis.Y.Dot(p));
        var positions=actors.Select(a=>a.GlobalPosition).ToArray();
        Vector3 center=new((positions.Min(p=>p.X)+positions.Max(p=>p.X))/2,0,(positions.Min(p=>p.Z)+positions.Max(p=>p.Z))/2);
        var objective=new Vector3(model.Actors[0].Goal.X/1e6f,0,model.Actors[0].Goal.Z/1e6f);
        Vector3 lead=objective-center;if(lead.Length()>1)lead=lead.Normalized();center+=lead;
        bool binding=!_bound;
        if(binding){_focus=center;_bound=true;BindCount++;}
        else if(!model.Paused)
        {
            var desired=new Vector3(_focus.X+Mathf.Sign(center.X-_focus.X)*Mathf.Max(0,Mathf.Abs(center.X-_focus.X)-.75f),0,
                _focus.Z+Mathf.Sign(center.Z-_focus.Z)*Mathf.Max(0,Mathf.Abs(center.Z-_focus.Z)-.75f));
            _focus=_focus.Lerp(desired,1-Mathf.Exp(-6*(float)dt));
        }
        float aspect=viewport.X/(float)Math.Max(1,viewport.Y);
        // Ground frustum AABB per unit orthographic HEIGHT, valid at this yaw/pitch.
        var groundForward=new Vector3(Mathf.Sin(Yaw),0,Mathf.Cos(Yaw));
        float factor=Mathf.Max(aspect*Mathf.Abs(basis.X.X)+Mathf.Abs(groundForward.X)/Mathf.Sin(Pitch),
            aspect*Mathf.Abs(basis.X.Z)+Mathf.Abs(groundForward.Z)/Mathf.Sin(Pitch));
        float safeW=1-2*MarginPixels/viewport.X,safeH=1-2*MarginPixels/viewport.Y;
        float fit=Mathf.Max((maxU-minU)/(aspect*safeW),(maxV-minV)/safeH)*factor;
        RequiredExtent=fit;
        float softFit=Mathf.Max((maxU-minU+3)/(aspect*safeW),(maxV-minV+3*Mathf.Sin(Pitch))/safeH)*factor;
        float objectiveNeed=Mathf.Max(Mathf.Abs(basis.X.Dot(objective-_focus))*2/aspect,Mathf.Abs(basis.Y.Dot(objective-_focus))*2)*factor;
        float target=Mathf.Clamp(Mathf.Max(Mathf.Max(8,softFit),objectiveNeed<=14?objectiveNeed:0),8,14);
        if(binding)_extent=target;
        else if(!model.Paused)_extent=Mathf.Lerp(_extent,target,1-Mathf.Exp(-4*(float)dt));
        // Safety takes effect immediately; discretionary zoom/follow remains smoothed.
        _extent=Mathf.Clamp(Mathf.Max(_extent,fit),8,14);
        float height=_extent/factor,width=height*aspect;
        float mu=width*MarginPixels/viewport.X,mv=height*MarginPixels/viewport.Y;
        float u0=maxU+mu-width/2,u1=minU-mu+width/2,v0=maxV+mv-height/2,v1=minV-mv+height/2;
        float u=u0<=u1?Mathf.Clamp(basis.X.Dot(_focus),u0,u1):(minU+maxU)/2;
        float focusV=v0<=v1?Mathf.Clamp(basis.Y.Dot(_focus),v0,v1):(minV+maxV)/2;
        // Invert the camera's two ground-plane axes. World X/Z bounds are not
        // screen horizontal/vertical bounds once yaw is non-zero.
        float determinant=basis.X.X*basis.Y.Z-basis.X.Z*basis.Y.X;
        _focus=new Vector3((u*basis.Y.Z-basis.X.Z*focusV)/determinant,0,(basis.X.X*focusV-u*basis.Y.X)/determinant);
        _focus.X=Mathf.Clamp(_focus.X,0,62);_focus.Z=Mathf.Clamp(_focus.Z,0,28);
        camera.Projection=Camera3D.ProjectionType.Orthogonal;camera.KeepAspect=Camera3D.KeepAspectEnum.Height;camera.Size=height;
        camera.Position=_focus+basis.Z*20;
        var bodies=new HashSet<int>();var plates=new HashSet<int>();MinBodyEdgePixels=float.MaxValue;MinQuadEdgePixels=float.MaxValue;BodyVertexCount=0;QuadVertexCount=0;
        foreach(var g in vertices)foreach(var v in g.points)
        {
            var p=camera.UnprojectPosition(v);float edge=Mathf.Min(Mathf.Min(p.X,p.Y),Mathf.Min(viewport.X-p.X,viewport.Y-p.Y));
            if(g.plate){QuadVertexCount++;MinQuadEdgePixels=Mathf.Min(MinQuadEdgePixels,edge);if(edge<-.001f)plates.Add(g.actor);}
            else {BodyVertexCount++;MinBodyEdgePixels=Mathf.Min(MinBodyEdgePixels,edge);if(edge<-.001f)bodies.Add(g.actor);}
        }
        OffscreenBodies=bodies.OrderBy(id=>id).ToArray();OffscreenAllies=plates.OrderBy(id=>id).ToArray();
        var screen=camera.UnprojectPosition(objective);ObjectiveOffscreen=screen.X<0||screen.Y<0||screen.X>viewport.X||screen.Y>viewport.Y;
        ObjectiveDirection=(screen-(Vector2)viewport/2).Normalized();
    }
}
