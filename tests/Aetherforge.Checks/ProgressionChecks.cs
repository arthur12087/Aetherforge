using System;
using System.Linq;
using Aetherforge.Domain;

public static class ProgressionChecks
{
    public static void Run()
    {
        var progress=new CharacterProgression();
        Require(progress.GainExperience(59)==0 && progress.Experience==59,"XP below threshold");
        Require(progress.GainExperience(1)==1 && progress.Level==2 && progress.Experience==0 && progress.RequiredExperience==90,"Exact first threshold");
        Require(progress.GainExperience(240)==2 && progress.Level==4 && progress.Experience==30,"Multi-level overflow");
        int total=progress.TotalExperience;
        bool rejected=false; try { progress.GainExperience(-1); } catch(ArgumentOutOfRangeException) { rejected=true; }
        Require(rejected && progress.TotalExperience==total,"Invalid XP must be atomic");
        progress.GainExperience(int.MaxValue);
        Require(progress.Level==20 && progress.Experience==0 && progress.RequiredExperience==0 && progress.TotalExperience==6270,"Cap and overflow safety");
        progress.GainExperience(50); Require(progress.TotalExperience==6270,"No banked XP at cap");

        var party=StarterParty.Create(); party[0].Health=100;
        party[0].GainExperience(60);
        Require(party[0].MaxHealth==198 && party[0].Health==118 && party[0].Attack==8 && party[0].Defense==10,"Linear level-two stats and missing HP preserved");
        party[0].GainExperience(90);
        Require(party[0].MaxHealth==216 && party[0].Attack==8 && party[0].Defense==11,"Growth must derive from original base, not rounded previous level");
        party[2].Health=0; party[2].GainExperience(60);
        Require(party[2].Progression.Level==2 && party[2].Health==0,"XP must not revive fallen hero");

        var run=new ExpeditionSimulation(); run.Advance(1000);
        Require(run.Phase==ExpeditionPhase.Complete && run.Rewards.Receipts.Count==5,"Exactly one receipt per victory");
        Require(run.Rewards.Coins==79 && run.Rewards.Scrap==11 && run.Rewards.Components==1,"GDD stage rewards");
        Require(run.Party.All(p=>p.Progression.Level==2 && p.Progression.Experience==62),"122 XP per hero on first route");
        Require(run.Rewards.Receipts.Last().Drop!=null,"Boss guarantees equipment");
        string earned=Ledger(run); run.Advance(1000); run.Retreat(); Require(Ledger(run)==earned,"Terminal state cannot duplicate rewards");
        var state=run.Rewards.RandomState; int items=run.Rewards.Inventory.Count;
        run.RestartRoute();
        Require(run.Rewards.RandomState==state && run.Rewards.Coins==79 && run.Rewards.Inventory.Count==items && run.Party.All(p=>p.Progression.Level==2 && p.Progression.Experience==62 && p.Health==p.MaxHealth),"Retry preserves earned state and RNG");
        run.Advance(1000);
        Require(run.Rewards.Receipts.Count==10 && run.Rewards.Coins==158 && run.Party.All(p=>p.Progression.TotalExperience==244),"New attempt can earn each encounter once");
        var dead=new ExpeditionSimulation(); dead.Party[2].Health=0; dead.Advance(12);
        // Trigger a victory through the normal scheduler with a strong living attacker.
        dead.Party[1].GainExperience(6270); dead.Advance(20);
        Require(dead.Rewards.Receipts.Count>0 && dead.Party[2].Progression.TotalExperience>0 && dead.Party[2].Health==0,"Fallen member receives victory XP");
        long coins=dead.Rewards.Coins; int xp=dead.Party[2].Progression.TotalExperience;
        foreach(var member in dead.Party) member.Health=0; dead.Advance(.1);
        Require(dead.Phase==ExpeditionPhase.Defeat && dead.Rewards.Coins==coins && dead.Party[2].Progression.TotalExperience==xp,"Defeat keeps prior gains without granting new rewards");

        var large=new ExpeditionSimulation(12); var sliced=new ExpeditionSimulation(12);
        large.Advance(1000); for(int i=0;i<1000;i++)sliced.Advance(1);
        Require(Ledger(large)==Ledger(sliced),"Seeded loot and XP independent of delta partition");
        foreach(var rarity in Enum.GetValues<LootRarity>())
        {
            bool observed=false;
            for(uint seed=0;seed<300;seed++)
            {
                var sample=new ExpeditionSimulation(seed); sample.Advance(1000);
                observed|=sample.Rewards.Inventory.Any(item=>item.Rarity==rarity);
                if(observed)break;
            }
            Require(observed,"Each configured rarity reachable");
        }
        var full=new ExpeditionSimulation();
        for(int i=0;i<24;i++) { full.Advance(1000); full.RestartRoute(); }
        Require(full.Rewards.Inventory.Count==21 && full.Rewards.PendingDrops.Count>=3,"Full bag retains excess loot as pending");
        var drops=full.Rewards.Receipts.Count(r=>r.Drop!=null);
        Require(full.Rewards.Inventory.Count+full.Rewards.PendingDrops.Count==drops && full.Rewards.Inventory.Concat(full.Rewards.PendingDrops).Select(item=>item.Id).Distinct().Count()==drops,"No lost or duplicate item IDs");
        full.Reset(); Require(full.Rewards.Coins==0 && full.Rewards.Receipts.Count==0 && full.Rewards.Inventory.Count==0 && full.Party.All(p=>p.Progression.TotalExperience==0),"Explicit fresh session clears progress");
        Console.WriteLine("PASS: XP, limite 20, atributos lineares, ganhos por vitória, caídos, retry, loot determinístico e bolsa cheia sem perda.");
    }
    private static string Ledger(ExpeditionSimulation sim) => string.Join("|",sim.Rewards.Coins,sim.Rewards.Scrap,sim.Rewards.Components,sim.Rewards.RandomState,
        string.Join(";",sim.Party.Select(p=>p.Progression.TotalExperience)),string.Join(";",sim.Rewards.Inventory.Select(i=>$"{i.Id}:{i.Role}:{i.Slot}:{i.Rarity}")),sim.Rewards.Receipts.Count);
    private static void Require(bool condition,string message) { if(!condition)throw new InvalidOperationException("Progression: "+message); }
}
