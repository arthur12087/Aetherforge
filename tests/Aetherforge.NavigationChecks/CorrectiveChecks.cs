using Aetherforge.Domain.NavigationTrial;
using System.Reflection;

// Independent injected fixtures from the GAMELAB-69 reproductions. No production hook.
internal static class CorrectiveChecks
{
    private static void Set(object o,string name,object value)=>o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(o,value);
    private static object Get(object o,string name)=>o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(o)!;
    internal static List<object> Run(TrialGrid grid,Action<bool,string> check)
    {
        var evidence=new List<object>();
        TrialSimulation Fixture(params (TrialPoint from,TrialPoint to)[] paths)
        {
            var s=new TrialSimulation(grid,TrialBranch.North){Budget=0};
            for(int i=0;i<3;i++)
            {
                var a=s.Actors[i];a.Anchor=paths[i].from;a.Goal=paths[i].to;a.SafeAnchor=a.Anchor;
                a.Route=new(){a.Anchor,a.Goal};a.Search=null;a.State="Moving";
                check(grid.Map.Segment(a.Anchor,a.Goal),"Fixture clearance");
            }
            for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)check(TrialPoint.Length(paths[i].from,paths[j].from)>=800000,"Fixture separation");
            return s;
        }
        void Suffix(TrialSimulation s,int ticks)
        {
            var snap=s.Checkpoint();string frozen=snap.Fingerprint;
            var copy=TrialSimulation.Restore(snap,grid);
            for(int i=0;i<ticks;i++){s.Advance(50000);copy.Advance(123);copy.Advance(49877);check(s.Fingerprint==copy.Fingerprint,"Corrective checkpoint suffix");}
            check(snap.Fingerprint==frozen,"Corrective snapshot detached");
        }
        foreach(bool obstruction in new[]{false,true})
        {
            TrialSimulation New()=>new(grid,TrialBranch.North){Budget=obstruction?512:1,Obstructed=obstruction};
            var bulk=New();var split=New();bulk.Advance(obstruction?12000000:3000000);
            for(int i=0;i<300&&split.Hold.Length==0;i++)split.Advance(50000);
            check(bulk.Hold==(obstruction?"NavigationBlocked":"NavigationBudgetExceeded"),"F1 both Hold reasons reached");
            check(bulk.Fingerprint==split.Fingerprint,"F1 Hold independent of input partition");
            check((long)Get(bulk,"_inputUs")==0,"F1 Hold has no time debt");
            var restored=TrialSimulation.Restore(bulk.Checkpoint(),grid);
            long time=bulk.TimeUs;string[] at=bulk.Actors.Select(a=>a.At().ToString()).ToArray();
            foreach(var s in new[]{bulk,split,restored}){s.Budget=512;s.Obstructed=false;check(s.Resume(),"F1 Resume");s.Advance(0);check(s.TimeUs==time&&at.SequenceEqual(s.Actors.Select(a=>a.At().ToString())),"F1 Resume/Advance(0) cannot move");}
            bulk.Advance(17321);restored.Advance(17321);check((long)Get(bulk,"_inputUs")==17321,"F1 new legitimate carry retained");
            var carry=TrialSimulation.Restore(bulk.Checkpoint(),grid);check(carry.Fingerprint==bulk.Fingerprint,"F1 marching carry restored");
            bulk.Advance(32679);restored.Advance(32679);carry.Advance(10000);carry.Advance(22679);
            check(bulk.Fingerprint==restored.Fingerprint&&bulk.Fingerprint==carry.Fingerprint,"F1 delta after Resume consumed once");
            evidence.Add(new{finding="F1",obstruction,holdAt=time,carry=17321,advanceZeroMoved=false});
        }
        var nearest=Fixture((TrialPoint.M(22.4m,20.45m),TrialPoint.M(23.5m,20.45m)),(TrialPoint.M(22.8m,21.15m),TrialPoint.M(23.8m,21.15m)),(TrialPoint.M(20,20.8m),TrialPoint.M(21,20.8m)));
        nearest.Advance(50000);
        check(nearest.Reservations.SequenceEqual(new[]{1}),"F2 physical occupant owns the committed token, rejected applicant owns none");
        check(nearest.Actors[1].At().MetresX>22.8&&nearest.Actors[0].At().MetresX==22.4,"F2 occupied lane progresses without overtaking");
        evidence.Add(new{finding="F2",reservations=nearest.Reservations.ToArray(),actors=nearest.Actors.Select(a=>new{a.Id,a.State,x=a.At().MetresX,z=a.At().MetresZ}).ToArray()});
        Suffix(nearest,4);
        var nearestWest=Fixture((TrialPoint.M(31.6m,20.45m),TrialPoint.M(30.5m,20.45m)),(TrialPoint.M(31.2m,21.15m),TrialPoint.M(30.2m,21.15m)),(TrialPoint.M(34,20.8m),TrialPoint.M(33,20.8m)));
        nearestWest.Advance(50000);
        check(nearestWest.Reservations.SequenceEqual(new[]{1})&&nearestWest.ReservationDirection==-1,"F2 mirrored west occupant wins");
        Suffix(nearestWest,4);
        foreach(int dir in new[]{1,-1})
        {
            decimal x=dir>0?23:31;
            var occupants=Fixture((TrialPoint.M(x,20.8m),TrialPoint.M(x+dir*3,20.8m)),(TrialPoint.M(x+dir,20.8m),TrialPoint.M(x+dir*4,20.8m)),(TrialPoint.M(x-dir*2,20.8m),TrialPoint.M(x-dir,20.8m)));
            occupants.Advance(50000);
            check(occupants.Reservations.SequenceEqual(new[]{1,0}),"F2 multiple unaligned occupants commit physical order before role");
            Suffix(occupants,4);
        }
        // No occupant to prioritize: the candidate rejected by collision gets no token.
        var rejected=Fixture((TrialPoint.M(22.4m,20.8m),TrialPoint.M(24,20.8m)),(TrialPoint.M(23.3m,20.8m),TrialPoint.M(24.5m,20.8m)),(TrialPoint.M(20,20.8m),TrialPoint.M(21,20.8m)));
        rejected.DelayedActor=1;rejected.DelayUntilUs=1000000;rejected.Advance(50000);
        check(!rejected.Reservations.Contains(0),"F2 rejected intention cannot commit admission");
        var waiting=Fixture((TrialPoint.M(10,14),TrialPoint.M(12,14)),(TrialPoint.M(11,14),TrialPoint.M(9,14)),(TrialPoint.M(10,16),TrialPoint.M(12,16)));
        waiting.Actors[1].BlockedUs=900000;waiting.Advance(50000);
        check(waiting.Actors[1].At().MetresX<11&&waiting.Actors[0].At().MetresX==10,"F3 older waiter precedes role outside N");
        evidence.Add(new{finding="F3-wait",actors=waiting.Actors.Select(a=>new{a.Id,a.State,x=a.At().MetresX,a.WaitUs}).ToArray()});
        foreach(int dir in new[]{1,-1})
        {
            decimal x=dir>0?22:32;
            var lane=Fixture(Enumerable.Range(0,3).Select(i=>(TrialPoint.M(x-dir*i,20.8m),TrialPoint.M(dir>0?34-i:19+i,20.8m))).ToArray());
            lane.Advance(500000);check(lane.ReservationDirection==dir,"F3 reservation direction follows route");
            var admitted=lane.Reservations.ToArray();Suffix(lane,15);
            bool released=false;
            for(int n=0;n<150&&!released;n++)
            {
                lane.Advance(50000);
                for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)check(TrialRational.Swept(lane.Actors[i].At(),lane.Actors[i].At(),lane.Actors[j].At(),lane.Actors[j].At()),"F3 separation while traversing");
                released=lane.Reservations.Count==0;
            }
            check(released&&lane.ReservationDirection==0,$"F3 last actor releases correct exit dir={dir} {lane.Hold} {string.Join(',',lane.Actors.Select(a=>$"{a.At().MetresX}:{a.State}"))}");
            check(lane.Actors.All(a=>dir>0?a.At().MetresX>=31.5:a.At().MetresX<=22.5),"F3 no early release");
            evidence.Add(new{finding="F3-lane",direction=dir,admitted,released,lane.TimeUs,positions=lane.Actors.Select(a=>a.At().MetresX).ToArray()});
        }
        var roundTrip=Fixture((TrialPoint.M(22,20.8m),TrialPoint.M(34,20.8m)),(TrialPoint.M(21,20.8m),TrialPoint.M(33,20.8m)),(TrialPoint.M(20,20.8m),TrialPoint.M(32,20.8m)));
        Set(roundTrip,"<Phase>k__BackingField",9); // End at reunion, without requesting another pilot stage.
        for(int i=0;i<150&&!roundTrip.Complete;i++)roundTrip.Advance(50000);
        check(roundTrip.Complete&&roundTrip.Reservations.Count==0,"F3 complete east traverse/reunion before explicit return");
        Set(roundTrip,"<Phase>k__BackingField",9);
        foreach(var a in roundTrip.Actors){a.Route=new(){a.Anchor,TrialPoint.M(21-a.Id,20.8m)};a.Goal=a.Route[1];a.Index=1;a.SegmentUs=0;a.Search=null;}
        roundTrip.Advance(500000);
        check(roundTrip.ReservationDirection==-1&&roundTrip.Reservations[0]==2,"F3 explicit return admits nearest west actor, reversing queue order");
        Suffix(roundTrip,60);
        for(int i=0;i<150&&!roundTrip.Complete;i++)roundTrip.Advance(50000);
        check(roundTrip.Complete&&roundTrip.Reservations.Count==0,"F3 explicit round trip completed");
        evidence.Add(new{finding="F3-explicit-return",roundTrip.Complete,roundTrip.TimeUs,positions=roundTrip.Actors.Select(a=>a.At().MetresX).ToArray(),automaticRecovery=false});
        var stale=new TrialSimulation(grid,TrialBranch.North){Budget=1};stale.Advance(50000);stale.Actors[0].RequestKey++;
        var old=stale.Actors[0].Search;long invalidatedKey=stale.Actors[0].RequestKey;
        stale.Advance(100000);check(!ReferenceEquals(stale.Actors[0].Search,old)&&stale.Actors[0].Search?.RequestKey>invalidatedKey&&stale.Actors[0].Route.Count==0,"F4 stale request cancelled before publication, fresh request keeps progress possible");
        Suffix(stale,2);evidence.Add(new{finding="F4-stale",cancelled=true,stale.TimeUs});
        var valid=new TrialSimulation(grid,TrialBranch.North){Budget=17};valid.Advance(67321);
        var completed=new TrialSimulation(grid,TrialBranch.South);completed.Advance(50000000);
        check(completed.Complete&&TrialSimulation.Restore(completed.Checkpoint(),grid).Fingerprint==completed.Fingerprint,"F4 valid terminal checkpoint retains unconsumed terminal tail");
        Suffix(valid.Checkpoint(),9);
        var corruptions=new (string name,Action<TrialSimulation> apply)[]{
            ("solid anchor",s=>{s.Actors[0].Anchor=TrialPoint.M(27,14);s.Actors[0].Route.Clear();s.Actors[0].Search=null;}),
            ("duplicate id",s=>s.Actors[1].Id=0),("negative clock",s=>Set(s,"_inputUs",-1L)),
            ("carry overflow",s=>Set(s,"_inputUs",50000L)),("invalid cursor",s=>Set(s,"_cursor",3)),
            ("invalid index",s=>s.Actors[0].Index=500),("negative segment",s=>s.Actors[0].SegmentUs=-1),
            ("unknown token",s=>s.Reservations.Add(9)),("missing direction",s=>s.Reservations.Add(0)),
            ("stale search",s=>s.Actors[0].RequestKey++),("future pending",s=>s.Actors[0].PendingSince=s.TimeUs+1),
            ("invalid heap",s=>Set(s.Actors[0].Search!,"_best",TrialGrid.Count)),
            ("unsafe route",s=>{s.Actors[0].Route=new(){TrialPoint.M(21,14),TrialPoint.M(33,14)};s.Actors[0].Anchor=s.Actors[0].Route[0];})};
        foreach(var fault in corruptions)
        {
            var candidate=valid.Checkpoint();fault.apply(candidate);string before=valid.Fingerprint;bool refused=false;
            try{TrialSimulation.Restore(candidate,grid);}catch(InvalidOperationException){refused=true;}
            check(refused,"F4 reject "+fault.name);check(valid.Fingerprint==before,"F4 rejection is atomic");
            evidence.Add(new{finding="F4-restore",fault=fault.name,rejected=refused,originalUnchanged=true});
        }
        return evidence;
    }
}
