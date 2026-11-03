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
    private const float Pitch=55*Mathf.Pi/180;
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
    public void Reset(){_bound=false;}
    public void Update(Camera3D camera,TrialSimulation model,Node3D[] actors,Vector2I viewport,double dt)
    {
        if(_geometry.Count==0)
        {
            var pending=new List<(Node3D node,Vector3[] vertices,bool plate,int actor)>();
            for(int i=0;i<actors.Length;i++)foreach(var child in actors[i].GetChildren())
            {
                if(child is MeshInstance3D mesh)pending.Add((mesh,mesh.Mesh.GetFaces().Distinct().ToArray(),false,i));
                if(child is Label3D label)
                {
                    var triangle=label.GenerateTriangleMesh();
                    if(triangle==null)return; // Godot generates glyph quads after label layout.
                    pending.Add((label,triangle.GetFaces().Distinct().ToArray(),true,i));
                }
            }
            _geometry.AddRange(pending);
        }
        camera.Rotation=new Vector3(-Pitch,0,0);
        var basis=camera.GlobalBasis;
        var vertices=_geometry.Select(g=>(g.plate,g.actor,points:g.vertices.Select(v=>g.plate?g.node.GlobalPosition+basis*v:g.node.GlobalTransform*v).ToArray())).ToArray();
        var all=vertices.SelectMany(g=>g.points).ToArray();
        float minX=all.Min(p=>p.X),maxX=all.Max(p=>p.X);
        float minZ=all.Min(p=>p.Z-p.Y/Mathf.Tan(Pitch)),maxZ=all.Max(p=>p.Z-p.Y/Mathf.Tan(Pitch));
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
        float aspect=viewport.X/(float)Math.Max(1,viewport.Y),ratio=aspect*Mathf.Sin(Pitch);
        float widthRatio=Mathf.Min(1,ratio),depthRatio=1/Mathf.Max(1,ratio);
        float fit=Mathf.Max((maxX-minX)/(widthRatio*(1-2*MarginPixels/viewport.X)),(maxZ-minZ)/(depthRatio*(1-2*MarginPixels/viewport.Y)));
        float objectiveNeed=Mathf.Max(Mathf.Abs(objective.X-_focus.X)*2/widthRatio,Mathf.Abs(objective.Z-_focus.Z)*2/depthRatio);
        float target=Mathf.Clamp(Mathf.Max(Mathf.Max(8,fit),objectiveNeed<=14?objectiveNeed:0),8,14);
        if(binding)_extent=target;
        else if(!model.Paused)_extent=Mathf.Lerp(_extent,target,1-Mathf.Exp(-4*(float)dt));
        // Safety takes effect immediately; discretionary zoom/follow remains smoothed.
        _extent=Mathf.Clamp(Mathf.Max(_extent,fit),8,14);
        float width=_extent*widthRatio,depth=_extent*depthRatio;
        float mx=width*MarginPixels/viewport.X,mz=depth*MarginPixels/viewport.Y;
        float x0=maxX+mx-width/2,x1=minX-mx+width/2,z0=maxZ+mz-depth/2,z1=minZ-mz+depth/2;
        _focus.X=x0<=x1?Mathf.Clamp(_focus.X,x0,x1):(minX+maxX)/2;
        _focus.Z=z0<=z1?Mathf.Clamp(_focus.Z,z0,z1):(minZ+maxZ)/2;
        _focus.X=Mathf.Clamp(_focus.X,width/2,62-width/2);_focus.Z=Mathf.Clamp(_focus.Z,depth/2,28-depth/2);
        camera.Projection=Camera3D.ProjectionType.Orthogonal;camera.KeepAspect=Camera3D.KeepAspectEnum.Height;camera.Size=width/aspect;
        camera.Position=_focus+new Vector3(0,Mathf.Sin(Pitch)*20,Mathf.Cos(Pitch)*20);
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
