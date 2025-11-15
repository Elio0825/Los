using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;

namespace Los.BLM.SlotResolver.GCD;

public class 秽浊 : ISlotResolver
{
    private readonly uint _skillId = Skill.秽浊;
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
        if (!BlackMageQT.GetQt("通晓")) return -5;
        if (!BLMHelper.双目标aoe() && !BLMHelper.三目标aoe() && Core.Me.Level >= 80) return -100;
        if (BlackMageQT.GetQt("倾泻资源") && BLMHelper.通晓层数 > 0) return 666;
        if (Core.Me.Level >= 98)
        {
            if (BLMHelper.通晓层数 == 3 && BLMHelper.通晓剩余时间 <= 10000) return 2;
            if (BLMHelper.通晓层数 == 3 && Skill.详述.GetSpell().AbilityCoolDownInNextXgcDsWindow(1)) return 3;
            if (BLMHelper.火状态)
            {
                if (Core.Me.CurrentMp < 800 && BLMHelper.耀星层数 != 6)
                {
                    if (Skill.墨泉.技能CD() < 300 && Skill.墨泉.GetSpell().IsReadyWithCanCast()) return -3;
                    if (Skill.墨泉.GetSpell().AbilityCoolDownInNextXgcDsWindow(2)) return 4;
                }
            }

        }

        if (Core.Me.Level >= 80 && Core.Me.Level < 98)
        {
            if (BLMHelper.通晓层数 == 2 && BLMHelper.通晓剩余时间 < 8000) return 2;
            if (BLMHelper.通晓层数 == 2 && Core.Me.Level >= 86 && Skill.详述.GetSpell().AbilityCoolDownInNextXgcDsWindow(1)) return 3;
        }
        if (Core.Me.Level<80 && Core.Me.Level >= 70)
            if (BLMHelper.通晓层数 >= 1 ) return 2;

        return -99;
    }
}
