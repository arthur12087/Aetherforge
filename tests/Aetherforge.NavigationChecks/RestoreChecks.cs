using Aetherforge.Domain.NavigationTrial;
using System.Collections;
using System.Reflection;

// GAMELAB-70 reproductions promoted to the standard gate. Reflection is confined
// to explicitly synthetic fault fixtures; natural snapshots use the public API.
internal static class RestoreChecks
{
    private const BindingFlags Fields=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    private static object Get(object o,string name)=>o.GetType().GetField(name,Fields)!.GetValue(o)!;
    private static void Set(object o,string name,object value)=>o.GetType().GetField(name,Fields)!.SetValue(o,value);
    internal static List<object> Run(TrialGrid grid,Action<bool,string> check)
    {
        var evidence=new List<object>();
        void Suffix(TrialSimulation s,int ticks,RestoreOracle oracle)
        {
            var snap=s.Checkpoint();string frozen=snap.Fingerprint;
            var a=TrialSimulation.Restore(snap,grid);var b=s.Checkpoint();
            for(int i=0;i<ticks;i++)
            {
                a.Advance(50000);b.Advance(19001);b.Advance(30999);
                check(a.Fingerprint==b.Fingerprint,"F4 partitioned restored suffix");oracle.Observe(a,check);
            }
            check(snap.Fingerprint==frozen&&s.Fingerprint==frozen,"F4 suffix leaves source/snapshot unchanged");
        }
        foreach(long t in new[]{21150000L,21200000L,21250000L,21300000L,21350000L})
        {
            var s=new TrialSimulation(grid,TrialBranch.North);s.Advance(t);
            var r=TrialSimulation.Restore(s.Checkpoint(),grid);
            check(r.Fingerprint==s.Fingerprint,"F4 public equality-clearance checkpoint");
            // Independent grid witness in the CLOSED eroded C rectangle: vertical,
            // horizontal at Z20.05, vertical. No product certificate as oracle.
            var positions=s.Actors.Select(a=>a.At()).ToArray();
            check(positions.All(p=>p.MetresX>=31.45&&p.MetresX<=39.55&&p.MetresZ>=2.45&&p.MetresZ<=25.55),"F4 endpoints in closed C");
            var lengths=new List<double>();
            for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)
            {
                double d=Math.Abs(positions[i].MetresZ-20.05)+Math.Abs(positions[i].MetresX-positions[j].MetresX)+Math.Abs(positions[j].MetresZ-20.05);
                lengths.Add(d);check(d<=8,"F4 independent C path witness below cohesion ceiling");
            }
            var oracle=new RestoreOracle();Suffix(s,240,oracle);
            evidence.Add(new{finding="F4-A-public",t,accepted=true,independentGridWitnessMetres=lengths,oracle.Spans,oracle.MinWall,oracle.MinDisc,oracle.MaxCohesionWitness});
        }
        foreach(var branch in new[]{TrialBranch.North,TrialBranch.South})
        {
            var s=new TrialSimulation(grid,branch);int snapshots=0;var oracle=new RestoreOracle();
            // Include t0 and terminal, plus every natural 50ms checkpoint. Replay
            // each transition with different input partitions, including new stages.
            while(true)
            {
                var snap=s.Checkpoint();string frozen=snap.Fingerprint;var r=TrialSimulation.Restore(snap,grid);
                check(r.Fingerprint==frozen,"F4 all natural checkpoints retain identity");snapshots++;
                if(s.Complete)break;
                check(snapshots<1500&&s.Hold=="","F4 natural journey progresses");
                s.Advance(50000);r.Advance(123);r.Advance(49877);
                check(s.Fingerprint==r.Fingerprint,"F4 every natural transition has deterministic suffix");
                check(snap.Fingerprint==frozen,"F4 natural snapshot detached");oracle.Observe(r,check);
                for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)
                    check(s.DistanceBound(s.Actors[i],s.Actors[j])<=8000000,"F4 every carried cohesion certificate bounded");
            }
            evidence.Add(new{finding="F4-A-all-natural",branch=branch.ToString(),snapshots,s.Complete,s.TimeUs,oracle.Spans,oracle.MinWall,oracle.MinDisc,oracle.MaxCohesionWitness});
        }
        TrialSimulation Fixture(params (decimal x,decimal z,decimal gx,decimal gz)[] paths)
        {
            var s=new TrialSimulation(grid,TrialBranch.North){Budget=0};Set(s,"<Phase>k__BackingField",9);
            for(int i=0;i<3;i++)
            {
                var p=paths[i];var a=s.Actors[i];a.Anchor=TrialPoint.M(p.x,p.z);a.Goal=TrialPoint.M(p.gx,p.gz);a.SafeAnchor=a.Anchor;
                a.Route=new(){a.Anchor,a.Goal};a.Search=null;a.State="Moving";
            }
            return s;
        }
        void Reject(string name,TrialSimulation seed,Action<TrialSimulation> mutate)
        {
            string source=seed.Fingerprint;var bad=seed.Checkpoint();mutate(bad);string corrupt=bad.Fingerprint;bool refused=false;
            try{TrialSimulation.Restore(bad,grid);}catch(InvalidOperationException){refused=true;}
            check(refused,"F4 reject synthetic "+name);
            check(seed.Fingerprint==source&&bad.Fingerprint==corrupt,"F4 atomic rejection "+name);
            evidence.Add(new{finding="F4-B-synthetic",name,rejected=refused,sourceAndInputUnchanged=true});
        }
        var valid=new TrialSimulation(grid,TrialBranch.North){Budget=17};valid.Advance(67321);
        Reject("erased frontier with open work",valid,s=>((IList)Get(s.Actors[0].Search!,"_heap")).Clear());
        Reject("negative queued G",valid,s=>{var heap=(IList)Get(s.Actors[0].Search!,"_heap");var e=heap[0]!;Set(e,"G",-1L);heap[0]=e;});
        Reject("detached root",valid,s=>{var parents=(IDictionary)Get(s.Actors[0].Search!,"_parent");int n=parents.Keys.Cast<int>().First(n=>(int)parents[n]! !=-1);parents[n]=-1;});
        Reject("root connector cost",valid,s=>{var search=s.Actors[0].Search!;var parents=(IDictionary)Get(search,"_parent");var g=(IDictionary)Get(search,"_g");int n=parents.Keys.Cast<int>().First(n=>(int)parents[n]! ==-1);g[n]=(long)g[n]!+1;});
        Reject("non-root edge cost",valid,s=>{var search=s.Actors[0].Search!;var parents=(IDictionary)Get(search,"_parent");var g=(IDictionary)Get(search,"_g");int n=parents.Keys.Cast<int>().First(n=>(int)parents[n]! !=-1);g[n]=(long)g[n]!+1;});
        Reject("closed membership",valid,s=>((HashSet<int>)Get(s.Actors[0].Search!,"_closed")).Clear());
        Reject("missing goal connectors",valid,s=>((IDictionary)Get(s.Actors[0].Search!,"_goals")).Clear());
        Reject("negative pending",valid,s=>s.Actors[0].PendingSince=-1);
        Reject("orphan segment",valid,s=>s.Actors[0].SegmentUs=1);
        Reject("clock alignment",valid,s=>Set(s,"<TimeUs>k__BackingField",50001L));
        Reject("duplicate reservation",valid,s=>{s.Reservations.AddRange(new[]{0,0});Set(s,"<ReservationDirection>k__BackingField",1);});
        Reject("orphan direction",valid,s=>Set(s,"<ReservationDirection>k__BackingField",1));
        Reject("identity overflow",valid,s=>{s.Actors[0].RequestKey=long.MaxValue;s.Actors[0].Search=null;});
        Reject("heading range",valid,s=>s.Actors[0].Heading=new TrialPoint(63000000,0));
        var route=Fixture((10,14,12,14),(10,16,12,16),(10,12,12,12));
        Reject("GoalX15 routeX12",route,s=>s.Actors[0].Goal=TrialPoint.M(15,14));
        Reject("one point false arrival",route,s=>s.Actors[0].Route.RemoveAt(1));
        Reject("unknown actor State",route,s=>s.Actors[0].State="unrecognized");
        Reject("unissued zero pair bounds",route,s=>Set(s,"_pairBounds",new long[3,3]));
        route.Advance(50000);
        Reject("issued pair bound replaced",route,s=>Set(s,"_pairBounds",new long[3,3]));
        Reject("certificate endpoint changed",route,s=>{s.Actors[0].Route[0]=TrialPoint.M(9,14);s.Actors[0].Anchor=s.Actors[0].Route[0];});
        var near=Fixture((10,14,12,14),(2.1m,14,4.1m,14),(3,15,5,15));near.DelayedActor=1;near.DelayUntilUs=1000000;
        Reject("7.90 to 8.04m corrupted-cache reproduction",near,s=>Set(s,"_pairBounds",new long[3,3]));
        // Independent suffix oracle: even a live tampered numeric cache cannot be
        // consumed by MoveInterval without endpoint-bound immutable provenance.
        Set(near,"_pairBounds",new long[3,3]);near.Advance(50000);
        check(Math.Abs(near.Actors[0].At().MetresX-near.Actors[1].At().MetresX)<=8,"F4 cohesion suffix refuses false cache");
        var nearOracle=new RestoreOracle();nearOracle.Observe(near,check);
        var queue=Fixture((25,20.45m,29,20.45m),(24,21.15m,28,21.15m),(23,20.8m,27,20.8m));queue.Advance(50000);
        Reject("reversed tokens",queue,s=>s.Reservations.Reverse());
        Reject("opposite token direction",queue,s=>Set(s,"<ReservationDirection>k__BackingField",-1));
        var equality=Fixture((25,20.45m,29,20.45m),(24,20.45m,28,20.45m),(23,20.45m,27,20.45m));equality.Advance(50000);
        Suffix(equality,50,new RestoreOracle());
        // One-node area makes a real Pending/empty-heap budget boundary, with a
        // reached goal. It must survive restore and finish at the next expansion.
        var boundary=Fixture((10.05m,14.05m,10.05m,14.05m),(10,16,12,16),(10,12,12,12));
        var actor=boundary.Actors[0];actor.Route.Clear();actor.State="PathPending";
        actor.Search=new TrialSearch(grid,actor.Anchor,actor.Goal,boundary.Branch,actor.RequestKey,area:new TrialRect("single",9.6m,13.6m,10.5m,14.5m));
        actor.Search.Expand(grid,1);
        check(actor.Search.Status==TrialSearchStatus.Pending&&actor.Search.OpenCount==0,"F4 real Pending empty frontier at budget boundary");
        var boundaryCopy=TrialSimulation.Restore(boundary.Checkpoint(),grid);boundaryCopy.Budget=512;boundaryCopy.Advance(50000);
        check(boundaryCopy.Actors[0].Arrived&&boundaryCopy.Actors[0].Anchor.Equals(actor.Goal),"F4 empty-frontier Pending finishes correctly");
        evidence.Add(new{finding="F4-B-valid",pendingEmptyFrontier=true,equalityClearance=true});
        // Tail replans and reversal intentionally retain an older route endpoint.
        foreach(bool reverse in new[]{false,true})
        {
            var s=Fixture((10,14,12,14),(10,16,12,16),(10,12,12,12));s.Advance(50000);var a=s.Actors[0];
            a.Goal=TrialPoint.M(15,14);a.Reversing=reverse;
            a.Search=new TrialSearch(grid,reverse?a.Route[0]:a.Route[1],a.Goal,s.Branch,++a.RequestKey);
            a.ReplanTail=!reverse;
            Suffix(s,25,new RestoreOracle());
            s.Advance(1000000);
            check(!a.Arrived,"F4 unfinished replan cannot declare arrival at old endpoint");
            var resumed=TrialSimulation.Restore(s.Checkpoint(),grid);resumed.Budget=512;resumed.Advance(2500000);
            check(resumed.Actors[0].Arrived&&resumed.Actors[0].Anchor.Equals(a.Goal),"F4 pending tail/reversal publishes new goal after restore");
            evidence.Add(new{finding="F4-B-valid",tailReplan=!reverse,reversal=reverse});
        }
        // Explicit exhaustion, without signed wraparound or a partially cleared route.
        var exhausted=new TrialSimulation(grid,TrialBranch.North){Budget=1};exhausted.Advance(50000);
        exhausted.Actors[0].RequestKey=long.MaxValue-1;bool stopped=false;
        try{exhausted.Advance(100000);}catch(InvalidOperationException e){stopped=e.Message.Contains("identity exhausted");}
        check(stopped&&exhausted.Actors[0].RequestKey==long.MaxValue-1,"F4 explicit identity exhaustion before overflow");
        return evidence;
    }
}
