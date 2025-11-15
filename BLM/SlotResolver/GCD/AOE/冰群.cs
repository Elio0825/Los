using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.GCD.AOE;

public class 冰群 : ISlotResolver
{
    private uint _skillId = 0;
    private Spell? GetSpell()
    {
        return BlackMageQT.GetQt("智能aoe目标")? _skillId.GetSpellBySmartTarget() : _skillId.GetSpell();
    }
    public void Build(Slot slot)
    {
        var spell = GetSpell();
        if (spell != null) 
            slot.Add(spell);
    }

    private uint GetSkillId()
    {
        if (BLMHelper.冰状态)
        {
            if (BLMHelper.冰针 < 3|| BattleData.Instance.三冰针进冰) return BLMHelper.三目标aoe() ? Skill.玄冰 : Skill.冰澈;
            if (!Skill.星灵移位.GetSpell().IsReadyWithCanCast())
            {
                if (BLMHelper.悖论指示 && !BLMHelper.三目标aoe()) return Skill.悖论;
                if (BLMHelper.通晓层数 > 0) return Skill.秽浊;
                if (Helper.有buff(Buffs.雷云buff)) return Skill.雷二.GetActionChange();
            }
        }

        if (BLMHelper.火状态)
        {
            if (BattleData.Instance.前一gcd is Skill.冰澈 or Skill.玄冰 && BattleData.Instance.前一能力技 == Skill.星灵移位) return 0;
            if (Core.Me.CurrentMp < 800) return BLMHelper.三目标aoe() ? Skill.冰冻.GetActionChange() : Skill.冰三;
        }
        if (!BLMHelper.冰状态 && !BLMHelper.火状态) return Skill.冰冻;
        return 0;
    }
    public int Check()
    {
        if (Core.Me.Level < 70) return -70;
        if (!(BLMHelper.三目标aoe() || BLMHelper.双目标aoe())) return -234;
        _skillId = GetSkillId();
        if (_skillId == 0) return -1;
        return (int)_skillId;
    }
}