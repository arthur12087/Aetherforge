using System.Reflection;
using System.Text.Json;
using Aetherforge.Domain;

/// <summary>Contract E01–E15/F01–F10 at the domain boundary. UI selection/focus remain client checks.</summary>
public static class EquipmentChecks
{
    private static int _assertions;
    public static void Run()
    {
        StatsAndDamage();
        AtomicMovesAndCapacity();
        HealthAndLevel();
        IntentAndReceipts();
        ManagementAndTime();
        TerminalsAndAttempts();
        CheckpointAndInvariants();
        Console.WriteLine($"PASS: equipamento V19, {_assertions} asserções de atributos, transações, identidade, FIFO, HP, scheduler e checkpoint volátil.");
    }

    private static string Id() => Guid.NewGuid().ToString("N");
    private static RouteCommand Route(ExpeditionSimulation s,RouteAction action,bool paused=false)
        => new(Id(),s.SessionId,s.AttemptId,action,paused);
    private static CommandResult Do(ExpeditionSimulation s,RouteAction action,bool paused=false) => s.Execute(Route(s,action,paused));
    private static EquipmentPreview Preview(ExpeditionSimulation s,int member,LootSlot slot,string? item=null)
        => s.Compare(new(s.SessionId,s.AttemptId,ExpeditionSimulation.GetMemberId((PartyRole)member),slot,item==null ? EquipmentAction.Unequip : EquipmentAction.Equip,item));
    private static CommandResult Equip(ExpeditionSimulation s,int member,ExpeditionDrop item)
        => s.Execute(Preview(s,member,item.Slot,item.Id).Confirm(Id()));
    private static CommandResult Unequip(ExpeditionSimulation s,int member,LootSlot slot)
        => s.Execute(Preview(s,member,slot).Confirm(Id()));
    private static void Manage(ExpeditionSimulation s) => Require(Do(s,RouteAction.RequestManagement).Outcome==CommandOutcome.Applied,"Management starts.");
    private static ExpeditionDrop Add(ExpeditionSimulation s,PartyRole role,LootSlot slot,LootRarity rarity=LootRarity.Common)
    {
        // Fixture-only ownership injection, never a public grant/import command or a manual save edit.
        long sequence=s.Rewards.LastIssuedItemSequence+1;
        var item=new ExpeditionDrop("local-"+sequence.ToString("D6"),role,slot,rarity);
        typeof(ExpeditionRewards).GetField("_nextItem",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(s.Rewards,sequence);
        s.Rewards.Items.Add(item); return item;
    }
    private static string State(ExpeditionSimulation s) => JsonSerializer.Serialize(s.CaptureCheckpoint());
    private static void Unchanged(ExpeditionSimulation s,Action action,string message)
    { string before=State(s); action(); Require(State(s)==before,message); }
    private static void Require(bool condition,string message)
    { _assertions++; if (!condition) throw new InvalidOperationException("Equipment: "+message); }
    private static void Near(double a,double b,string message) => Require(Math.Abs(a-b)<.00001,message+$" ({a:R} vs {b:R})");

    private static void StatsAndDamage()
    {
        var s=new ExpeditionSimulation(); var armor=Add(s,PartyRole.Tank,LootSlot.Armor);
        Manage(s); var preview=Preview(s,0,LootSlot.Armor,armor.Id);
        Require(preview.Before!.Defense==10 && preview.After!.Defense==12 && preview.DefenseDelta==2,"E01 defense preview.");
        Require(Equip(s,0,armor).Outcome==CommandOutcome.Applied && s.Party[0].Defense==12,"E01 common armor applies.");
        Do(s,RouteAction.FinishManagement); s.Advance(16/2.4); s.Advance(1.01);
        Near(s.Events.First(e=>e.Kind=="hurt" && e.Actor==3).Amount,5,"E01 armor mitigates raw 17 before shield.");
        var shielded=new ExpeditionSimulation(); var a=Add(shielded,PartyRole.Tank,LootSlot.Armor);
        Manage(shielded); Equip(shielded,0,a); Do(shielded,RouteAction.FinishManagement);
        shielded.Advance(16/2.4); shielded.ActivateShield(); shielded.Advance(1.01);
        Near(shielded.Events.First(e=>e.Kind=="hurt" && e.Actor==3).Amount,5*.35,"Defense precedes shield.");

        var d=new ExpeditionSimulation();
        var common=Add(d,PartyRole.Dps,LootSlot.Weapon); var equal=Add(d,PartyRole.Dps,LootSlot.Weapon);
        var uncommon=Add(d,PartyRole.Dps,LootSlot.Weapon,LootRarity.Uncommon); var rare=Add(d,PartyRole.Dps,LootSlot.Weapon,LootRarity.Rare);
        Manage(d); var first=Preview(d,1,LootSlot.Weapon,common.Id);
        Near(first.DpsBefore,14.4,"Base DPS."); Near(first.DpsAfter,16,"Common DPS."); Equip(d,1,common);
        var unchanged=Preview(d,1,LootSlot.Weapon,equal.Id);
        Require(unchanged.AttackDelta==0 && d.Execute(unchanged.Confirm(Id())).Outcome==CommandOutcome.Applied,"E02 equal stats swaps distinct IDs.");
        Require(d.Rewards.Inventory[0].Id==common.Id && d.Party[1].Loadout.Weapon!.Id==equal.Id,"Old item returns to candidate index.");
        var better=Preview(d,1,LootSlot.Weapon,uncommon.Id);
        Require(better.AttackDelta==1 && better.After!.Attack==21,"E03 upgrade adds only one.");
        Equip(d,1,uncommon); Equip(d,1,rare);
        var worse=Preview(d,1,LootSlot.Weapon,common.Id);
        Require(worse.AttackDelta==-2 && Equip(d,1,common).Outcome==CommandOutcome.Applied && d.Party[1].Attack==20,"E04 downgrade allowed.");
        Unequip(d,1,LootSlot.Weapon); Require(d.Party[1].Attack==18,"All attack bonus removed.");
        Unchanged(d,()=>Require(Unequip(d,1,LootSlot.Weapon).Outcome==CommandOutcome.NoChange,"Empty slot no-op."),"No-op preserves full snapshot.");

        var h=new ExpeditionSimulation(); var focus=Add(h,PartyRole.Healer,LootSlot.Weapon);
        Manage(h); Equip(h,2,focus);
        Require(h.Party[2].Attack==8 && h.CurrentInfusionAmount==44,"E06 level-one focus attack/infusion.");
        h.Party[2].GainExperience(60); Require(h.CurrentInfusionAmount==47,"E06 level-two focus added after rounding.");
        Unequip(h,2,LootSlot.Weapon); Require(h.CurrentInfusionAmount==45,"Focus removal clears infusion bonus.");
        Equip(h,2,focus); Do(h,RouteAction.FinishManagement); h.Advance(16/2.4);
        h.Party[0].Health=100; h.Advance(.01);
        Near(h.Events.Last(e=>e.Kind=="heal").Amount,47,"Combat uses same infusion calculation.");
        var cap=new ExpeditionSimulation(); var capFocus=Add(cap,PartyRole.Healer,LootSlot.Weapon);
        Manage(cap); Equip(cap,2,capFocus); Do(cap,RouteAction.FinishManagement); cap.Advance(16/2.4);
        cap.Party[1].Health=72; cap.Advance(.01);
        Require(cap.Party[1].Health==100 && cap.Events.Last(e=>e.Kind=="heal").Amount==28,"E06 real infusion capped to missing HP.");
        var fractional=new PartyMember(PartyRole.Tank,"legacy","",180.5f,8.5f,10.5f);
        Require(fractional.MaxHealth==180.5f && fractional.Attack==8.5f,"Legacy level-one fractional bases preserved.");
    }

    private static void AtomicMovesAndCapacity()
    {
        var s=new ExpeditionSimulation(); var old=Add(s,PartyRole.Tank,LootSlot.Armor); Manage(s); Equip(s,0,old);
        var candidate=Add(s,PartyRole.Tank,LootSlot.Armor,LootRarity.Rare);
        while(s.Rewards.Inventory.Count<21) Add(s,PartyRole.Dps,LootSlot.Accessory);
        var pending=Add(s,PartyRole.Healer,LootSlot.Weapon); Add(s,PartyRole.Dps,LootSlot.Weapon);
        var ids=s.CaptureCheckpoint().Items.Select(i=>i.Id).ToArray(); long revision=s.InventoryRevision;
        Require(Equip(s,0,candidate).Outcome==CommandOutcome.Applied,"E10 full bag substitution accepted.");
        Require(s.Rewards.Inventory.Count==21 && s.Rewards.Inventory[0].Id==old.Id && s.Rewards.PendingDrops[0].Id==pending.Id && s.InventoryRevision==revision+1,"E10 return has priority, equal count changes revision.");
        Unchanged(s,()=>Require(Unequip(s,0,LootSlot.Armor).Code==ExpeditionCode.BagFull,"E12 full bag rejected."),"BagFull preserves everything.");
        Require(s.CaptureCheckpoint().Items.Select(i=>i.Id).SequenceEqual(ids),"All IDs conserved.");
        Unchanged(s,()=>Require(Equip(s,2,pending).Code==ExpeditionCode.ItemNotAtExpectedLocation,"Pending is not a source."),"Pending rejection preserves FIFO.");

        var empty=new ExpeditionSimulation(); var arm=Add(empty,PartyRole.Tank,LootSlot.Armor);
        while(empty.Rewards.Inventory.Count<21) Add(empty,PartyRole.Dps,LootSlot.Accessory);
        var p1=Add(empty,PartyRole.Healer,LootSlot.Weapon); var p2=Add(empty,PartyRole.Healer,LootSlot.Armor);
        Manage(empty); var result=Equip(empty,0,arm);
        Require(result.PendingRecovered==1 && empty.Rewards.Inventory.Count==21 && empty.Rewards.Inventory.Last().Id==p1.Id && empty.Rewards.PendingDrops.Single().Id==p2.Id,"E11 only first pending fills real vacancy.");

        foreach (int count in new[]{19,20})
        {
            var f=new ExpeditionSimulation(); var equipped=Add(f,PartyRole.Tank,LootSlot.Weapon); Manage(f); Equip(f,0,equipped);
            while(f.Rewards.Inventory.Count<21) Add(f,PartyRole.Dps,LootSlot.Accessory);
            var p=Add(f,PartyRole.Healer,LootSlot.Weapon); Add(f,PartyRole.Healer,LootSlot.Armor);
            // Seed a bag with 19/20 pieces and pending items without running recovery first.
            for(int i=count;i<21;i++)
            {
                var piece=f.Rewards.Items.Bag.Last(); f.Rewards.Items.Bag.RemoveAt(f.Rewards.Items.Bag.Count-1);
                f.Rewards.Items.Pending.Add(piece);
            }
            var r=Unequip(f,0,LootSlot.Weapon);
            Require(r.PendingRecovered==20-count && f.Rewards.Inventory[count].Id==equipped.Id && f.Rewards.Inventory.Count==21,"E12 returned item precedes recovery.");
            if(count==19) Require(f.Rewards.Inventory.Last().Id==p.Id,"E12 FIFO after old item.");
        }
        var view=s.ReadView(); var saved=view.Inventory[0].Id;
        s.RestartRoute(); Require(view.Inventory[0].Id==saved && view.AttemptId!=s.AttemptId,"Read view is detached.");
        Require(view.Inventory is not List<ExpeditionDrop> && view.Members is not ExpeditionMemberView[],"No mutable collection escapes read API.");
    }

    private static void HealthAndLevel()
    {
        var s=new ExpeditionSimulation(); var common=Add(s,PartyRole.Tank,LootSlot.Accessory); var rare=Add(s,PartyRole.Tank,LootSlot.Accessory,LootRarity.Rare);
        Manage(s); s.Party[0].Health=100; Equip(s,0,common);
        Require(s.Party[0].Health==100 && s.Party[0].MaxHealth==190,"E07 maximum increase never heals.");
        Unequip(s,0,LootSlot.Accessory); Require(s.Party[0].Health==100,"Removing accessory preserves absolute HP.");
        s.Party[0].Health=180; Equip(s,0,common); Require(s.Party[0].Health==180,"E07 full before does not mean full after.");
        Equip(s,0,rare); s.Party[0].Health=202;
        var preview=Preview(s,0,LootSlot.Accessory,common.Id);
        Require(preview.HealthBefore==202 && preview.HealthAfter==190,"E08 preview shows clamp.");
        Equip(s,0,common); Equip(s,0,rare);
        Require(s.Party[0].Health==190 && s.Party[0].MaxHealth==202,"E08 toggling cannot heal.");
        for(int i=0;i<4;i++) { Equip(s,0,common); Equip(s,0,rare); }
        Require(s.Party[0].Health==190,"Repeated swapping cannot restore lost HP.");
        Unequip(s,0,LootSlot.Accessory); Require(s.Party[0].Health==180,"Clamp on unequip.");
        s.Party[0].Health=100.125f; Equip(s,0,rare); Unequip(s,0,LootSlot.Accessory);
        Require(s.Party[0].Health==100.125f,"Fractional HP preserved exactly.");
        s.Party[0].Health=0; Equip(s,0,common); Require(s.Party[0].Health==0,"E09 equipment never revives.");

        var level=new ExpeditionSimulation(); var accessory=Add(level,PartyRole.Tank,LootSlot.Accessory);
        level.Party[0].GainExperience(48); level.Advance(16/2.4);
        // Synthetic nearly won combat isolates the credit/level/management boundary.
        level.Party[0].Health=100; foreach(var enemy in level.Enemies) enemy.Health=0;
        Do(level,RouteAction.RequestManagement); level.Advance(1000);
        Require(level.Party[0].Progression.Level==2 && level.Party[0].Health==118 && level.Party[0].MaxHealth==198,"E15 level before management preserves missing HP.");
        Equip(level,0,accessory); Require(level.Party[0].Health==118 && level.Party[0].MaxHealth==208,"E15 gear does not repeat level healing.");
        var fallen=new ExpeditionSimulation(); var worn=Add(fallen,PartyRole.Tank,LootSlot.Accessory);
        Manage(fallen); Equip(fallen,0,worn); fallen.Party[0].Health=0; fallen.Party[0].GainExperience(60);
        Require(fallen.Party[0].Health==0 && fallen.Party[0].MaxHealth==208,"Fallen level-up with gear still does not revive.");
    }

    private static void IntentAndReceipts()
    {
        var s=new ExpeditionSimulation(); var item=Add(s,PartyRole.Dps,LootSlot.Weapon); var other=Add(s,PartyRole.Healer,LootSlot.Armor);
        Manage(s);
        Unchanged(s,()=>Require(Equip(s,0,item).Code==ExpeditionCode.RoleMismatch,"E05 role mismatch."),"Wrong role atomic.");
        Unchanged(s,()=>Require(s.Execute(Preview(s,1,LootSlot.Armor,item.Id).Confirm(Id())).Code==ExpeditionCode.SlotMismatch,"Wrong slot."),"Wrong slot atomic.");
        var stale=Preview(s,1,LootSlot.Weapon,item.Id).Confirm(Id()); s.Party[1].Health-=1;
        Unchanged(s,()=>Require(s.Execute(stale).Code==ExpeditionCode.StaleComparison,"HP change invalidates intent."),"Stale HP leaves state intact.");
        var accepted=Preview(s,1,LootSlot.Weapon,item.Id).Confirm(Id()); var applied=s.Execute(accepted);
        Require(applied.Outcome==CommandOutcome.Applied,"Apply command.");
        Unchanged(s,()=>Require(s.Execute(accepted).Outcome==CommandOutcome.Replay,"E14 replay before stale revision."),"Replay changes nothing.");
        var conflict=new EquipmentCommand(accepted.OperationId,new(s.SessionId,s.AttemptId,"healer",LootSlot.Armor,EquipmentAction.Equip,other.Id),s.StateRevision,null);
        Unchanged(s,()=>Require(s.Execute(conflict).Code==ExpeditionCode.OperationConflict,"OperationId payload collision."),"Conflict atomic.");
        Unchanged(s,()=>Require(Equip(s,1,item).Outcome==CommandOutcome.NoChange,"Already equipped no-op."),"No-op makes no receipt.");
        var slotPreview=Preview(s,1,LootSlot.Weapon);
        Unequip(s,1,LootSlot.Weapon);
        Unchanged(s,()=>Require(s.Execute(slotPreview.Confirm(Id())).Code==ExpeditionCode.StaleComparison,"Slot change invalidates preview."),"Stale slot atomic.");
        s.RestartRoute();
        var replay=s.Execute(accepted);
        Require(replay.Outcome==CommandOutcome.Replay && replay.LatestReadModel.AttemptId==s.AttemptId && replay.Original!.AppliedRevision==applied.AppliedRevision,"Replay after retry carries current view and original receipt.");
        Unchanged(s,()=>Require(s.Execute(stale).Code==ExpeditionCode.AttemptMismatch,"Unapplied old attempt rejected."),"Old attempt atomic.");
        var fromOtherSession=new EquipmentCommand(Id(),new(Id(),s.AttemptId,"dps",LootSlot.Weapon,EquipmentAction.Equip,item.Id),s.StateRevision,null);
        Unchanged(s,()=>Require(s.Execute(fromOtherSession).Code==ExpeditionCode.SessionMismatch,"Session scoped identity."),"Foreign session atomic.");
        var invalid=new EquipmentCommand("bad",new(s.SessionId,s.AttemptId,"dps",LootSlot.Weapon,EquipmentAction.Equip,item.Id),s.StateRevision,null);
        Unchanged(s,()=>Require(s.Execute(invalid).Code==ExpeditionCode.InvalidOperationId,"Operation ID format."),"Malformed operation atomic.");
        var badEnum=new EquipmentCommand(Id(),new(s.SessionId,s.AttemptId,"dps",(LootSlot)99,EquipmentAction.Equip,item.Id),s.StateRevision,null);
        Unchanged(s,()=>Require(s.Execute(badEnum).Code==ExpeditionCode.InvalidCommand,"Unknown slot rejected."),"Enum atomic.");
        bool rejected=false; try { _=new ExpeditionDrop("local-999999",PartyRole.Tank,LootSlot.Weapon,(LootRarity)99); } catch(ArgumentException) { rejected=true; }
        Require(rejected,"Unknown rarity never falls through to rare bonus.");
        // E13 cancellation is a read-only domain interaction; selection epoch belongs to the client.
        Unchanged(s,()=>{ _=Preview(s,1,LootSlot.Weapon,item.Id); },"Comparing/canceling does not change management or inventory.");
    }

    private static void ManagementAndTime()
    {
        var s=new ExpeditionSimulation(); var item=Add(s,PartyRole.Tank,LootSlot.Weapon);
        s.Advance(1.5); var preview=Preview(s,0,LootSlot.Weapon,item.Id);
        Require(preview.BlockReason==ExpeditionCode.ManagementRequired,"F01 menu does not grant management.");
        double distance=s.Distance; Manage(s);
        Unchanged(s,()=>s.Advance(1000),"March management freezes exact position/timers."); Near(s.Distance,distance,"No march drift.");
        Unchanged(s,()=>Require(s.Execute(preview.Confirm(Id())).Code==ExpeditionCode.StaleComparison,"Entry needs a new preview."),"Old callback cannot auto-confirm.");
        Equip(s,0,item); Do(s,RouteAction.FinishManagement); s.Advance((s.EncounterAt-s.Distance)/2.4);
        Require(s.Phase==ExpeditionPhase.Combat,"F06 management release does not skip march.");
        Do(s,RouteAction.SetPaused,true); var request=Route(s,RouteAction.RequestManagement); s.Execute(request);
        Require(s.Management==ExpeditionManagement.Requested && s.Paused,"F02 paused combat only requests.");
        Unchanged(s,()=>Require(Unequip(s,0,LootSlot.Weapon).Code==ExpeditionCode.InCombat,"Paused combat still blocks."),"Combat rejection atomic.");
        Unchanged(s,()=>s.Advance(1000),"Paused combat does not fulfill request.");
        Do(s,RouteAction.CancelManagementRequest); Require(s.Paused && s.Management==ExpeditionManagement.None,"Cancel never resumes pause.");
        Do(s,RouteAction.RequestManagement); Do(s,RouteAction.SetPaused,false);
        s.ActivateShield(); var result=s.AdvanceDetailed(1000);
        Require(result.StopReason==AdvanceStopReason.Management && result.Discarded>900 && s.Phase==ExpeditionPhase.Loot && s.Rewards.Receipts.Count==1,"F03 huge delta intercepted on first credit.");
        Near(s.PhaseRemaining,2,"F03 two full seconds of rest remain.");
        Unchanged(s,()=>s.Advance(1000),"F07 management freezes HP/RNG/recargas/XP/events.");
        Unchanged(s,()=>Require(s.Execute(request).Outcome==CommandOutcome.Replay,"Fulfilled request retry does not schedule another."),"Request replay unchanged.");
        foreach(double invalid in new[]{-1d,double.NaN,double.PositiveInfinity,double.NegativeInfinity})
        {
            Unchanged(s,()=>{ bool rejected=false; try{s.Advance(invalid);}catch(ArgumentOutOfRangeException){rejected=true;} Require(rejected,"Invalid delta always rejects."); },"Invalid frozen delta atomic.");
        }
        Do(s,RouteAction.FinishManagement); s.Advance(1.25); Manage(s);
        Near(s.PhaseRemaining,.75,"F05 partial rest captured.");
        var timers=s.CaptureCheckpoint(); Unequip(s,0,LootSlot.Weapon);
        Require(s.CaptureCheckpoint().HeroNext.SequenceEqual(timers.HeroNext) && s.CaptureCheckpoint().EnemyNext.SequenceEqual(timers.EnemyNext) && s.HealCooldown==timers.View.HealCooldown,"Gear leaves deadlines untouched.");
        Unchanged(s,()=>s.Advance(200),"Human wait adds no time.");
        Do(s,RouteAction.FinishManagement); s.Advance(.74); Require(s.Phase==ExpeditionPhase.Loot,"F05 no early treatment.");
        s.Advance(.01); Require(s.Phase==ExpeditionPhase.March && s.StageIndex==1,"F05 rest finishes exactly once.");
        var rest=new ExpeditionSimulation(); var healthGear=Add(rest,PartyRole.Dps,LootSlot.Accessory);
        rest.Advance(16/2.4); foreach(var enemy in rest.Enemies) enemy.Health=0;
        Do(rest,RouteAction.RequestManagement); rest.Advance(1000); rest.Party[1].Health=30;
        Equip(rest,1,healthGear); Do(rest,RouteAction.FinishManagement); rest.Advance(2);
        Require(rest.Party[1].Health==52,"Rest uses currently equipped max, once, with living healer.");
        var noHealer=new ExpeditionSimulation(); noHealer.Advance(16/2.4); noHealer.Party[2].Health=0;
        foreach(var enemy in noHealer.Enemies) enemy.Health=0;
        Do(noHealer,RouteAction.RequestManagement); noHealer.Advance(1000); noHealer.Party[1].Health=30;
        Do(noHealer,RouteAction.FinishManagement); noHealer.Advance(2);
        Require(noHealer.Party[1].Health==30 && noHealer.Party[2].Health==0,"Rest with fallen healer never treats or revives.");

        var canceled=new ExpeditionSimulation(); canceled.Advance(16/2.4);
        Do(canceled,RouteAction.RequestManagement); Do(canceled,RouteAction.CancelManagementRequest); canceled.Advance(1000);
        Require(canceled.Phase==ExpeditionPhase.Complete && canceled.Rewards.Receipts.Count==5,"F04 canceled request allows route to finish.");
        var paused=new ExpeditionSimulation(); Do(paused,RouteAction.SetPaused,true); Manage(paused);
        Require(Do(paused,RouteAction.SetPaused,false).Code==ExpeditionCode.ManagementActive,"Pause control rejects during active management.");
        Do(paused,RouteAction.FinishManagement); Require(paused.Paused,"F10 preexisting pause survives management.");
        var sliced=new ExpeditionSimulation(12); var large=new ExpeditionSimulation(12);
        large.Advance(1000); for(int i=0;i<1000;i++) sliced.Advance(.2);
        Require(large.Rewards.RandomState==sliced.Rewards.RandomState && large.Rewards.Coins==sliced.Rewards.Coins && large.Rewards.Receipts.Count==sliced.Rewards.Receipts.Count,"F06 delta partition preserves ledger/RNG.");
        Near(large.Elapsed,sliced.Elapsed,"F06 delta partition time.");
    }

    private static void TerminalsAndAttempts()
    {
        foreach(var phase in new[]{ExpeditionPhase.Complete,ExpeditionPhase.Defeat,ExpeditionPhase.Retreated})
        {
            var s=new ExpeditionSimulation(); var item=Add(s,PartyRole.Tank,LootSlot.Accessory);
            if(phase==ExpeditionPhase.Complete)
            {
                for(int i=0;i<4;i++) { s.Advance((s.EncounterAt-s.Distance)/2.4); Do(s,RouteAction.RequestManagement); s.Advance(1000); Do(s,RouteAction.FinishManagement); s.Advance(2); }
                s.Advance((s.EncounterAt-s.Distance)/2.4); Do(s,RouteAction.RequestManagement); s.Advance(1000);
            }
            else
            {
                s.Advance(16/2.4); Do(s,RouteAction.RequestManagement);
                if(phase==ExpeditionPhase.Defeat) { foreach(var p in s.Party) p.Health=0; s.Advance(.01); }
                else s.Retreat();
            }
            Require(s.Phase==phase && s.Management==ExpeditionManagement.None,"F08 terminal clears management.");
            float hp=s.Party[0].Health; long coins=s.Rewards.Coins; double elapsed=s.Elapsed;
            Require(Equip(s,0,item).Outcome==CommandOutcome.Applied && s.Party[0].Health==hp && s.Phase==phase && s.Elapsed==elapsed && s.Rewards.Coins==coins,"E09/F08 terminal gear neither resumes nor revives.");
            Unchanged(s,()=>s.Advance(1000),"Terminal ignores time after equipment.");
            string session=s.SessionId; long attempt=s.AttemptId; uint rng=s.Rewards.RandomState; long counter=s.Rewards.LastIssuedItemSequence;
            var restart=Route(s,RouteAction.RestartRoute); s.Execute(restart);
            Require(s.SessionId==session && s.AttemptId==attempt+1 && s.Party[0].Health==s.Party[0].MaxHealth && s.Party[0].Loadout.Accessory!.Id==item.Id && s.Rewards.RandomState==rng && s.Rewards.LastIssuedItemSequence==counter,"F09 explicit restart preserves gear/identity/RNG.");
            Unchanged(s,()=>Require(s.Execute(restart).Outcome==CommandOutcome.Replay,"Restart retry deduplicated across attempts."),"Replay cannot restart twice.");
            var reset=Route(s,RouteAction.ResetSession); s.Execute(reset);
            Require(s.SessionId!=session && s.AttemptId==1 && s.Party.All(p=>p.Loadout.Accessory==null) && s.Rewards.LastIssuedItemSequence==0 && s.Rewards.Coins==0,"F09 reset creates new empty session.");
            Unchanged(s,()=>Require(s.Execute(reset).Code==ExpeditionCode.SessionMismatch,"Reset retry cannot reset new session."),"Old reset atomic.");
        }
    }

    private sealed class Sink : IExpeditionCheckpointSink
    {
        public Func<ExpeditionSnapshot,bool> CommitAction = _=>true;
        public bool Commit(ExpeditionSnapshot candidate) => CommitAction(candidate);
    }
    private static void CheckpointAndInvariants()
    {
        var sink=new Sink(); var s=new ExpeditionSimulation(checkpointSink:sink);
        var item=Add(s,PartyRole.Tank,LootSlot.Armor); Manage(s);
        var command=Preview(s,0,LootSlot.Armor,item.Id).Confirm(Id());
        sink.CommitAction=candidate=>false;
        Unchanged(s,()=>Require(s.Execute(command).Code==ExpeditionCode.CheckpointFailed,"Injected commit refusal."),"Failed publication rolls back gear/HP/receipts/revisions.");
        sink.CommitAction=candidate=>throw new IOException("fixture");
        Unchanged(s,()=>Require(s.Execute(command).Code==ExpeditionCode.CheckpointFailed,"Thrown commit failure."),"Port exception cannot partially publish.");
        sink.CommitAction=candidate=>
        {
            bool rejected=false; try{ s.Advance(.1); }catch(InvalidOperationException){rejected=true;}
            Require(rejected,"Mutation callback reentry prohibited.");
            Require(candidate.View.Members[0].Stats.Defense==12 && candidate.View.Inventory.Count==0,"Sink sees coherent candidate.");
            return true;
        };
        Require(s.Execute(command).Outcome==CommandOutcome.Applied,"Apply after definite failed commit.");
        var checkpoint=s.CaptureCheckpoint(); var copy=ExpeditionSimulation.RestoreValidated(checkpoint);
        Require(State(copy)==State(s),"Exact memory checkpoint restores without granting/healing.");
        Unchanged(copy,()=>Require(copy.Execute(command).Outcome==CommandOutcome.Replay,"Receipt preserved in checkpoint."),"Restore replay unchanged.");
        var restoredView=copy.ReadView(); copy.RestartRoute(); Require(checkpoint.View.AttemptId==restoredView.AttemptId,"Checkpoint cannot change with live aggregate.");

        sink.CommitAction=_=>true; Do(s,RouteAction.FinishManagement); s.Advance(16/2.4); Do(s,RouteAction.RequestManagement);
        sink.CommitAction=candidate=>candidate.View.Phase!=ExpeditionPhase.Loot;
        var stopped=s.AdvanceDetailed(1000);
        Require(stopped.StopReason==AdvanceStopReason.CheckpointFailed && s.Rewards.Receipts.Count==0 && s.Phase==ExpeditionPhase.Combat && s.Victories==0,"Victory failure keeps last confirmed step, no partial credit.");
        sink.CommitAction=_=>true; s.Advance(1000);
        Require(s.Phase==ExpeditionPhase.Loot && s.Rewards.Receipts.Count==1,"Failed victory can commit once with unchanged RNG.");

        var overflow=new ExpeditionSimulation(); var gear=Add(overflow,PartyRole.Tank,LootSlot.Weapon); Manage(overflow);
        typeof(ExpeditionSimulation).GetProperty(nameof(ExpeditionSimulation.InventoryRevision))!.SetValue(overflow,long.MaxValue);
        Unchanged(overflow,()=>Require(Equip(overflow,0,gear).Code==ExpeditionCode.NumericOverflow,"Inventory revision overflow rejected."),"Overflow discards entire transaction.");
        var corrupt=new ExpeditionSimulation(); var owned=Add(corrupt,PartyRole.Tank,LootSlot.Weapon); Manage(corrupt);
        corrupt.Rewards.Items.Pending.Add(owned.Id);
        Unchanged(corrupt,()=>Require(Equip(corrupt,0,owned).Code==ExpeditionCode.InvalidState,"Duplicate owned ID fails closed."),"Invalid partition not repaired by deleting item.");
        var victoryOverflow=new ExpeditionSimulation(); victoryOverflow.Advance(16/2.4);
        foreach(var enemy in victoryOverflow.Enemies) enemy.Health=0;
        typeof(ExpeditionRewards).GetProperty(nameof(ExpeditionRewards.Coins))!.SetValue(victoryOverflow.Rewards,long.MaxValue);
        Unchanged(victoryOverflow,()=>
        {
            bool rejected=false; try{victoryOverflow.Advance(.01);}catch(OverflowException){rejected=true;}
            Require(rejected,"Victory wallet overflow refuses complete candidate.");
        },"Overflow cannot partially grant XP, RNG, receipt, time or victory.");
        var wideId=new ExpeditionSimulation();
        typeof(ExpeditionRewards).GetField("_nextItem",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(wideId.Rewards,999999L);
        var widePiece=Add(wideId,PartyRole.Tank,LootSlot.Weapon); Manage(wideId);
        Require(widePiece.Id=="local-1000000" && Equip(wideId,0,widePiece).Outcome==CommandOutcome.Applied,"Local identity D6 is minimum width, not a six-digit limit.");
        var many=new ExpeditionSimulation(); var pieces=Enumerable.Range(0,18).Select(i=>Add(many,(PartyRole)(i%3),(LootSlot)(i/6),(LootRarity)(i%3))).ToArray(); Manage(many);
        for(int n=0;n<80;n++)
        {
            var piece=pieces[n%pieces.Length]; Equip(many,(int)piece.Role,piece); if(n%3==0) Unequip(many,(int)piece.Role,piece.Slot);
            many.ValidateState(); Require(many.CaptureCheckpoint().Items.Count==pieces.Length,"Sequence conserves all IDs.");
        }
        var combat=new ExpeditionSimulation(); combat.Advance(16/2.4); combat.ActivateShield(); combat.Advance(3.21);
        var resumed=ExpeditionSimulation.RestoreValidated(combat.CaptureCheckpoint());
        combat.Advance(1000); resumed.Advance(1000);
        Require(State(combat)==State(resumed),"Mid-combat memory restore preserves cooldowns/swings/RNG/event history.");
    }
}
