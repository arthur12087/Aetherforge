using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Aetherforge.Domain.NavigationTrial
{
    // Logical units are micrometres. No Godot types or floating predicates.
    public readonly struct TrialPoint : IEquatable<TrialPoint>
    {
        public readonly long X, Z;
        public TrialPoint(long x, long z) { X = x; Z = z; }
        public static TrialPoint M(decimal x, decimal z) => new TrialPoint((long)(x * 1000000), (long)(z * 1000000));
        public bool Equals(TrialPoint p) => X == p.X && Z == p.Z;
        public override bool Equals(object? o) => o is TrialPoint p && Equals(p);
        public override int GetHashCode() => X.GetHashCode() * 397 ^ Z.GetHashCode();
        public override string ToString() => $"{X},{Z}";
        public static long Length(TrialPoint a, TrialPoint b) => RootCeil((a.X-b.X)*(a.X-b.X)+(a.Z-b.Z)*(a.Z-b.Z));
        public static long RootCeil(long n)
        {
            long lo = 0, hi = Math.Min(n, 100000000) + 1;
            while (lo + 1 < hi) { long m = (lo + hi) / 2; if (m*m >= n) hi=m; else lo=m; }
            return n == 0 ? 0 : hi;
        }
    }

    public readonly struct TrialRect
    {
        public readonly string Id;
        public readonly long X0, Z0, X1, Z1;
        public TrialRect(string id, decimal x0, decimal z0, decimal x1, decimal z1)
        { Id=id; X0=(long)(x0*1000000); Z0=(long)(z0*1000000); X1=(long)(x1*1000000); Z1=(long)(z1*1000000); }
        public bool Contains(TrialPoint p) => p.X >= X0 && p.X <= X1 && p.Z >= Z0 && p.Z <= Z1;
    }

    public sealed class TrialMap
    {
        public const string Identity = "fornalha-c1/revision-1";
        public readonly IReadOnlyList<TrialRect> Floor = Array.AsReadOnly(new[] {
            new TrialRect("A",0,10,8,18), new TrialRect("B",8,2,23,26),
            new TrialRect("N",23,20,31,21.6m), new TrialRect("S",23,4,31,8),
            new TrialRect("C",31,2,40,26), new TrialRect("D",40,8,48,20), new TrialRect("F",48,2,62,26) });
        private readonly List<(TrialPoint A, TrialPoint B)> _boundary = new List<(TrialPoint,TrialPoint)>();
        public IReadOnlyList<(TrialPoint A,TrialPoint B)> Boundary { get; }
        public string Hash { get; }
        public long SegmentQueries { get; private set; }
        public long PredicateTests { get; private set; }

        public TrialMap()
        {
            Boundary=_boundary.AsReadOnly();
            // Partition the UNION; shared portals produce no boundary edge.
            var xs=Floor.SelectMany(r=>new[]{r.X0,r.X1}).Distinct().OrderBy(x=>x).ToArray();
            var zs=Floor.SelectMany(r=>new[]{r.Z0,r.Z1}).Distinct().OrderBy(x=>x).ToArray();
            bool[,] cells=new bool[xs.Length-1,zs.Length-1];
            for(int i=0;i<xs.Length-1;i++) for(int j=0;j<zs.Length-1;j++)
                cells[i,j]=Floor.Any(r=>r.Contains(new TrialPoint((xs[i]+xs[i+1])/2,(zs[j]+zs[j+1])/2)));
            for(int i=0;i<xs.Length-1;i++) for(int j=0;j<zs.Length-1;j++) if(cells[i,j])
            {
                if(i==0 || !cells[i-1,j]) _boundary.Add((new TrialPoint(xs[i],zs[j]),new TrialPoint(xs[i],zs[j+1])));
                if(i==xs.Length-2 || !cells[i+1,j]) _boundary.Add((new TrialPoint(xs[i+1],zs[j]),new TrialPoint(xs[i+1],zs[j+1])));
                if(j==0 || !cells[i,j-1]) _boundary.Add((new TrialPoint(xs[i],zs[j]),new TrialPoint(xs[i+1],zs[j])));
                if(j==zs.Length-2 || !cells[i,j+1]) _boundary.Add((new TrialPoint(xs[i],zs[j+1]),new TrialPoint(xs[i+1],zs[j+1])));
            }
            using(var sha=SHA256.Create()) Hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(
                Identity+string.Join(";",Floor.Select(r=>$"{r.Id}:{r.X0},{r.Z0},{r.X1},{r.Z1}"))))).Replace("-","").ToLowerInvariant();
        }

        public bool Inside(TrialPoint p) => Floor.Any(r=>r.Contains(p));
        public bool Clear(TrialPoint p, long radius=450000)
        {
            if(!Inside(p)) return false;
            foreach(var e in Boundary) if(PointNear(p,e.A,e.B,radius,false)) return false;
            return true;
        }
        public bool Segment(TrialPoint a, TrialPoint b, long clearance=450000)
        {
            SegmentQueries++;
            if(!Clear(a,clearance) || !Clear(b,clearance)) return false;
            foreach(var e in Boundary)
            {
                // Broad phase also skips costly rational products for distant edges.
                if(Math.Max(a.X,b.X)+clearance < Math.Min(e.A.X,e.B.X) || Math.Min(a.X,b.X)-clearance > Math.Max(e.A.X,e.B.X) ||
                   Math.Max(a.Z,b.Z)+clearance < Math.Min(e.A.Z,e.B.Z) || Math.Min(a.Z,b.Z)-clearance > Math.Max(e.A.Z,e.B.Z)) continue;
                PredicateTests++;
                if(Intersects(a,b,e.A,e.B) || PointNear(a,e.A,e.B,clearance,false) || PointNear(b,e.A,e.B,clearance,false) ||
                    PointNear(e.A,a,b,clearance,false) || PointNear(e.B,a,b,clearance,false)) return false;
            }
            return true;
        }
        private static long Cross(TrialPoint a,TrialPoint b,TrialPoint c) => (b.X-a.X)*(c.Z-a.Z)-(b.Z-a.Z)*(c.X-a.X);
        private static bool Intersects(TrialPoint a,TrialPoint b,TrialPoint c,TrialPoint d)
        {
            if(Math.Max(a.X,b.X)<Math.Min(c.X,d.X)||Math.Max(c.X,d.X)<Math.Min(a.X,b.X)||
                Math.Max(a.Z,b.Z)<Math.Min(c.Z,d.Z)||Math.Max(c.Z,d.Z)<Math.Min(a.Z,b.Z)) return false;
            return Math.Sign(Cross(a,b,c))*Math.Sign(Cross(a,b,d))<=0 && Math.Sign(Cross(c,d,a))*Math.Sign(Cross(c,d,b))<=0;
        }
        public static bool PointNear(TrialPoint p,TrialPoint a,TrialPoint b,long radius,bool inclusive)
        {
            long dx=b.X-a.X,dz=b.Z-a.Z,px=p.X-a.X,pz=p.Z-a.Z;
            long len=dx*dx+dz*dz,dot=px*dx+pz*dz,r2=radius*radius;
            BigInteger lhs,rhs;
            if(dot<=0 || len==0) { lhs=(BigInteger)px*px+(BigInteger)pz*pz; rhs=r2; }
            else if(dot>=len) { lhs=(BigInteger)(p.X-b.X)*(p.X-b.X)+(BigInteger)(p.Z-b.Z)*(p.Z-b.Z); rhs=r2; }
            else { long cross=dx*pz-dz*px; lhs=(BigInteger)cross*cross; rhs=(BigInteger)r2*len; }
            return inclusive ? lhs<=rhs : lhs<rhs;
        }
        // Continuous relative swept-disc predicate. Used for the whole intention interval.
        public static bool Separated(TrialPoint a0,TrialPoint a1,TrialPoint b0,TrialPoint b1,long minimum=800000) =>
            !PointNear(new TrialPoint(0,0),new TrialPoint(a0.X-b0.X,a0.Z-b0.Z),new TrialPoint(a1.X-b1.X,a1.Z-b1.Z),minimum,false);
    }
}
