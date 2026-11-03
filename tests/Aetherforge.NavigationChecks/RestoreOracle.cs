using Aetherforge.Domain.NavigationTrial;
using System.Collections;
using System.Reflection;

// Literal floor complement and relative swept-disc oracle from review70.
// Does not call the product geometry or distance certificate predicates.
internal sealed class RestoreOracle{
 readonly List<(double x0,double z0,double x1,double z1)> solids=new();
 public int Spans;public double MinWall=double.MaxValue,MinDisc=double.MaxValue;
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
 public void Observe(TrialSimulation s, Action<bool,string> check){foreach(var span in (IEnumerable)typeof(TrialSimulation).GetField("_motion",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(s)!){
  Spans++;var a=(TrialRational[])span.GetType().GetField("From")!.GetValue(span)!;var b=(TrialRational[])span.GetType().GetField("To")!.GetValue(span)!;
  for(int i=0;i<3;i++){foreach(var r in solids)MinWall=Math.Min(MinWall,SB(a[i],b[i],r));for(int j=0;j<i;j++)MinDisc=Math.Min(MinDisc,PS(0,0,a[i].MetresX-a[j].MetresX,a[i].MetresZ-a[j].MetresZ,b[i].MetresX-b[j].MetresX,b[i].MetresZ-b[j].MetresZ));}
  check(MinWall>=.449998&&MinDisc>=.799998,$"F4 independent continuous clearance={MinWall} separation={MinDisc}");
 }}
}
