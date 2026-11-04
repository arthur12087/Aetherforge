using Aetherforge.Domain.NavigationTrial;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

// Continuous oracle adapted from Technical Lead's independent GAMELAB-69 harness.
// Literal floor complement and double geometry; never calls TrialMap.Segment/Swept.
internal static class ContinuousChecks
{
    internal static List<object> Run(Action<bool,string> check)
    {
        var rectangles=new(double x0,double z0,double x1,double z1)[]{(0,10,8,18),(8,2,23,26),(23,20,31,21.6),(23,4,31,8),(31,2,40,26),(40,8,48,20),(48,2,62,26)};
        double[] xs={-1,0,8,23,31,40,48,62,63},zs={-1,2,4,8,10,18,20,21.6,26,29};
        var solids=new List<(double x0,double z0,double x1,double z1)>();
        for(int i=0;i<xs.Length-1;i++)for(int j=0;j<zs.Length-1;j++)
        {
            double x=(xs[i]+xs[i+1])/2,z=(zs[j]+zs[j+1])/2;
            if(!rectangles.Any(r=>x>r.x0&&x<r.x1&&z>r.z0&&z<r.z1))solids.Add((xs[i],zs[j],xs[i+1],zs[j+1]));
        }
        double PointSegment(double x,double z,double ax,double az,double bx,double bz)
        {
            double dx=bx-ax,dz=bz-az,t=dx*dx+dz*dz==0?0:Math.Clamp(((x-ax)*dx+(z-az)*dz)/(dx*dx+dz*dz),0,1);
            return Math.Sqrt(Math.Pow(x-ax-t*dx,2)+Math.Pow(z-az-t*dz,2));
        }
        double PointBox(double x,double z,(double x0,double z0,double x1,double z1) r)=>Math.Sqrt(Math.Pow(Math.Max(0,Math.Max(r.x0-x,x-r.x1)),2)+Math.Pow(Math.Max(0,Math.Max(r.z0-z,z-r.z1)),2));
        double SegmentBox(TrialRational a,TrialRational b,(double x0,double z0,double x1,double z1) r)
        {
            double ax=a.MetresX,az=a.MetresZ,bx=b.MetresX,bz=b.MetresZ,lo=0,hi=1;
            bool Clip(double start,double end,double low,double high)
            {
                if(end==start)return start>=low&&start<=high;
                double t0=(low-start)/(end-start),t1=(high-start)/(end-start);lo=Math.Max(lo,Math.Min(t0,t1));hi=Math.Min(hi,Math.Max(t0,t1));return lo<=hi;
            }
            if(Clip(ax,bx,r.x0,r.x1)&&Clip(az,bz,r.z0,r.z1))return 0;
            return new[]{PointBox(ax,az,r),PointBox(bx,bz,r),PointSegment(r.x0,r.z0,ax,az,bx,bz),PointSegment(r.x1,r.z0,ax,az,bx,bz),PointSegment(r.x0,r.z1,ax,az,bx,bz),PointSegment(r.x1,r.z1,ax,az,bx,bz)}.Min();
        }
        var results=new List<object>();var sw=new Stopwatch();
        foreach(var branch in new[]{TrialBranch.North,TrialBranch.South})
        {
            sw.Start();var grid=new TrialGrid(new TrialMap());double construction=sw.Elapsed.TotalMilliseconds;
            var s=new TrialSimulation(grid,branch);var times=new List<double>();
            double minDisc=double.MaxValue,minWall=double.MaxValue;int spans=0,expansions=0;
            while(!s.Complete&&s.Hold.Length==0&&times.Count<2500)
            {
                sw.Restart();s.Advance(50000);times.Add(sw.Elapsed.TotalMilliseconds);expansions=Math.Max(expansions,s.LastExpansions);
                foreach(var span in (IEnumerable)typeof(TrialSimulation).GetField("_motion",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(s)!)
                {
                    spans++;var from=(TrialRational[])span.GetType().GetField("From")!.GetValue(span)!;var to=(TrialRational[])span.GetType().GetField("To")!.GetValue(span)!;
                    for(int i=0;i<3;i++)
                    {
                        foreach(var r in solids)minWall=Math.Min(minWall,SegmentBox(from[i],to[i],r));
                        for(int j=i+1;j<3;j++)minDisc=Math.Min(minDisc,PointSegment(0,0,from[i].MetresX-from[j].MetresX,from[i].MetresZ-from[j].MetresZ,to[i].MetresX-to[j].MetresX,to[i].MetresZ-to[j].MetresZ));
                    }
                    check(minWall>=.449998&&minDisc>=.799998,"Independent continuous clearance/separation");
                }
            }
            check(s.Complete&&expansions<=512,"Cold default journey completes within budget");
            var sorted=times.Order().ToArray();double P(double p)=>sorted[Math.Min(sorted.Length-1,(int)(sorted.Length*p))];
            results.Add(new{branch=branch.ToString(),s.Complete,s.TimeUs,constructionMs=construction,advanceTotalMs=new{count=times.Count,p50=P(.5),p95=P(.95),p99=P(.99),max=times.Max()},maxExpansions=expansions,spans,minWallMetres=minWall,minSeparationMetres=minDisc,toleranceMetres=.000002,queries=grid.Map.SegmentQueries,predicates=grid.Map.PredicateTests,cacheHits=grid.EdgeCacheHits});
            sw.Reset();
        }
        return results;
    }
}
