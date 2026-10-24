using Aetherforge.Domain;
using Godot;
using System;
using System.Globalization;

/// <summary>Formatting only. All totals, HP policy and authorization come from the domain.</summary>
public static class EquipmentPresentation
{
    private static readonly CultureInfo Pt=CultureInfo.GetCultureInfo("pt-BR");
    public const string SessionWarning="Equipamento e ganhos desta sessão: perdidos ao fechar.";
    public static string Number(double value) => value.ToString("0.##",Pt);
    public static string HealthNumber(double value) => value>0 && value<.01?"menos de 0,01":value.ToString("0.####",Pt);
    public static string Role(PartyRole role)=>role==PartyRole.Tank?"Guarda":role==PartyRole.Dps?"DPS":"Cura";
    public static string Delta(double value) => value==0 ? "0 · Sem alteração" :
        (Math.Abs(value)<.01 ? (value>0?"+":"−")+"menos de 0,01" : (value>0?"+":"−")+Number(Math.Abs(value)))+(value>0?" · Ganho":" · Perda");
    public static string Slot(LootSlot slot,PartyRole role) => slot==LootSlot.Weapon ? role==PartyRole.Healer?"Foco":"Arma" : slot==LootSlot.Armor?"Armadura":"Acessório";
    public static string Item(ExpeditionDrop? item) => item==null ? "Vazio · bônus 0" : ExpeditionLootView.Describe(item).Split('\n')[0]+" · "+
        (item.AttackBonus>0?$"+{item.AttackBonus} ataque"+(item.Role==PartyRole.Healer?" / infusão":""):item.DefenseBonus>0?$"+{item.DefenseBonus} defesa":$"+{item.HealthBonus} vida");
    public static string Reason(ExpeditionCode code) => code switch
    {
        ExpeditionCode.Applied => "Confira os valores antes de confirmar.",
        ExpeditionCode.NoChange => "Sem alteração.",
        ExpeditionCode.InCombat => "Em combate. Gerencie após a vitória. Pausar não libera a troca.",
        ExpeditionCode.ManagementRequired => "Ative o gerenciamento entre encontros para trocar.",
        ExpeditionCode.ManagementActive => "Conclua o gerenciamento para controlar a pausa.",
        ExpeditionCode.RoleMismatch => "Função incompatível. Confira a função da peça e o alvo.",
        ExpeditionCode.SlotMismatch => "Slot incompatível. Confira o slot da peça.",
        ExpeditionCode.BagFull => "Bolsa cheia (21/21). Abra uma vaga antes de desequipar.",
        ExpeditionCode.ItemNotAtExpectedLocation => "Item não está na bolsa ou no slot esperado. Selecione a peça novamente.",
        ExpeditionCode.StaleComparison => "Comparação desatualizada; confira os valores novamente.",
        ExpeditionCode.SessionMismatch or ExpeditionCode.AttemptMismatch => "A sessão ou tentativa mudou. Abra uma nova comparação.",
        ExpeditionCode.MemberNotFound => "Integrante indisponível. Selecione um integrante da equipe.",
        ExpeditionCode.NumericOverflow or ExpeditionCode.InvalidState => "Não foi possível aplicar a troca. O estado foi preservado.",
        ExpeditionCode.CheckpointFailed or ExpeditionCode.CommitIndeterminate => "Não foi possível confirmar a operação. O estado deve ser verificado.",
        _ => "Não foi possível validar a ação. Abra uma nova comparação."
    };
    public static string Composition(EffectiveStats s) =>
        $"Ataque: base {Number(s.BaseAttack)} + nível {Number(s.LevelAttack-s.BaseAttack)} + equipamento {s.AttackBonus} = {Number(s.Attack)}\n"+
        $"Defesa: base {Number(s.BaseDefense)} + nível {Number(s.LevelDefense-s.BaseDefense)} + equipamento {s.DefenseBonus} = {Number(s.Defense)}\n"+
        $"Vida máxima: base {Number(s.BaseHealth)} + nível {Number(s.LevelHealth-s.BaseHealth)} + equipamento {s.HealthBonus} = {Number(s.MaxHealth)} PV";
    public static string Comparison(EquipmentPreview p)
    {
        if(p.Before==null || p.After==null || p.BlockReason is ExpeditionCode.RoleMismatch or ExpeditionCode.SlotMismatch or ExpeditionCode.ItemNotAtExpectedLocation)
            return "Total atual\n"+(p.Before==null?"Indisponível":Composition(p.Before))+"\nApós: indisponível. "+Reason(p.BlockReason);
        var a=p.Before; var b=p.After;
        string Line(string name,double before,double after,string unit="") => $"{name}\nTotal atual: {Number(before)} → Total após troca: {Number(after)} {unit}\nDiferença: {Delta(after-before)} {unit}\n";
        return Line("Ataque",a.Attack,b.Attack)+Line("Defesa",a.Defense,b.Defense)+Line("Vida máxima",a.MaxHealth,b.MaxHealth,"PV")+
            $"Vida atual/máxima: {HealthNumber(p.HealthBefore)}/{Number(a.MaxHealth)} → {HealthNumber(p.HealthAfter)}/{Number(b.MaxHealth)} PV\n"+
            (p.HealthAfter<p.HealthBefore?$"Corte de vida: {Delta(p.HealthAfter-p.HealthBefore)} PV.\n":"Vida atual sem alteração; equipar não cura.\n")+
            (p.HealthBefore==0?"Continua caído até reiniciar a rota.\n":"")+
            Line("DPS básico teórico, antes da defesa",p.DpsBefore,p.DpsAfter,"/ s")+
            "Intervalo de ataque inalterado.\n"+
            (a.InfusionPotential>0?Line("Potência da infusão",a.InfusionPotential,b.InfusionPotential,"PV")+"Cura real limitada à vida faltante; não ressuscita.\n":"")+
            "\nComposição após troca\n"+Composition(b);
    }
    public static Button Button(string name,string text,Action action,bool primary=false)
    {
        var button=BrassUi.Button(text,action); button.Name=name;
        Style(button,primary); return button;
    }
    public static void Style(Button button,bool primary=false)
    {
        BrassUi.DossierButton(button,primary);
        button.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        button.SizeFlagsHorizontal=Control.SizeFlags.ExpandFill;
        foreach(var state in new[]{"normal","hover","pressed","hover_pressed","disabled"})
            button.AddThemeStyleboxOverride(state,BrassUi.Box(state=="disabled" || !primary?BrassUi.Paper:BrassUi.Wine,BrassUi.Tobacco,1,6,3));
        foreach(var state in new[]{"font_color","font_hover_color","font_pressed_color","font_hover_pressed_color","font_focus_color"})
            button.AddThemeColorOverride(state,primary?BrassUi.Linen:BrassUi.Soot);
        button.AddThemeColorOverride("font_disabled_color",BrassUi.Tobacco);
        var focus=BrassUi.Box(Colors.Transparent,BrassUi.Wine,2,2,2); focus.ExpandMarginLeft=2; focus.ExpandMarginRight=2;
        focus.ExpandMarginTop=2; focus.ExpandMarginBottom=2; button.AddThemeStyleboxOverride("focus",focus);
    }
}
