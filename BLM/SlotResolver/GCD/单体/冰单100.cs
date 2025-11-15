using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.SlotResolver.Data;
using los.BLM.SlotResolver;

namespace los.BLM.SlotResolver.GCD.单体;

public class 冰单100 : ISlotResolver
{
    private uint _skillId = 0;

    private Spell? GetSpell()
    {
        if (_skillId == 0) return null;
        return _skillId.GetSpell();
    }

    public void Build(Slot slot)
    {
        var spell = GetSpell();
        if (spell != null)
            slot.Add(spell);
    }

    public int Check()
    {
        if (Core.Me.Level < 100) return -100;
        if (!BLMHelper.冰状态) return -2;
        if (BLMHelper.双目标aoe() || BLMHelper.三目标aoe()) return -3;

        _skillId = GetSkillId();
        if (_skillId == 0) return -1;
        return (int)_skillId;
    }

    private uint GetSkillId()
    {
        var bd = BattleData.Instance;

        if (bd.LoopPhase is BlmLoopPhase.None or BlmLoopPhase.IceCore)
        {
            if (BLMHelper.冰层数 < 3)
                return Skill.冰三;

            if (BLMHelper.冰针 < 3 || !bd.UsedIce4ThisCycle)
                return Skill.冰澈;

            if (BLMHelper.悖论指示)
            {
                bd.UsedUiParadoxThisCycle = true;
                bd.LoopPhase = BlmLoopPhase.ToFire;
                return Skill.悖论;
            }

            return Skill.冰澈;
        }

        return 0;
    }
}
