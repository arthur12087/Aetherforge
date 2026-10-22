using Aetherforge.Domain;
using Aetherforge.Persistence;

ExpeditionChecks.Run();
Console.WriteLine("PASS: expedição local, marcha, cinco etapas, papéis, cura, escudo, pausa, recuo, derrota, determinismo e limites.");

void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
var team = StarterParty.Create();
Require(team.Count == 3 && team.Select(x => x.Role).Distinct().Count() == 3, "Equipe precisa das três funções.");
var preview = new CombatPreview();
preview.Step();
Require(preview.Party[0].Health < 180 && preview.Party[1].Health == 100 && preview.Party[2].Health == 110, "Tank precisa receber o ataque comum.");
preview.Party[0].Health = 0;
preview.Step();
Require(preview.Party[1].Health < 100, "Quando o tank cai, outro integrante recebe dano.");
for (var i = 0; i < 1000; i++) preview.Step();
Require(preview.Party.All(p => p.Health >= 0 && p.Health <= p.MaxHealth), "Vida fora dos limites.");
Require(preview.Encounters > 0, "Loop não progrediu.");
var healerPreview = new CombatPreview();
for (var i = 0; i < 7; i++) healerPreview.Step();
var beforeHeal = healerPreview.Party[0].Health;
healerPreview.Step();
Require(healerPreview.LastEvent == "Infusão Etérea" && healerPreview.Party[0].Health > beforeHeal, "Healer não curou quando necessário.");
var diagonal = MovementRules.Validate(1, 1);
Require(Math.Abs(diagonal.X * diagonal.X + diagonal.Y * diagonal.Y - 1) < .001, "Diagonal aumenta velocidade.");
Require(MovementRules.Validate(float.NaN, 1) == (0f, 0f), "Entrada inválida aceita.");
Require(MovementRules.Validate(float.MaxValue, float.MaxValue) == (0f, 0f), "Overflow precisa falhar fechado.");
Console.WriteLine("PASS: composição, alvo do tank, recuperação, cura, limites de HP, velocidade diagonal e entradas inválidas.");

var file = Path.Combine(Path.GetTempPath(), "aetherforge-check-" + Guid.NewGuid().ToString("N") + ".json");
try
{
    string itemId;
    var transfer = new ItemCommand { OperationId = Guid.NewGuid().ToString("N") };
    var equip = new ItemCommand { OperationId = Guid.NewGuid().ToString("N") };
    using (var store = new IntegrationStore(file))
    {
        var now = DateTimeOffset.UtcNow;
        store.Tick(now); Require(store.Snapshot().Battle.Tick == 0, "Worker sem presença não pode produzir.");
        for (var i=0; i<12; i++) { store.Heartbeat(now.AddSeconds(i)); store.Tick(now.AddSeconds(i)); }
        var item = store.Snapshot().Items.Single(); itemId = item.Id;
        Require(item.Scope == EquipmentScope.Idle && item.Origin.StartsWith("Idle"), "Drop não veio do idle.");
        transfer.ItemId = itemId; equip.ItemId = itemId;
        try { store.Execute("equip", equip); throw new Exception("Equipou sem transferir."); } catch (InvalidOperationException) { }
        Parallel.For(0,20, _ => store.Execute("transfer", transfer));
        Require(store.Snapshot().Items.Single().Id == itemId && store.Snapshot().Items.Single().Scope == EquipmentScope.Shared, "Transferência duplicou ou mudou o ID.");
        store.Execute("hit",new ItemCommand { OperationId = Guid.NewGuid().ToString("N") });
        Require(store.Snapshot().Character.LastDamage == 20, "Dano base incorreto.");
        Parallel.For(0,20, _ => store.Execute("equip",equip));
        var snapshot=store.Snapshot();
        Require(snapshot.Character.Defense == 13 && snapshot.Character.EquippedItemId == itemId && snapshot.Items.Single().Bound, "Armadura não foi aplicada.");
        store.Execute("hit",new ItemCommand { OperationId = Guid.NewGuid().ToString("N") });
        Require(store.Snapshot().Character.LastDamage == 17, "O equipamento não mitigou dano real.");
        try { store.Execute("transfer", new ItemCommand { OperationId=Guid.NewGuid().ToString("N"),ItemId=itemId }); throw new Exception("Transferiu duas vezes."); } catch (InvalidOperationException) { }
        try { store.Execute("equip", new ItemCommand { OperationId=transfer.OperationId,ItemId=itemId }); throw new Exception("Reutilizou operação para outra ação."); } catch (InvalidOperationException) { }
        for(var i=12;i<50;i++) { store.Heartbeat(now.AddSeconds(i)); store.Tick(now.AddSeconds(i)); }
        Require(store.Snapshot().Items.Length==1,"Drop introdutório repetido.");
        try { using var second = new IntegrationStore(file); throw new Exception("Dois processos possuem o arquivo."); } catch (IOException) { }
    }
    using(var reopened = new IntegrationStore(file))
    {
        Require(reopened.Snapshot().Character.EquippedItemId==itemId && reopened.Snapshot().Character.LastDamage==17,"Estado não sobreviveu ao reinício.");
        reopened.Execute("transfer",transfer); reopened.Execute("equip",equip);
        Require(reopened.Snapshot().Items.Length==1,"Retry após reinício duplicou item.");
        reopened.Execute("unequip",new ItemCommand { OperationId=Guid.NewGuid().ToString("N"),ItemId=itemId });
        Require(reopened.Snapshot().Character.Defense==10 && reopened.Snapshot().Items.Single().Scope==EquipmentScope.Shared,"Desequipar não removeu bônus.");
    }
    Console.WriteLine("PASS: drop autoritativo, mesmo ID, concorrência/idempotência, defesa, dano 20→17, reinício e desequipar.");
}
finally { foreach(var suffix in new[]{"",".lock",".tmp"}) if(File.Exists(file+suffix)) File.Delete(file+suffix); }
