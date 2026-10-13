using System;

namespace Aetherforge.Domain;

public enum EquipmentScope { Idle, Shared, Equipped }

public sealed class EquipmentInstance
{
    public string Id { get; set; } = "";
    public string DefinitionId { get; set; } = "brass-armor";
    public string Name { get; set; } = "Couraça de Latão";
    public string Rarity { get; set; } = "Incomum";
    public string Slot { get; set; } = "Armadura";
    public string Origin { get; set; } = "Idle / Sentinela das Galerias";
    public int DefenseBonus { get; set; } = 3;
    public EquipmentScope Scope { get; set; }
    public bool Bound { get; set; }
    public DateTimeOffset DroppedAt { get; set; }
}

public sealed class CombatSnapshot
{
    public int Tick { get; set; }
    public int Encounters { get; set; }
    public float EnemyHealth { get; set; } = CombatPreview.EnemyMaxHealth;
    public float[] Health { get; set; } = new[] { 180f, 100f, 110f };
    public string LastEvent { get; set; } = "Equipe em marcha";
}

public sealed class CharacterSnapshot
{
    public int BaseDefense { get; set; } = 10;
    public int Defense { get; set; } = 10;
    public int Health { get; set; } = 180;
    public int MaxHealth { get; set; } = 180;
    public string? EquippedItemId { get; set; }
    public string EquipmentName { get; set; } = "Sem armadura";
    public int LastDamage { get; set; }
    public int HitCount { get; set; }
}

public sealed class IntegrationSnapshot
{
    public CombatSnapshot Battle { get; set; } = new();
    public EquipmentInstance[] Items { get; set; } = Array.Empty<EquipmentInstance>();
    public CharacterSnapshot Character { get; set; } = new();
}

public sealed class ItemCommand
{
    public string OperationId { get; set; } = "";
    public string ItemId { get; set; } = "";
}

public sealed class CommandReceipt
{
    public string OperationId { get; set; } = "";
    public string ItemId { get; set; } = "";
    public string Action { get; set; } = "";
    public string Message { get; set; } = "";
}

public static class EquipmentRules
{
    public static void Transfer(EquipmentInstance item)
    {
        if (item.Scope != EquipmentScope.Idle) throw new InvalidOperationException("Este item já saiu do idle.");
        item.Scope = EquipmentScope.Shared;
    }
    public static void Equip(EquipmentInstance item)
    {
        if (item.Scope != EquipmentScope.Shared) throw new InvalidOperationException("Envie o item ao inventário compartilhado antes de equipar.");
        item.Scope = EquipmentScope.Equipped; item.Bound = true;
    }
    public static int IncomingDamage(int raw, int defense) => Math.Max(1, raw - defense);
}
