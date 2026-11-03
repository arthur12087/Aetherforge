using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Aetherforge.Domain.NavigationTrial
{
    public readonly struct TrialRational
    {
        public readonly BigInteger X,Z,D;
        public TrialRational(BigInteger x,BigInteger z,BigInteger d) { X=x;Z=z;D=d; }
        public TrialRational(TrialPoint p) : this(p.X,p.Z,1) { }
        public TrialPoint Rounded => new TrialPoint((long)(X/D),(long)(Z/D));
        public double MetresX => (double)X/(double)D/1000000;
        public double MetresZ => (double)Z/(double)D/1000000;
        public override string ToString() => $"{X}/{D},{Z}/{D}";
        public static TrialRational Blend(TrialRational a,TrialRational b,long t,long duration)
        {
            if(duration==0||t>=duration)return b;
            if(t<=0)return a;
            BigInteger d=a.D*b.D*duration;
            return new TrialRational(a.X*b.D*(duration-t)+b.X*a.D*t,a.Z*b.D*(duration-t)+b.Z*a.D*t,d);
        }
        public static bool Swept(TrialRational a,TrialRational a1,TrialRational b,TrialRational b1,long radius=800000)
        {
            BigInteger d=a.D*a1.D*b.D*b1.D;
            BigInteger x=a.X*(d/a.D)-b.X*(d/b.D), z=a.Z*(d/a.D)-b.Z*(d/b.D);
            BigInteger vx=a1.X*(d/a1.D)-b1.X*(d/b1.D)-x, vz=a1.Z*(d/a1.D)-b1.Z*(d/b1.D)-z;
            BigInteger l=vx*vx+vz*vz,dot=-(x*vx+z*vz),r=(BigInteger)radius*radius*d*d;
            if(l==0||dot<=0)return x*x+z*z>=r;
            if(dot>=l)return (x+vx)*(x+vx)+(z+vz)*(z+vz)>=r;
            BigInteger cross=x*vz-z*vx;
            return cross*cross>=r*l;
        }
    }

    public sealed class TrialActor
    {
        public int Id;
        public List<TrialPoint> Route=new List<TrialPoint>();
        public int Index=1;
        public long SegmentUs, PendingSince, BlockedUs, RequestKey;
        public TrialPoint Anchor,Goal,SafeAnchor,Heading=new TrialPoint(1000000,0);
        public string State="PathPending";
        public int Recovery;
        public bool Reversing,ReplanTail,LeashReturning;
        public TrialSearch? Search;
        public bool Arrived => Route.Count>0 && Index>=Route.Count;
        public long Duration => Route.Count>Index ? (TrialPoint.Length(Route[Index-1],Route[Index])*1000000+2799999)/2800000 : 0;
        public TrialRational At(long extra=0)
        {
            if(Route.Count<=Index)return new TrialRational(Anchor);
            var p=Route[Index-1];var q=Route[Index];
            long t=Math.Max(0,Math.Min(Duration,SegmentUs+(Reversing?-extra:extra))),length=TrialPoint.Length(p,q);
            if(t>=Duration || length==0)return new TrialRational(q);
            BigInteger d=(BigInteger)length*1000000;
            return new TrialRational(p.X*d+(BigInteger)(q.X-p.X)*2800000*t,p.Z*d+(BigInteger)(q.Z-p.Z)*2800000*t,d);
        }
        public void Move(long us)
        {
            if(Route.Count<=Index)return;
            Heading=new TrialPoint(Route[Index].X-Route[Index-1].X,Route[Index].Z-Route[Index-1].Z);
            if(Reversing)
            {
                Heading=new TrialPoint(-Heading.X,-Heading.Z);SegmentUs=Math.Max(0,SegmentUs-us);
                State="RecoveryBacktrack";
                if(SegmentUs==0)
                {
                    Anchor=Route[Index-1];
                    if(LeashReturning&&Index>1){Index--;SegmentUs=Duration;}
                    else {Reversing=false;Route.Clear();Index=1;State=LeashReturning?"LeashReturned":"PathPending";}
                }
                return;
            }
            SegmentUs+=us;
            if(SegmentUs>=Duration){Anchor=Route[Index];Index++;SegmentUs=0;}
            State=Arrived?"Arrived":"Moving";BlockedUs=0;
        }
        public TrialActor Clone() {
            var a=(TrialActor)MemberwiseClone();a.Route=new List<TrialPoint>(Route);a.Search=Search?.Clone();return a;
        }
        public Dictionary<TrialPoint,long> Witnesses()
        {
            var result=new Dictionary<TrialPoint,long>();
            if(Route.Count==0){result[Anchor]=0;return result;}
            var p=At().Rounded;
            int prev=Math.Min(Index-1,Route.Count-1);
            long d=TrialPoint.Length(p,Route[prev])+4;
            result[Route[prev]]=d;
            for(int i=prev-1;i>=0;i--){d+=TrialPoint.Length(Route[i],Route[i+1]);result[Route[i]]=d;}
            if(Index<Route.Count) {
                d=TrialPoint.Length(p,Route[Index])+4;result[Route[Index]]=d;
                for(int i=Index+1;i<Route.Count;i++){d+=TrialPoint.Length(Route[i-1],Route[i]);result[Route[i]]=d;}
            }
            return result;
        }
    }

    // Volatile, isolated pilot state. Does not serialize or touch production saves.
    public sealed class TrialSimulation
    {
        private sealed class MotionSpan
        {
            public long Start,Duration;
            public TrialRational[] From=Array.Empty<TrialRational>(),To=Array.Empty<TrialRational>();
        }
        private List<MotionSpan> _motion=new List<MotionSpan>();
        private long[,] _pairBounds=new long[3,3];
        public readonly TrialGrid Grid;
        public TrialBranch Branch { get; private set; }
        public TrialActor[] Actors { get; private set; }
        public long TimeUs { get; private set; }
        private long _inputUs;
        public long MapGeneration { get; private set; }=1;
        public int Phase { get; private set; }
        public bool Paused { get; set; }
        public string Hold { get; private set; }="";
        public bool Complete => Phase>=_stops.Length;
        public List<int> Reservations { get; private set; }=new List<int>();
        public int ReservationDirection { get; private set; }
        // Measured pilot calibration within C2's <=8192 expansion ceiling.
        public int Budget { get; set; }=512;
        public bool Obstructed { get; set; }
        public int LastExpansions { get; private set; }
        public int PeakReservations { get; private set; }
        public long CohesionWaits { get; private set; }
        public long CollisionWaits { get; private set; }
        public int LeashedActor { get; private set; }=-1;
        public int DelayedActor { get; set; }=-1;
        public long DelayUntilUs { get; set; }
        private TrialRect _leashArea;
        private int _cursor;
        private TrialPoint[] _stops;
        private bool _resumed;
        private bool _regroupRequested,_regrouping,_resumeAfterRegroup;
        private long _regroupUs;
        private TrialPoint[] _originalGoals=Array.Empty<TrialPoint>();
        public TrialSimulation(TrialGrid grid,TrialBranch branch)
        {
            Grid=grid;Branch=branch;
            _stops=new[]{TrialPoint.M(11,18),TrialPoint.M(17,8),TrialPoint.M(21,14),
                TrialPoint.M(21,branch==TrialBranch.North?20.8m:6),TrialPoint.M(23.6m,branch==TrialBranch.North?20.8m:6),
                TrialPoint.M(30.4m,branch==TrialBranch.North?20.8m:6),TrialPoint.M(33,branch==TrialBranch.North?20.8m:6),
                TrialPoint.M(33,14),TrialPoint.M(41.5m,14),TrialPoint.M(50,14)};
            Actors=new[]{new TrialActor{Id=0,Anchor=TrialPoint.M(4,14)},new TrialActor{Id=1,Anchor=TrialPoint.M(3,15)},new TrialActor{Id=2,Anchor=TrialPoint.M(3,13)}};
            foreach(var a in Actors)a.SafeAnchor=a.Anchor;
            for(int i=0;i<3;i++)for(int j=0;j<3;j++)_pairBounds[i,j]=i==j?0:long.MaxValue;
            Request();
        }
        private void Request()
        {
            if(Complete)return;
            foreach(var a in Actors)
            {
                var goal=_stops[Phase];
                bool lane=Branch==TrialBranch.North && Phase>=3 && Phase<=6;
                a.Goal=new TrialPoint(goal.X-(lane?a.Id*1000000:a.Id==0?0:1000000),goal.Z+(lane?0:a.Id==1?800000:a.Id==2?-800000:0));
            }
            foreach(var a in Actors)
            {
                a.Search=new TrialSearch(Grid,a.Anchor,a.Goal,Branch,++a.RequestKey,Actors.Where(b=>b.Id<a.Id).Select(b=>b.Goal).ToArray());
                a.Route.Clear();a.Index=1;a.SegmentUs=0;a.PendingSince=TimeUs;a.State="PathPending";
            }
        }
        public void Advance(long elapsedUs)
        {
            if(elapsedUs<0)throw new ArgumentOutOfRangeException(nameof(elapsedUs));
            if(Paused||Hold.Length>0||Complete)return;
            _inputUs=checked(_inputUs+elapsedUs);
            while(_inputUs>=50000 && Hold.Length==0&&!Complete)
            { _inputUs-=50000; Tick(); }
        }
        private void Tick()
        {
            _motion=new List<MotionSpan>();
            if(_regroupRequested){_regroupRequested=false;StartRegroup();}
            if(TimeUs%100000==0) SearchRound();
            if(Hold.Length>0)return;
            long remaining=50000;
            while(remaining>0)
            {
                long step=remaining;
                foreach(var a in Actors)if(a.Route.Count>a.Index)step=Math.Min(step,a.Reversing?a.SegmentUs:a.Duration-a.SegmentUs);
                MoveInterval(step);remaining-=step;TimeUs+=step;
            }
            if(Actors.All(a=>a.Arrived))
            {
                if(_regrouping){if(_resumeAfterRegroup)ResumeOriginalGoals();}
                else {Phase++;if(Phase==1||Phase==2||Phase==8||Phase==9)foreach(var a in Actors)a.SafeAnchor=a.Anchor;Request();}
            }
        }
        private void SearchRound()
        {
            LastExpansions=0;
            int idle=0;
            while(LastExpansions<Budget && idle<Actors.Length)
            {
                var a=Actors[_cursor];_cursor=(_cursor+1)%Actors.Length;
                if(a.Search?.Status!=TrialSearchStatus.Pending){idle++;continue;}
                int used=a.Search.Expand(Grid,1);LastExpansions+=used;idle=0;
            }
            foreach(var a in Actors)
            {
                if(a.Search==null)continue;
                if(a.Search.RequestKey!=a.RequestKey)throw new InvalidOperationException("Stale route request");
                if(a.Search.Status==TrialSearchStatus.Found && a.Route.Count==0)
                {a.Route=new List<TrialPoint>(a.Search.Path);a.Index=1;a.SegmentUs=0;a.State="Moving";}
                else if(a.Search.Status==TrialSearchStatus.Found && a.ReplanTail && a.Index<a.Route.Count)
                {
                    a.Route=a.Route.Take(a.Index+1).Concat(a.Search.Path.Skip(1)).ToList();a.ReplanTail=false;
                }
                else if(a.Search.Status==TrialSearchStatus.Pending && TimeUs-a.PendingSince>=2000000)
                {Hold="NavigationBudgetExceeded";return;}
                else if(a.Search.Status==TrialSearchStatus.NoPath) {a.State="NoPath";Recover(a,100000);}
            }
        }
        private void MoveInterval(long us)
        {
            var starts=Actors.Select(a=>a.At()).ToArray();
            var ends=(TrialRational[])starts.Clone();
            var bounds=new long[3,3];
            for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)bounds[i,j]=bounds[j,i]=DistanceBound(Actors[i],Actors[j]);
            // Existing reservation always precedes new actors. Stable roles then EntityId.
            var order=Actors.OrderBy(a=>Reservations.Contains(a.Id)?Reservations.IndexOf(a.Id):10+a.Id).ToArray();
            foreach(var a in order)
            {
                if(a.Route.Count<=a.Index)continue;
                if(LeashedActor>=0&&a.Id!=LeashedActor){a.State="LeashWitness";continue;}
                if(a.Id==DelayedActor&&TimeUs<DelayUntilUs){a.State="ControlledDelay";continue;}
                if(Obstructed&&!a.Reversing&&!_regrouping){a.State="Blocked";Recover(a,us);continue;}
                var from=starts[a.Id];var to=a.At(us);
                if(a.Id==LeashedActor&&!a.LeashReturning)
                {
                    long cost=TrialPoint.Length(a.Route[a.Index-1],to.Rounded)+4;
                    for(int k=1;k<a.Index;k++)cost+=TrialPoint.Length(a.Route[k-1],a.Route[k]);
                    var p=to.Rounded;
                    if(cost>6000000||p.X<_leashArea.X0+450004||p.X>_leashArea.X1-450004||p.Z<_leashArea.Z0+450004||p.Z>_leashArea.Z1-450004)
                    {a.LeashReturning=true;a.Reversing=true;a.State="LeashReturning";continue;}
                }
                bool entering=Branch==TrialBranch.North && to.X>=22500000*to.D && to.X<=31500000*to.D && to.Z>=20000000*to.D && to.Z<=21600000*to.D;
                if(entering&&!Reservations.Contains(a.Id))
                {
                    var previous=Reservations.Count>0?Actors[Reservations[Reservations.Count-1]].At():from;
                    if(Reservations.Count>0 && previous.X*from.D-from.X*previous.D<900000*previous.D*from.D)
                    {a.State="ReservationWait";continue;}
                    Reservations.Add(a.Id);ReservationDirection=1;PeakReservations=Math.Max(PeakReservations,Reservations.Count);
                }
                bool safe=true;
                foreach(var b in Actors)if(a.Id!=b.Id && !TrialRational.Swept(from,to,starts[b.Id],ends[b.Id])){safe=false;break;}
                if(!safe){a.State="SeparationWait";CollisionWaits++;Recover(a,us);continue;}
                long travel=TrialPoint.Length(from.Rounded,to.Rounded)+4;
                foreach(var b in Actors)if(a.Id!=b.Id)
                {
                    // A valid path witness certifies navigable distance, not a Euclidean shortcut.
                    long initial=bounds[a.Id,b.Id];
                    long other=TrialPoint.Length(starts[b.Id].Rounded,ends[b.Id].Rounded)+4;
                    if(initial>8000000-travel-other){safe=false;break;}
                }
                if(!safe){a.State="CohesionWait";CohesionWaits++;continue;}
                ends[a.Id]=to;bool reverse=a.Reversing;a.Move(us);
                if(reverse&&a.Route.Count==0&&!a.LeashReturning)Replan(a,a.Anchor);
            }
            // Tokens remain ordered until the LAST member leaves, including inter-stage checkpoints.
            if(Reservations.Count>0 && Actors.All(a=>a.At().X>=31500000*a.At().D)) {Reservations.Clear();ReservationDirection=0;}
            for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)
            {
                long motion=TrialPoint.Length(starts[i].Rounded,Actors[i].At().Rounded)+TrialPoint.Length(starts[j].Rounded,Actors[j].At().Rounded)+8;
                long carried=bounds[i,j]<=long.MaxValue-motion?bounds[i,j]+motion:long.MaxValue;
                // Re-use a valid Lipschitz certificate until it approaches the ceiling.
                // Geometry/path recertification is synchronous and creates no hidden search.
                _pairBounds[i,j]=_pairBounds[j,i]=carried<7000000?carried:Math.Min(carried,FreshDistanceBound(Actors[i],Actors[j]));
            }
            _motion.Add(new MotionSpan{Start=TimeUs,Duration=us,From=starts,To=Actors.Select(a=>a.At()).ToArray()});
            if(_regrouping&&!Actors.Any(a=>a.Search?.Status==TrialSearchStatus.Pending))
            {_regroupUs+=us;if(_regroupUs>=8000000)Hold="NavigationBlocked";}
        }
        public TrialRational PresentationPosition(int actor)
        {
            long at=TimeUs-50000+Math.Min(_inputUs,50000);
            foreach(var span in _motion)if(at>=span.Start&&at<=span.Start+span.Duration)
                return TrialRational.Blend(span.From[actor],span.To[actor],at-span.Start,span.Duration);
            return Actors[actor].At();
        }
        public long DistanceBound(TrialActor a,TrialActor b)
            => _pairBounds[a.Id,b.Id]<=8000000?_pairBounds[a.Id,b.Id]:FreshDistanceBound(a,b);
        private long FreshDistanceBound(TrialActor a,TrialActor b)
        {
            var p=a.At().Rounded;var q=b.At().Rounded;
            long direct=Grid.DistanceCertificate(p,q,Branch);
            if(direct<long.MaxValue)return direct+4;
            var wa=a.Witnesses();var wb=b.Witnesses();long best=long.MaxValue;
            foreach(var kv in wa)if(wb.TryGetValue(kv.Key,out long d))best=Math.Min(best,kv.Value+d);
            return best;
        }
        public void EnableLeash(int actor,TrialRect area)
        {if(TimeUs!=0||actor<0||actor>=Actors.Length)throw new InvalidOperationException("Configure a valid pilot leash before motion");LeashedActor=actor;_leashArea=area;}
        private void Recover(TrialActor a,long us)
        {
            a.BlockedUs+=us;
            if(a.BlockedUs>=1000000 && a.Recovery==0)
            {
                a.Recovery=1;a.State="RecoveryReplan";
                if(a.Index<a.Route.Count){Replan(a,a.Route[a.Index]);a.ReplanTail=true;}
            }
            if(a.BlockedUs>=2000000 && a.Recovery==1)
            {
                a.Recovery=2;a.State="RecoveryAlternative";
                if(a.Index<a.Route.Count&&a.SegmentUs>0&&!Reservations.Contains(a.Id))a.Reversing=true;
            }
            if(a.BlockedUs>=5000000 && a.Recovery==2)
            {a.Recovery=3;a.State="RecoveryRegroup";if(Reservations.Count==0)_regroupRequested=true;}
            if(a.BlockedUs>=8000000 || (_resumed&&a.BlockedUs>=1000000))Hold="NavigationBlocked";
        }
        private void StartRegroup()
        {
            _regrouping=true;_regroupUs=5000000;_originalGoals=Actors.Select(a=>a.Goal).ToArray();
            foreach(var a in Actors)
            {
                a.Goal=a.SafeAnchor;a.ReplanTail=false;a.Search=null;
                if(a.Index<a.Route.Count&&a.SegmentUs>0)a.Reversing=true;
                else {a.Route.Clear();a.Index=1;a.SegmentUs=0;Replan(a,a.Anchor);}
            }
        }
        private void ResumeOriginalGoals()
        {
            _regrouping=false;_resumeAfterRegroup=false;
            foreach(var a in Actors){a.Goal=_originalGoals[a.Id];a.Route.Clear();a.Index=1;a.SegmentUs=0;a.BlockedUs=0;a.Recovery=0;Replan(a,a.Anchor);}
        }
        private void Replan(TrialActor a,TrialPoint from)
        {
            a.Search=new TrialSearch(Grid,from,a.Goal,Branch,++a.RequestKey,Actors.Where(b=>b.Id<a.Id).Select(b=>b.Goal).ToArray());a.PendingSince=TimeUs;
        }
        public bool Resume()
        {
            if(Hold.Length==0)return false;
            Hold="";_resumed=true;
            foreach(var a in Actors){a.BlockedUs=0;a.PendingSince=TimeUs;}
            if(_regrouping){if(Actors.All(a=>a.Arrived))ResumeOriginalGoals();else _resumeAfterRegroup=true;}
            return true;
        }
        public TrialSimulation Checkpoint() {
            var c=(TrialSimulation)MemberwiseClone();c.Actors=Actors.Select(a=>a.Clone()).ToArray();c.Reservations=new List<int>(Reservations);c._stops=(TrialPoint[])_stops.Clone();c._pairBounds=(long[,])_pairBounds.Clone();c._originalGoals=(TrialPoint[])_originalGoals.Clone();return c;
        }
        public static TrialSimulation Restore(TrialSimulation checkpoint,TrialGrid grid,long generation=1) {
            if(checkpoint.Grid.Map.Hash!=grid.Map.Hash || checkpoint.MapGeneration!=generation)throw new InvalidOperationException("Map generation/geometry mismatch");
            if(!ReferenceEquals(checkpoint.Grid,grid))throw new InvalidOperationException("Pilot restore requires the same immutable graph instance");
            return checkpoint.Checkpoint();
        }
        public string Fingerprint => $"{Grid.Map.Hash}:{MapGeneration}:{TimeUs}:{_inputUs}:{Phase}:{Branch}:{Hold}:{Paused}:{_cursor}:{ReservationDirection}:{string.Join(",",Reservations)}:{Budget}:{Obstructed}:{_resumed}:{LeashedActor}:{_leashArea.Id}:{DelayedActor}:{DelayUntilUs}:"+
            $"{_regroupRequested}:{_regrouping}:{_resumeAfterRegroup}:{_regroupUs}:"+string.Join(";",_originalGoals)+":"+string.Join(",",_pairBounds.Cast<long>())+":"+string.Join("|",Actors.Select(a=>$"{a.Id}:{a.At()}:{a.Heading}:{a.Goal}:{a.SafeAnchor}:{a.Index}:{a.SegmentUs}:{a.PendingSince}:{a.RequestKey}:{a.State}:{a.BlockedUs}:{a.Recovery}:{a.Reversing}:{a.ReplanTail}:{a.LeashReturning}:"+
                string.Join(";",a.Route)+":"+a.Search?.Fingerprint));
    }
}
