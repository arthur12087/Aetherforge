using System;
using System.Collections.Generic;
using System.Linq;

namespace Aetherforge.Domain.NavigationTrial
{
    public enum TrialBranch { North, South }
    public enum TrialSearchStatus { Pending, Found, NoPath }

    public sealed class TrialGrid
    {
        private static readonly int[] Dx={1,0,-1,0,1,-1,-1,1}, Dz={0,1,0,-1,1,1,-1,-1};
        public const int Width=620, Height=280, Count=Width*Height;
        public readonly TrialMap Map;
        private readonly bool[] _mask=new bool[Count];
        private readonly Dictionary<long,bool> _edges=new Dictionary<long,bool>();
        public long EdgeCacheHits { get; private set; }
        public TrialGrid(TrialMap map)
        {
            Map=map;
            for(int n=0;n<Count;n++) _mask[n]=map.Clear(Point(n));
        }
        public static TrialPoint Point(int n) => new TrialPoint(50000+(n%Width)*100000,50000+(n/Width)*100000);
        public bool Allowed(int n, TrialBranch branch) => n>=0 && n<Count && _mask[n] &&
            !(Point(n).X>=23000000 && Point(n).X<=31000000 && (branch==TrialBranch.North ? Point(n).Z<20000000 : Point(n).Z>8000000));
        public IEnumerable<int> Connect(TrialPoint p,TrialBranch branch)
        {
            int ix=(int)((p.X-50000)/100000), iz=(int)((p.Z-50000)/100000);
            for(int j=iz-1;j<=iz+1;j++) for(int i=ix-1;i<=ix+1;i++)
            {
                if(i<0||i>=Width||j<0||j>=Height) continue;
                int n=j*Width+i; var q=Point(n);
                if(Math.Abs(p.X-q.X)<=100000 && Math.Abs(p.Z-q.Z)<=100000 && Allowed(n,branch) && Map.Segment(p,q)) yield return n;
            }
        }
        public IEnumerable<(int Node,long Cost)> Neighbours(int n,TrialBranch branch)
        {
            int x=n%Width,z=n/Width;
            for(int k=0;k<8;k++)
            {
                int i=x+Dx[k],j=z+Dz[k]; if(i<0||i>=Width||j<0||j>=Height) continue;
                int q=j*Width+i; if(!Allowed(q,branch)) continue;
                long key=(long)Math.Min(n,q)*Count+Math.Max(n,q);
                if(!_edges.TryGetValue(key,out bool clear)) { clear=Map.Segment(Point(n),Point(q)); _edges.Add(key,clear); } else EdgeCacheHits++;
                if(clear) yield return (q,k<4?100000:141422);
            }
        }
        // A real path witness on the contracted grid, not Euclidean distance.
        // Two monotone octile paths are sufficient for nearby open-space pairs;
        // around a concave corner callers fall back to their shared route witnesses.
        public long DistanceCertificate(TrialPoint a,TrialPoint b,TrialBranch branch)
        {
            if(!Map.Clear(a,450004)||!Map.Clear(b,450004))return long.MaxValue;
            long best=long.MaxValue;
            var ac=Connect(a,branch).ToArray();var bc=Connect(b,branch).ToArray();
            foreach(int n in ac)foreach(int m in bc)
            {
                var p=Point(n);var q=Point(m);long dx=q.X-p.X,dz=q.Z-p.Z;
                if(!Map.Segment(a,p,450004)||!Map.Segment(q,b,450004))continue;
                long count=Math.Min(Math.Abs(dx),Math.Abs(dz))/100000;
                var corner=new TrialPoint(p.X+Math.Sign(dx)*count*100000,p.Z+Math.Sign(dz)*count*100000);
                var other=new TrialPoint(q.X-Math.Sign(dx)*count*100000,q.Z-Math.Sign(dz)*count*100000);
                long cost=TrialPoint.Length(a,p)+TrialPoint.Length(q,b)+count*141422+Math.Abs(Math.Abs(dx)-Math.Abs(dz));
                if(cost>=best)continue;
                if(Map.Segment(p,corner)&&Map.Segment(corner,q)||Map.Segment(p,other)&&Map.Segment(other,q))best=cost;
            }
            return best;
        }
    }

