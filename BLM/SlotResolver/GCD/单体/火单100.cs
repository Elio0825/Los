using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.SlotResolver.Data;
using los.BLM.SlotResolver;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Oblivion.BLM.SlotResolver.GCD.单体;

public class 火单100 : ISlotResolver
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
        if (!BLMHelper.火状态) return -2;
        if (BLMHelper.三目标aoe() || BLMHelper.双目标aoe()) return -234;

        // 移动中且没有瞬发，就不要在这里抢 GCD
        if (Helper.IsMove && !Helper.可瞬发()) return -99;

        _skillId = GetSkillId();
        if (_skillId == 0) return -1;
        return (int)_skillId;
    }

    private uint GetSkillId()
    {
        var bd = BattleData.Instance;

        // 用你封装好的蓝量，并转成 int 方便后面计算
        int mp = (int)Helper.蓝量;

        if (bd.LoopPhase == BlmLoopPhase.ToFire)
        {
            if (!bd.UsedAfParadoxThisCycle && BLMHelper.悖论指示)
            {
                bd.UsedAfParadoxThisCycle = true;
                return Skill.悖论;
            }

            if (BLMHelper.火层数 < 3)
                return Skill.火三;

            bd.LoopPhase = BlmLoopPhase.FireCore;
        }

        if (bd.LoopPhase == BlmLoopPhase.FireCore)
        {
            if (!bd.UsedAfParadoxThisCycle && BLMHelper.悖论指示)
            {
                bd.UsedAfParadoxThisCycle = true;
                return Skill.悖论;
            }

            if (bd.F4CountThisAF < 6 && mp >= 800)
                return Skill.火四;

            if (BLMHelper.耀星层数 >= 6 || mp < 800)
                bd.LoopPhase = BlmLoopPhase.Finisher;
        }

        if (bd.LoopPhase == BlmLoopPhase.Finisher)
        {
            return DecideFinisher(mp, bd);
        }

        // 其它 Phase（比如冰相）不该让火单出手
        return 0;
    }

    private static uint DecideFinisher(int mp, BattleData bd)
    {
        // 优先打耀星（耀星层>=6 且还没耀）
        if (!bd.UsedFlareThisCycle && BLMHelper.耀星层数 >= 6)
        {
            bd.UsedFlareThisCycle = true;
            return Skill.耀星;
        }

        // 然后看绝望：蓝量足够 & 这一轮还没绝望
        if (!bd.UsedDespairThisCycle && mp >= 800)
        {
            bd.UsedDespairThisCycle = true;
            return Skill.绝望;
        }

        // 绝望打完了，后续交给冰单回冰，火单返回 0
        return 0;
    }
}
