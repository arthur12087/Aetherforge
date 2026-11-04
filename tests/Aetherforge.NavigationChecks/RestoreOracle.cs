using Aetherforge.Domain.NavigationTrial;
using System.Collections;
using System.Reflection;

// Literal floor complement and relative swept-disc oracle from review70.
// Does not call the product geometry or distance certificate predicates.
internal sealed class RestoreOracle{
 readonly List<(double x0,double z0,double x1,double z1)> solids=new();
 public int Spans;public double MinWall=double.MaxValue,MinDisc=double.MaxValue;
 public double MaxCohesionWitness;
 public RestoreOracle(){
  (double,double,double,double)[] floor={(0,10,8,18),(8,2,23,26),(23,20,31,21.6),(23,4,31,8),(31,2,40,26),(40,8,48,20),(48,2,62,26)};
  double[] xs={-1,0,8,23,31,40,48,62,63},zs={-1,2,4,8,10,18,20,21.6,26,29};
  for(int i=1;i<xs.Length;i++)for(int j=1;j<zs.Length;j++){double x=(xs[i-1]+xs[i])/2,z=(zs[j-1]+zs[j])/2;if(!floor.Any(r=>x>r.Item1&&x<r.Item3&&z>r.Item2&&z<r.Item4))solids.Add((xs[i-1],zs[j-1],xs[i],zs[j]));}
 }
 static double PS(double x,double z,double a,double b,double c,double d){double dx=c-a,dz=d-b,t=dx*dx+dz*dz==0?0:Math.Clamp(((x-a)*dx+(z-b)*dz)/(dx*dx+dz*dz),0,1);return Math.Sqrt(Math.Pow(x-a-t*dx,2)+Math.Pow(z-b-t*dz,2));}
 static double PB(double x,double z,(double x0,double z0,double x1,double z1) r)=>Math.Sqrt(Math.Pow(Math.Max(0,Math.Max(r.x0-x,x-r.x1)),2)+Math.Pow(Math.Max(0,Math.Max(r.z0-z,z-r.z1)),2));
 static double SB(TrialRational p,TrialRational q,(double x0,double z0,double x1,double z1) r){double a=p.MetresX,b=p.MetresZ,c=q.MetresX,d=q.MetresZ,lo=0,hi=1;bool Clip(double s,double e,double min,double max){if(s==e)return s>=min&&s<=max;double t=(min-s)/(e-s),u=(max-s)/(e-s);lo=Math.Max(lo,Math.Min(t,u));hi=Math.Min(hi,Math.Max(t,u));return lo<=hi;}
  if(Clip(a,c,r.x0,r.x1)&&Clip(b,d,r.z0,r.z1))return 0;
  return new[]{PB(a,b,r),PB(c,d,r),PS(r.x0,r.z0,a,b,c,d),PS(r.x0,r.z1,a,b,c,d),PS(r.x1,r.z0,a,b,c,d),PS(r.x1,r.z1,a,b,c,d)}.Min();
 }
 // An explicit free-space polyline is an upper bound on navigable distance.
 // Test the whole path against the literal floor complement, never Euclidean
 // distance alone and never the product's cohesion cache/certificate predicate.
 double Witness(TrialRational p,TrialRational q){
  double Leg(TrialRational a,TrialRational b)=>solids.All(r=>SB(a,b,r)>=.449998)?Math.Sqrt(Math.Pow(a.MetresX-b.MetresX,2)+Math.Pow(a.MetresZ-b.MetresZ,2)):double.PositiveInfinity;
  var xz=new TrialRational(p.X*q.D,q.Z*p.D,p.D*q.D);var zx=new TrialRational(q.X*p.D,p.Z*q.D,p.D*q.D);
  return Math.Min(Leg(p,q),Math.Min(Leg(p,xz)+Leg(xz,q),Leg(p,zx)+Leg(zx,q)));
 }
 public void Observe(TrialSimulation s, Action<bool,string> check){foreach(var span in (IEnumerable)typeof(TrialSimulation).GetField("_motion",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(s)!){
  Spans++;var a=(TrialRational[])span.GetType().GetField("From")!.GetValue(span)!;var b=(TrialRational[])span.GetType().GetField("To")!.GetValue(span)!;
  for(int i=0;i<3;i++){foreach(var r in solids)MinWall=Math.Min(MinWall,SB(a[i],b[i],r));for(int j=0;j<i;j++)MinDisc=Math.Min(MinDisc,PS(0,0,a[i].MetresX-a[j].MetresX,a[i].MetresZ-a[j].MetresZ,b[i].MetresX-b[j].MetresX,b[i].MetresZ-b[j].MetresZ));}
  for(int i=0;i<3;i++)for(int j=0;j<i;j++){
   double before=Witness(a[i],a[j]),after=Witness(b[i],b[j]);
   // Lipschitz extension of a real path proves the entire motion span, not just
   // the endpoints. Displacement equals distance travelled in these linear spans.
   double travel=Math.Sqrt(Math.Pow(a[i].MetresX-b[i].MetresX,2)+Math.Pow(a[i].MetresZ-b[i].MetresZ,2))+Math.Sqrt(Math.Pow(a[j].MetresX-b[j].MetresX,2)+Math.Pow(a[j].MetresZ-b[j].MetresZ,2));
   double bound=Math.Min(before,after)+travel;MaxCohesionWitness=Math.Max(MaxCohesionWitness,bound);
   check(bound<=8.000002,$"F4 independent navigable cohesion witness={bound} at {s.TimeUs}");
  }
  check(MinWall>=.449998&&MinDisc>=.799998,$"F4 independent continuous clearance={MinWall} separation={MinDisc}");
 }}
}
