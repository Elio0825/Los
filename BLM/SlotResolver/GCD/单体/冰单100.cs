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

        // 1) 如果循环刚开始（或被打乱重置），从冰三起手
        if (bd.LoopPhase == BlmLoopPhase.None)
        {
            // 冰层不满，先冰三
            if (BLMHelper.冰层数 < 3)
                return Skill.冰三;

            // 冰针不满，冰澈填满
            if (BLMHelper.冰针 < 3)
                return Skill.冰澈;

            // 本轮还没打过冰四，先补上
            if (!bd.UsedIce4ThisCycle)
                return Skill.冰澈;

            // 有 UI 悖论指示，先打悖论
            if (BLMHelper.悖论指示)
            {
                bd.UsedUiParadoxThisCycle = true;
                bd.LoopPhase = BlmLoopPhase.ToFire;
                return Skill.悖论;
            }

            // 否则继续冰澈，保证回蓝/冰针
            return Skill.冰澈;
        }

        // 2) 已在 IceCore 阶段：确保 B3/B4/UI 悖论都完成
        if (bd.LoopPhase == BlmLoopPhase.IceCore)
        {
            if (BLMHelper.冰层数 < 3)
                return Skill.冰三;

            if (BLMHelper.冰针 < 3)
                return Skill.冰澈;

            if (!bd.UsedIce4ThisCycle)
                return Skill.冰澈;

            if (!bd.UsedUiParadoxThisCycle && BLMHelper.悖论指示)
            {
                bd.UsedUiParadoxThisCycle = true;
                bd.LoopPhase = BlmLoopPhase.ToFire;
                return Skill.悖论;
            }

            // 没悖论指示就继续冰澈当保底
            return Skill.冰澈;
        }

        // 3) ToFire / FireCore / Finisher 阶段，冰单一般不该出手（交给火单）
        return 0;
    }
}