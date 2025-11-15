using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;

namespace Oblivion.BLM.SlotResolver.Ability;

public class 黑魔纹 : ISlotResolver
{
    private readonly uint _skillId = Skill.黑魔纹;
    private Spell? GetSpell()
    {
        //if (GCDHelper.GetGCDCooldown() < 500) return null;
        return  _skillId.GetSpell(SpellTargetType.Self);
    }
    public void Build(Slot slot)
    {
        var spell = GetSpell();
        if (spell != null) 
            slot.Add(spell);
    }

    public int Check()
    {
        if (!BlackMageQT.GetQt("黑魔纹")) return -5;
        if (_skillId.RecentlyUsed(2000)) return -6;
        if (_skillId.GetSpell().Charges < 1) return -1;
        if (Helper.有buff(737)) return -3;
        if (GCDHelper.GetGCDCooldown() < 500) return -4;
        return 1;
    }
}