    // Incremental A*. All mutable search state is deep-cloned, including heap duplicates.
    public sealed class TrialSearch
    {
        private struct Entry
        {
            public int Node; public long G,H;
            public Entry(int n,long g,long h) { Node=n; G=g; H=h; }
        }
        public readonly TrialPoint Start, Destination;
        public readonly TrialBranch Branch;
        public readonly long RequestKey;
        public TrialSearchStatus Status { get; private set; }=TrialSearchStatus.Pending;
        public int Expansions { get; private set; }
        public List<TrialPoint> Path { get; private set; }=new List<TrialPoint>();
        public long Cost { get; private set; }
        private List<Entry> _heap=new List<Entry>();
        private Dictionary<int,long> _g=new Dictionary<int,long>();
        private Dictionary<int,int> _parent=new Dictionary<int,int>();
        private HashSet<int> _closed=new HashSet<int>();
        private Dictionary<int,long> _goals=new Dictionary<int,long>();
        private int _best=-1;
        private long _bestCost=long.MaxValue;
        private TrialPoint[] _reserved;
        private TrialRect? _area;
        public int OpenCount => _heap.Count;
        public TrialSearch(TrialGrid grid,TrialPoint start,TrialPoint destination,TrialBranch branch,long requestKey,TrialPoint[]? reserved=null,TrialRect? area=null)
        {
            Start=start; Destination=destination; Branch=branch; RequestKey=requestKey;
            _reserved=reserved ?? Array.Empty<TrialPoint>();
            _area=area;
            if(!InArea(start)||!InArea(destination)){Status=TrialSearchStatus.NoPath;return;}
            foreach(int n in grid.Connect(destination,branch)) if(InArea(TrialGrid.Point(n))&&Free(destination,TrialGrid.Point(n))) _goals[n]=TrialPoint.Length(TrialGrid.Point(n),destination);
            if(_goals.Count==0){Status=TrialSearchStatus.NoPath;return;}
            foreach(int n in grid.Connect(start,branch))
            { if(!InArea(TrialGrid.Point(n))||!Free(start,TrialGrid.Point(n)))continue;long g=TrialPoint.Length(start,TrialGrid.Point(n)); _g[n]=g; _parent[n]=-1; Push(new Entry(n,g,H(n))); }
            if(_goals.Count==0||_heap.Count==0) Status=TrialSearchStatus.NoPath;
        }
        private bool InArea(TrialPoint p) => !_area.HasValue || p.X>=_area.Value.X0+450000 && p.X<=_area.Value.X1-450000 && p.Z>=_area.Value.Z0+450000 && p.Z<=_area.Value.Z1-450000;
        private bool Free(TrialPoint a,TrialPoint b)
        {
            for(int i=0;i<_reserved.Length;i++)if(TrialMap.PointNear(_reserved[i],a,b,800000,false))return false;
            return true;
        }
        private long H(int n)
        {
            // Exact connector-to-destination lower bound; octile costs on the grid.
            long best=long.MaxValue;
            foreach(var goal in _goals) {
                long dx=Math.Abs(n%620-goal.Key%620),dz=Math.Abs(n/620-goal.Key/620);
                long h=Math.Min(dx,dz)*141422+Math.Abs(dx-dz)*100000+goal.Value;
                if(h<best) best=h;
            }
            return best;
        }
        private static bool Before(Entry a,Entry b) => a.G+a.H!=b.G+b.H ? a.G+a.H<b.G+b.H : a.H!=b.H ? a.H<b.H : a.Node<b.Node;
        private void Push(Entry e) {
            int i=_heap.Count; _heap.Add(e);
            while(i>0) {int p=(i-1)/2;if(!Before(e,_heap[p]))break;_heap[i]=_heap[p];i=p;} _heap[i]=e;
        }
        private Entry Pop() {
            var root=_heap[0]; var e=_heap[_heap.Count-1]; _heap.RemoveAt(_heap.Count-1);
            if(_heap.Count==0)return root;
            int i=0;
            while(i*2+1<_heap.Count){int c=i*2+1;if(c+1<_heap.Count&&Before(_heap[c+1],_heap[c]))c++;if(!Before(_heap[c],e))break;_heap[i]=_heap[c];i=c;} _heap[i]=e;return root;
        }
        public int Expand(TrialGrid grid,int budget)
        {
            int used=0;
            while(Status==TrialSearchStatus.Pending && used<budget)
            {
                if(_heap.Count==0 || (_best>=0 && _heap[0].G+_heap[0].H>_bestCost)) { Finish(); break; }
                var e=Pop();
                if(_closed.Contains(e.Node)||_g[e.Node]!=e.G)continue;
                _closed.Add(e.Node); used++; Expansions++;
                if(_goals.TryGetValue(e.Node,out long final)) {
                    long cost=e.G+final;
                    if(cost<_bestCost || (cost==_bestCost && e.Node<_best)) {_best=e.Node;_bestCost=cost;}
                }
                foreach(var next in grid.Neighbours(e.Node,Branch))
                {
                    if(!InArea(TrialGrid.Point(next.Node))||!Free(TrialGrid.Point(e.Node),TrialGrid.Point(next.Node)))continue;
                    long cost=e.G+next.Cost;
                    if(_closed.Contains(next.Node))
                    {if(_g[next.Node]==cost&&e.Node<_parent[next.Node])_parent[next.Node]=e.Node;continue;}
                    if(!_g.TryGetValue(next.Node,out long old)||cost<old) {
                        _g[next.Node]=cost;_parent[next.Node]=e.Node;Push(new Entry(next.Node,cost,H(next.Node)));
                    } else if(cost==old && e.Node<_parent[next.Node]) _parent[next.Node]=e.Node;
                }
            }
            return used;
        }
        private void Finish() {
            if(_best<0){Status=TrialSearchStatus.NoPath;return;}
            var list=new List<TrialPoint>{Destination};
            for(int n=_best;n>=0;n=_parent[n])list.Add(TrialGrid.Point(n));
            list.Add(Start);list.Reverse();Path=new List<TrialPoint>();
            foreach(var p in list)if(Path.Count==0||!Path[Path.Count-1].Equals(p))Path.Add(p);
            Cost=_bestCost;Status=TrialSearchStatus.Found;
        }
        public TrialSearch Clone() {
            var copy=(TrialSearch)MemberwiseClone();
            copy._heap=new List<Entry>(_heap);copy._g=new Dictionary<int,long>(_g);copy._parent=new Dictionary<int,int>(_parent);
            copy._closed=new HashSet<int>(_closed);copy._goals=new Dictionary<int,long>(_goals);copy.Path=new List<TrialPoint>(Path);return copy;
        }
        public string Fingerprint => $"{RequestKey}:{Status}:{Expansions}:{_heap.Count}:{_closed.Count}:{_best}:{_bestCost}:"+
            string.Join(";",_heap.Select(e=>$"{e.Node},{e.G},{e.H}"))+"|"+string.Join(";",_g.OrderBy(e=>e.Key).Select(e=>$"{e.Key},{e.Value},{_parent[e.Key]}"));
    }
}
