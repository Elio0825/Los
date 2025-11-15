using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.GCD.AOE;

public class 雷2 : ISlotResolver
{
    private readonly uint _skillId = Skill.雷二.GetActionChange();
    private Spell? GetSpell()
    {
        //if (!_skillId.GetSpell().IsReadyWithCanCast()) return null;
        return BlackMageQT.GetQt("智能aoe目标")? _skillId.GetSpellBySmartTarget() : _skillId.GetSpell();
    }
    public void Build(Slot slot)
    {
        var spell = GetSpell();
        if (spell != null) 
            slot.Add(spell);
    }

    public int Check()
    {
        if (!BLMHelper.双目标aoe() && !BLMHelper.三目标aoe()) return -100;
        if (!BlackMageQT.GetQt("dot")) return -2;
        if (BlackMageQT.GetQt("dot")) return -5;
        if (BattleData.Instance.正在特殊循环中) return -4;
        if (BLMHelper.冰状态&&Core.Me.HasAura(Buffs.雷云buff) && BLMHelper.补dot()) return 1;
        return -99;
    }
}
