using Aetherforge.Domain;
using System.Text.Json;

namespace Aetherforge.Persistence;

/// <summary>Single local test account. Atomic JSON storage; not a production authentication/database service.</summary>
public sealed class IntegrationStore : IDisposable
{
    private readonly object _gate = new();
    private readonly string _file;
    private readonly FileStream _lease;
    private DurableState _state;
    private readonly CombatPreview _battle = new();
    private DateTimeOffset _lastHeartbeat = DateTimeOffset.MinValue;
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public IntegrationStore(string file)
    {
        _file = Path.GetFullPath(file);
        Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
        _lease = new FileStream(_file + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        try
        {
            _state = File.Exists(_file) ? JsonSerializer.Deserialize<DurableState>(File.ReadAllText(_file), Json) ?? throw new InvalidDataException("Estado inválido.") : new DurableState();
            if (_state.Items.Select(i => i.Id).Distinct().Count() != _state.Items.Count || _state.Items.Count(i => i.Scope == EquipmentScope.Equipped) > 1)
                throw new InvalidDataException("Estado contém equipamento duplicado.");
        }
        catch { _lease.Dispose(); throw; }
    }

    public void Heartbeat(DateTimeOffset now) { lock (_gate) _lastHeartbeat = now; }

    public void Tick(DateTimeOffset now)
    {
        lock (_gate)
        {
            if (now < _lastHeartbeat || now - _lastHeartbeat > TimeSpan.FromSeconds(3)) return;
            var before = _battle.EnemyHealth;
            _battle.Step();
            if (before > 0 && _battle.EnemyHealth <= 0 && !_state.IntroDropGranted)
            {
                var next = Clone(_state);
                next.IntroDropGranted = true;
                next.Items.Add(new EquipmentInstance { Id = Guid.NewGuid().ToString("N"), Scope = EquipmentScope.Idle, DroppedAt = now });
                Commit(next);
            }
        }
    }

    public IntegrationSnapshot Snapshot()
    {
        lock (_gate)
        {
            var state = Clone(_state);
            var equipped = state.Items.SingleOrDefault(i => i.Scope == EquipmentScope.Equipped);
            return new IntegrationSnapshot
            {
                Battle = _battle.Snapshot(), Items = state.Items.ToArray(),
                Character = new CharacterSnapshot { Defense = 10 + (equipped?.DefenseBonus ?? 0), Health = state.Health,
                    EquippedItemId = equipped?.Id, EquipmentName = equipped?.Name ?? "Sem armadura", LastDamage = state.LastDamage, HitCount = state.HitCount }
            };
        }
    }

    public CommandReceipt Execute(string action, ItemCommand command)
    {
        lock (_gate)
        {
            if (!Guid.TryParse(command.OperationId, out _)) throw new InvalidOperationException("Identificador de operação inválido.");
            if (_state.Commands.TryGetValue(command.OperationId, out var previous))
            {
                if (previous.Action != action || previous.ItemId != command.ItemId) throw new InvalidOperationException("Operação já usada para outro pedido.");
                return Clone(previous);
            }
            var next = Clone(_state);
            if (action == "hit")
            {
                if (command.ItemId != "") throw new InvalidOperationException("O canhão não recebe equipamentos como entrada.");
                var defense = 10 + (next.Items.SingleOrDefault(i => i.Scope == EquipmentScope.Equipped)?.DefenseBonus ?? 0);
                next.LastDamage = EquipmentRules.IncomingDamage(30, defense); next.HitCount++;
                if (next.Health <= 0) next.Health = 180;
                next.Health = Math.Max(0, next.Health - next.LastDamage);
            }
            else
            {
                var item = next.Items.SingleOrDefault(i => i.Id == command.ItemId) ?? throw new InvalidOperationException("Equipamento não encontrado nesta conta.");
                if (action == "transfer") EquipmentRules.Transfer(item);
                else if (action == "equip")
                {
                    if (item.Scope != EquipmentScope.Shared) throw new InvalidOperationException("O item precisa estar na mochila do MMO.");
                    foreach (var old in next.Items.Where(i => i.Scope == EquipmentScope.Equipped)) old.Scope = EquipmentScope.Shared;
                    EquipmentRules.Equip(item);
                }
                else if (action == "unequip")
                {
                    if (item.Scope != EquipmentScope.Equipped) throw new InvalidOperationException("O item não está equipado.");
                    item.Scope = EquipmentScope.Shared;
                }
                else throw new InvalidOperationException("Ação desconhecida.");
            }
            var receipt = new CommandReceipt { OperationId = command.OperationId, ItemId = command.ItemId, Action = action,
                Message = action == "transfer" ? "Equipamento enviado ao MMO." : action == "equip" ? "Armadura equipada. Defesa +3." : action == "unequip" ? "Armadura voltou para a mochila." : $"Canhão: {next.LastDamage} de dano recebido." };
            next.Commands.Add(command.OperationId, receipt);
            Commit(next);
            return Clone(receipt);
        }
    }

    private static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, Json), Json)!;
    private void Commit(DurableState state)
    {
        var temporary = _file + ".tmp";
        using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            JsonSerializer.Serialize(stream, state, Json); stream.Flush(true);
        }
        File.Move(temporary, _file, true);
        _state = state;
    }
    public void Dispose() => _lease.Dispose();

    public sealed class DurableState
    {
        public bool IntroDropGranted { get; set; }
        public List<EquipmentInstance> Items { get; set; } = new();
        public Dictionary<string, CommandReceipt> Commands { get; set; } = new();
        public int Health { get; set; } = 180;
        public int LastDamage { get; set; }
        public int HitCount { get; set; }
    }
}
