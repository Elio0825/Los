using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;

namespace Oblivion.BLM.SlotResolver.Ability;

public class 醒梦 : ISlotResolver
{
    private readonly uint _skillId = Skill.醒梦;
    private Spell? GetSpell()
    {
        return _skillId.GetSpell(SpellTargetType.Self);
    }
    public int Check()
    {
        //if (!QT.Instance.GetQt("醒梦")) return -2;
        if (_skillId.GetSpell().Cooldown.TotalMilliseconds > 0) return -1;
        //if (BLMHelper.冰状态 && Core.Me.CurrentMp < 800 && Skill.墨泉.GetSpell().Cooldown.TotalSeconds < 8 && BLMHelper.冰层数 < 3) return 1;
        if (BlackMageQT.GetQt("TTK")) return 1;
        return -99;
    }

    public void Build(Slot slot)
    {
        var spell = GetSpell();
        if (spell != null) 
            slot.Add(spell);
    }
}