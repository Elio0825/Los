using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Oblivion.BLM.SlotResolver.Ability;

public class 墨泉 : ISlotResolver
{
    private readonly uint _skillId = Skill.墨泉;
    private Spell? GetSpell()
    {
        
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
        if (!BlackMageQT.GetQt("魔泉")) return -5;
        if (_skillId.GetSpell().Cooldown.TotalMilliseconds > 0) return -1;
        if (!BLMHelper.火状态) return -2;
        if (Core.Me.CurrentMp > 800) return -3;
        if (BlackMageQT.GetQt("TTK")) return 999;
        //if (!BattleData.Instance.已使用瞬发 && !QT.Instance.GetQt("能力技卡G放")) return -4;
        if (BLMHelper.耀星层数 == 6 )return -6;
        if (GCDHelper.GetGCDCooldown() < 500) return -7;
        if (los.BLM.SlotResolver.BattleData.Instance.前一gcd is Skill.冰澈 or Skill.玄冰 && BattleData.Instance.前一能力技 == Skill.星灵移位) return -8;
        return 1;
    }
}
