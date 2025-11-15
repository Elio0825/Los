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

        // === ToFire 阶段：火三起手 ===
        if (bd.LoopPhase == BlmLoopPhase.ToFire)
        {
            // 确保火层数拉满
            if (BLMHelper.火层数 < 3)
                return Skill.火三;

            // AF3 已经就绪，进入 FireCore
            bd.LoopPhase = BlmLoopPhase.FireCore;
        }

        // === FireCore 阶段：F4 + AF 悖论 ===
        if (bd.LoopPhase == BlmLoopPhase.FireCore)
        {
            // 1) AF 悖论优先：有指示 & 这一轮还没打过 & 蓝量还能支持后续 F4
            if (!bd.UsedAfParadoxThisCycle
                && BLMHelper.悖论指示
                && mp >= 4000)
            {
                bd.UsedAfParadoxThisCycle = true;
                return Skill.悖论;
            }

            // 2) F4 计数：理论上还能打几发 F4
            int theoreticalF4Left = mp / 2400;

            if (bd.F4CountThisAF < 6 && theoreticalF4Left > 0)
            {
                return Skill.火四;
            }

            // 3) 如果 Astral Soul 已经满层，准备进 Finisher
            if (BLMHelper.耀星层数 >= 6)
            {
                bd.LoopPhase = BlmLoopPhase.Finisher;
                // 交给 Finisher 阶段决策
                return DecideFinisher(mp, bd);
            }

            // 4) 否则蓝量不够，再考虑提前收尾
            if (mp < 2400)
                return DecideFinisher(mp, bd);

            // 保底：再打一发火四
            return Skill.火四;
        }

        // === Finisher 阶段：耀星 + 绝望 ===
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