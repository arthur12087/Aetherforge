using Aetherforge.Domain.NavigationTrial;
using System.Diagnostics;
using System.Text.Json;

int assertions=0;
try
{
void Check(bool value,string message){assertions++;if(!value)throw new Exception(message);}
var watch=Stopwatch.StartNew();var map=new TrialMap();var grid=new TrialGrid(map);
double construction=watch.Elapsed.TotalMilliseconds;
var corrective=CorrectiveChecks.Run(grid,Check);
Check(map.Clear(TrialPoint.M(8,14)),"Union portal must remain open");
Check(!map.Segment(TrialPoint.M(21,14),TrialPoint.M(33,14)),"Central machine blocks a segment with valid endpoints");
Check(!map.Segment(TrialPoint.M(21,24),TrialPoint.M(33,24)),"Exterior cannot be crossed and re-entered");
Check(map.Clear(TrialPoint.M(25,20.45m)),"Clearance equality is valid");
Check(!map.Clear(TrialPoint.M(25,20.449999m)),"One micrometre inside wall margin must fail");
Check(map.Clear(TrialPoint.M(25,20.65m),650000),"Boss-radius clearance uses union geometry");
Check(!TrialMap.Separated(TrialPoint.M(0,0),TrialPoint.M(2,0),TrialPoint.M(2,0),TrialPoint.M(0,0)),"Swapping endpoints collides between frames");
Check(TrialMap.Separated(TrialPoint.M(0,0),TrialPoint.M(2,0),TrialPoint.M(0,.8m),TrialPoint.M(2,.8m)),"Exact minimum separation accepted");
var results=new List<object>();var frontierMs=new List<double>();var cloneMs=new List<double>();
var leashQuery=new TrialLeash(grid,TrialPoint.M(14.05m,19.05m),TrialPoint.M(20.05m,19.05m),map.Floor[1],101);
while(leashQuery.Search.Status==TrialSearchStatus.Pending)leashQuery.Search.Expand(grid,256);
Check(leashQuery.Within==true&&leashQuery.Search.Cost==6000000,"Leash exactly 6 navigable metres accepted");
var tooFar=new TrialLeash(grid,TrialPoint.M(14.05m,19.05m),TrialPoint.M(20.15m,19.05m),map.Floor[1],102);
while(tooFar.Search.Status==TrialSearchStatus.Pending)tooFar.Search.Expand(grid,256);
Check(tooFar.Within==false,"Leash beyond 6 metres rejected");
Check(!leashQuery.Certifies(leashQuery.Search.Destination,TrialPoint.M(20.15m,19.05m)),"Whole next movement must fit leash");
var leashed=new TrialSimulation(grid,TrialBranch.North);leashed.EnableLeash(0,map.Floor[0]);
for(int i=0;i<200&&leashed.Actors[0].State!="LeashReturned";i++)leashed.Advance(50000);
Check(leashed.Actors[0].State=="LeashReturned"&&leashed.Actors[0].Anchor.Equals(TrialPoint.M(4,14)),"Leash actor returns through actual route to spawn");
var delayed=new TrialSimulation(grid,TrialBranch.North){DelayedActor=2,DelayUntilUs=6000000};
for(int i=0;i<240;i++)delayed.Advance(50000);
Check(delayed.CohesionWaits>0&&delayed.Hold=="","Leader waits on real navigable cohesion and resumes after follower");
foreach(var branch in new[]{TrialBranch.North,TrialBranch.South})
{
    var sim=new TrialSimulation(grid,branch){Budget=8192};var split=sim.Checkpoint();
    sim.Advance(50000);split.Advance(17000);split.Advance(33000);
    Check(sim.Fingerprint==split.Fingerprint,"Advance partition invariance at first boundary");
    bool pending=false, checkpointed=false, sawCorridor=false;
    int ticks=0;long worstExpansions=0;double minimumSeparation=double.MaxValue;int lastPhase=-1;
    while(!sim.Complete && sim.Hold.Length==0 && ticks++<6000)
    {
        if(sim.Actors.Any(a=>a.Search?.Status==TrialSearchStatus.Pending))pending=true;
        var before=sim.Actors.Select(a=>a.At()).ToArray();
        watch.Restart();sim.Advance(50000);frontierMs.Add(watch.Elapsed.TotalMilliseconds);
        worstExpansions=Math.Max(worstExpansions,sim.LastExpansions);
        Check(sim.LastExpansions<=8192,"Shared search budget");
        if(sim.Phase!=lastPhase){Console.WriteLine($"{branch} phase={sim.Phase} t={sim.TimeUs/1e6:0.00} states={string.Join(',',sim.Actors.Select(a=>a.State))}");lastPhase=sim.Phase;}
        for(int i=0;i<3;i++)
        {
            var a=sim.Actors[i];var p=a.At().Rounded;
            Check(map.Inside(p),"Actor remains in actual floor union");
            // Independent rectangular oracle for the central machine, including swept substeps tested above.
            Check(!(p.X>23000000&&p.X<31000000&&p.Z>8000000&&p.Z<20000000),"No actor inside machine");
            Check(TrialPoint.Length(before[i].Rounded,p)<=140010,"No teleport: max speed × fixed tick");
            for(int j=i+1;j<3;j++)
            {
                var q=sim.Actors[j].At().Rounded;
                double d=Math.Sqrt((double)(p.X-q.X)*(p.X-q.X)+(double)(p.Z-q.Z)*(p.Z-q.Z));minimumSeparation=Math.Min(minimumSeparation,d);
                Check(d>=799998,"Independent minimum separation oracle (rounding <=2um)");
                Check(sim.DistanceBound(a,sim.Actors[j])<=8000000,"Navigable cohesion certificate");
            }
        }
        if(sim.Reservations.Count>0)sawCorridor=true;
        if(!checkpointed && (branch==TrialBranch.North ? sim.Reservations.Count>=2 : sim.Phase>=5))
        {
            watch.Restart();var snap=sim.Checkpoint();var replay=TrialSimulation.Restore(snap,grid);cloneMs.Add(watch.Elapsed.TotalMilliseconds);
            string frozen=snap.Fingerprint;sim.Paused=true;string paused=sim.Fingerprint;sim.Advance(9000000);
            Check(sim.Fingerprint==paused,"Pause freezes time/routes/reservations/Pending");sim.Paused=false;
            for(int k=0;k<20;k++){sim.Advance(50000);replay.Advance(11000);replay.Advance(39000);Check(sim.Fingerprint==replay.Fingerprint,"Checkpoint suffix matches different call partitions");}
            Check(snap.Fingerprint==frozen,"Checkpoint detached from live state");checkpointed=true;
        }
    }
    Check(sim.Complete,$"{branch} must reach R5, hold={sim.Hold} phase={sim.Phase} states={string.Join(',',sim.Actors.Select(a=>a.State))}");
    Check(checkpointed&&pending,"Checkpoint and Pending exercised");
    Check(branch!=TrialBranch.North || sawCorridor&&sim.PeakReservations>=2,"Sequential admission permits multiple corridor members");
    Check(sim.Reservations.Count==0,"Reservations released after final exit");
    results.Add(new{branch=branch.ToString(),sim.TimeUs,ticks,minimumSeparation,sim.PeakReservations,sim.CohesionWaits,sim.CollisionWaits,worstExpansions});
}
// An unfinished frontier and zero budget must remain Pending, never become NoPath.
var pendingSim=new TrialSimulation(grid,TrialBranch.North){Budget=1};pendingSim.Advance(50000);
var pendingCopy=TrialSimulation.Restore(pendingSim.Checkpoint(),grid);
Check(pendingSim.Actors.Any(a=>a.Search?.Status==TrialSearchStatus.Pending),"Pending search retained");
for(int i=0;i<8;i++){pendingSim.Advance(50000);pendingCopy.Advance(50000);Check(pendingSim.Fingerprint==pendingCopy.Fingerprint,"Heap/parents/cursor restored exactly");}
pendingSim.Advance(3000000);Check(pendingSim.Hold=="NavigationBudgetExceeded","Budget timeout is technical Hold");
try{TrialSimulation.Restore(pendingCopy,grid,2);throw new Exception("Accepted wrong generation");}catch(InvalidOperationException) {assertions++;}
var blocked=new TrialSimulation(grid,TrialBranch.North);blocked.Advance(500000);blocked.Obstructed=true;blocked.Advance(9000000);
Check(blocked.Hold=="NavigationBlocked","Obstruction ends in Hold, not defeat/teleport");
var held=blocked.Actors.Select(a=>a.At().ToString()).ToArray();blocked.Advance(5000000);
Check(held.SequenceEqual(blocked.Actors.Select(a=>a.At().ToString())),"Hold freezes actors");
blocked.Obstructed=false;Check(blocked.Resume(),"Explicit resume accepted");blocked.Advance(1500000);
Check(blocked.Hold==""&&!held.SequenceEqual(blocked.Actors.Select(a=>a.At().ToString())),"Recovered obstruction moves by route");
var limited=new TrialSearch(grid,TrialPoint.M(21,14),TrialPoint.M(27,14),TrialBranch.North,90);
Check(limited.Status==TrialSearchStatus.NoPath,"Destination in solid is not rounded across wall");
double Percentile(List<double> data,double p)=>data.OrderBy(x=>x).ElementAt(Math.Min(data.Count-1,(int)(data.Count*p)));
object Stats(List<double> data)=>new{count=data.Count,p50=Percentile(data,.5),p95=Percentile(data,.95),p99=Percentile(data,.99),max=data.Max()};
var queryMs=new List<double>();var sweptMs=new List<double>();var searchMs=new List<double>();var pendingCloneMs=new List<double>();
for(int i=0;i<1000;i++)
{
    watch.Restart();map.Segment(TrialPoint.M(21,20.8m),TrialPoint.M(33,20.8m));queryMs.Add(watch.Elapsed.TotalMilliseconds);
    watch.Restart();TrialRational.Swept(new TrialRational(TrialPoint.M(0,0)),new TrialRational(TrialPoint.M(2,0)),new TrialRational(TrialPoint.M(2,.79m)),new TrialRational(TrialPoint.M(0,.79m)));sweptMs.Add(watch.Elapsed.TotalMilliseconds);
}
var benchSearch=new TrialSearch(grid,TrialPoint.M(4,14),TrialPoint.M(11,18),TrialBranch.North,501);
while(benchSearch.Status==TrialSearchStatus.Pending){watch.Restart();benchSearch.Expand(grid,512);searchMs.Add(watch.Elapsed.TotalMilliseconds);}
var clonePending=new TrialSimulation(grid,TrialBranch.North){Budget=1};clonePending.Advance(50000);
for(int i=0;i<30;i++){watch.Restart();var copy=TrialSimulation.Restore(clonePending.Checkpoint(),grid);pendingCloneMs.Add(watch.Elapsed.TotalMilliseconds);Check(copy.Fingerprint==clonePending.Fingerprint,"Measured pending clones retain whole search");}
// Fresh grid: compare a calibrated cold run, rather than reusing the baseline's edge cache.
var calibratedGrid=new TrialGrid(new TrialMap());
var sliced=new TrialSimulation(calibratedGrid,TrialBranch.North);var slicedTimes=new List<double>();int slicedTicks=0;
while(!sliced.Complete&&sliced.Hold.Length==0&&slicedTicks++<3000){watch.Restart();sliced.Advance(50000);slicedTimes.Add(watch.Elapsed.TotalMilliseconds);}
Check(sliced.Complete,"512-expansion alternative finishes without Pending timeout");
var continuous=ContinuousChecks.Run(Check);
var report=new{passed=true,assertions,corrective,continuous,constructionMs=construction,geometryHash=map.Hash,results,
    frontierMs=new{p50=Percentile(frontierMs,.5),p95=Percentile(frontierMs,.95),p99=Percentile(frontierMs,.99),max=frontierMs.Max()},
    cloneRestoreMs=cloneMs,geometryQueriesMs=Stats(queryMs),sweptDiscMs=Stats(sweptMs),warmSearchRounds512Ms=Stats(searchMs),pendingCloneRestoreMs=Stats(pendingCloneMs),
    budget512=new{cache="fresh grid (cold first traversal)",complete=sliced.Complete,sliced.TimeUs,frontierMs=Stats(slicedTimes)},
    queries=map.SegmentQueries,predicates=map.PredicateTests,cacheHits=grid.EdgeCacheHits,
    managedMemory=GC.GetTotalMemory(false),workingSet=Process.GetCurrentProcess().WorkingSet64,environment=System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription};
string json=JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true});
if(args.Length>0){Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0]))!);File.WriteAllText(args[0],json);}
Console.WriteLine(json);Console.WriteLine("NAVIGATION TRIAL CHECK PASS");
return 0;
}
catch(Exception ex)
{
    string failure=JsonSerializer.Serialize(new{passed=false,assertions,error=ex.ToString()},new JsonSerializerOptions{WriteIndented=true});
    if(args.Length>0){Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0]))!);File.WriteAllText(args[0],failure);}
    Console.Error.WriteLine(failure);return 1;
}
