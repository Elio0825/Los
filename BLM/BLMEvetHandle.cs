using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using los.BLM;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Oblivion.BLM;

/// <summary>
/// 黑魔法师职业的事件处理类，实现了IRotationEventHandler接口

public class BLMEvetHandle : IRotationEventHandler
{
    private int 释放技能时状态 = 0;
   
    private readonly HashSet<uint> _gcdSpellIds = new HashSet<uint>
    {
        Skill.冰一,Skill.冰三,Skill.冰冻.GetActionChange(),Skill.冰澈,Skill.玄冰,
        Skill.火一,Skill.火三,Skill.火二.GetActionChange(),Skill.火四,Skill.核爆,Skill.绝望,Skill.耀星,
        Skill.异言,Skill.悖论,Skill.秽浊,Skill.雷一.GetActionChange(),Skill.雷二.GetActionChange(),Skill.崩溃,Skill.灵极魂
    };

    /// <summary>
    /// 黑魔法师能力技(oGCD)技能ID集合，用于识别和处理非GCD技能的使用
    /// 包含黑魔纹、三连咏唱、墨泉等辅助技能
    /// </summary>
    private readonly HashSet<uint> _ogcdSpellIds = new HashSet<uint>
    {
        Skill.黑魔纹,Skill.三连,Skill.墨泉,Skill.即刻,Skill.星灵移位,Skill.醒梦,Skill.详述
    };

    private readonly uint[] _fireSpellIds = new[]
    {
        Skill.核爆, Skill.火三, Skill.火四, Skill.绝望, Skill.火二.GetActionChange(), Skill.耀星, Skill.悖论
    };

    private readonly uint[] _bSpellIds = new[]
    {
        Skill.悖论, Skill.冰三, Skill.冰澈, Skill.冰冻.GetActionChange(), Skill.玄冰, Skill.灵极魂
    };

    public async Task OnPreCombat()
    {
        if (Core.Me.InCombat()) 
            return;
        if (!BlackMageQT.GetQt("Boss上天"))
            return;
        // 火状态下使用星灵移位
        if (BLMHelper.火状态&&Skill.星灵移位.GetSpell().IsReadyWithCanCast())
        {
            await Skill.星灵移位.GetSpell(SpellTargetType.Self).Cast();
        }

        // 冰状态且条件满足时使用灵极魂
        if (BLMHelper.冰状态 && (BLMHelper.冰层数 < 3 || BLMHelper.冰针 < 3 || Core.Me.CurrentMp < 10000))
        {
            await Skill.灵极魂.GetSpell(SpellTargetType.Self).Cast();
            await Task.Delay(100);
        }
        await Task.CompletedTask;
    }


    public void OnResetBattle()
    {
        // 创建新的战斗数据实例并重置所有状态标志
        los.BLM.SlotResolver.BattleData.Instance = new BattleData();
        los.BLM.SlotResolver.BattleData.Instance.IsInnerOpener = false;
        los.BLM.SlotResolver.BattleData.Instance.需要即刻 = false;
        los.BLM.SlotResolver.BattleData.Instance.需要瞬发gcd = false;
        los.BLM.SlotResolver.BattleData.Instance.正在特殊循环中 = false;
        // 重置QT状态
        BlackMageQT.Reset();
        los.BLM.SlotResolver.BattleData.Reset();
        los.BLM.SlotResolver.BattleData.RebuildSettings();
    }

    private int 转圈次数 = 0;
    public async Task OnNoTarget()
    {
        // 战斗时间小于10秒时不处理
        if (AI.Instance.BattleData.CurrBattleTimeInMs < 10 * 1000) return;
        
        // 重置技能状态标志
        los.BLM.SlotResolver.BattleData.Instance.需要即刻 = false;
        los.BLM.SlotResolver.BattleData.Instance.需要瞬发gcd = false;
        los.BLM.SlotResolver.BattleData.Instance.正在特殊循环中 = false;
        
        // 处理Boss上天特殊情况
        if (BlackMageQT.GetQt("Boss上天"))
        {
            if (BLMHelper.火状态&&Skill.星灵移位.GetSpell().IsReadyWithCanCast())
            {
                await Skill.星灵移位.GetSpell(SpellTargetType.Self).Cast();
            }

            // 冰状态且条件满足时使用灵极魂
            if (BLMHelper.冰状态 && (BLMHelper.冰层数 < 3 || BLMHelper.冰针 < 3 || Core.Me.CurrentMp < 10000))
            {
                if(转圈次数<3)
                {
                    await Skill.灵极魂.GetSpell(SpellTargetType.Self).Cast();
                    转圈次数++;
                }
            }
        }
        await Task.CompletedTask;
    }


    public void OnSpellCastSuccess(Slot slot, Spell spell)
    {
        if (_gcdSpellIds.Contains(spell.Id))
            los.BLM.SlotResolver.BattleData.Instance.已使用瞬发 = GCDHelper.GetGCDCooldown() >= (Core.Me.HasAura(Buffs.咏速Buff) ? 1500 : 1700);

    }
    


    public void AfterSpell(Slot slot, Spell spell)
{
    uint id = spell.Id;

    // ================= 原来这块你的旧逻辑 =================

    if (释放技能时状态 == 1)
    {
        if (id == Skill.火二.GetActionChange() || id == Skill.火三 || id == Skill.星灵移位)
        {
            转圈次数 = 0;
            los.BLM.SlotResolver.BattleData.Instance.上一轮循环 = new List<uint>(los.BLM.SlotResolver.BattleData.Instance.冰状态gcd);
            los.BLM.SlotResolver.BattleData.Instance.冰状态gcd.Clear();
            if (id == Skill.火二.GetActionChange() || id == Skill.火三)
            {
                los.BLM.SlotResolver.BattleData.Instance.火状态gcd.Add(id);
            }
        }
        else
        {
            los.BLM.SlotResolver.BattleData.Instance.冰状态gcd.Add(id);
        }
    }
    else if (释放技能时状态 == 2)
    {
        if (id == Skill.冰冻.GetActionChange() || id == Skill.冰三 || id == Skill.星灵移位)
        {
            los.BLM.SlotResolver.BattleData.Instance.上一轮循环 = new List<uint>(los.BLM.SlotResolver.BattleData.Instance.火状态gcd);
            los.BLM.SlotResolver.BattleData.Instance.火状态gcd.Clear();
            if (id == Skill.冰冻.GetActionChange() || id == Skill.冰三)
            {
                los.BLM.SlotResolver.BattleData.Instance.冰状态gcd.Add(id);
            }
        }
        else
        {
            los.BLM.SlotResolver.BattleData.Instance.火状态gcd.Add(id);
        }
    }
    else if (释放技能时状态 == 0)
    {
        los.BLM.SlotResolver.BattleData.Instance.冰状态gcd.Clear();
        los.BLM.SlotResolver.BattleData.Instance.火状态gcd.Clear();
        if (_fireSpellIds.Contains(id))
        {
            los.BLM.SlotResolver.BattleData.Instance.火状态gcd.Add(id);
        }
        else if (_bSpellIds.Contains(id))
        {
            los.BLM.SlotResolver.BattleData.Instance.冰状态gcd.Add(id);
        }
    }

    if (id == Skill.星灵移位)
    {
        if (BLMHelper.冰状态) 释放技能时状态 = 1;
        else if (BLMHelper.火状态) 释放技能时状态 = 2;
        else 释放技能时状态 = 0;

        if (BLMHelper.冰状态 && BLMHelper.冰针 == 3)
        {
            los.BLM.SlotResolver.BattleData.Instance.三冰针进冰 = true;
        }
    }

    // GCD 技能施放后的通用状态
    if (_gcdSpellIds.Contains(id))
    {
        los.BLM.SlotResolver.BattleData.Instance.前一gcd = id;
        AI.Instance.BattleData.CurrGcdAbilityCount = 1;
        los.BLM.SlotResolver.BattleData.Instance.已使用瞬发 =
            GCDHelper.GetGCDCooldown() >= (Core.Me.HasAura(Buffs.咏速Buff) ? 1500 : 1700);

        if (BLMHelper.冰状态) 释放技能时状态 = 1;
        else if (BLMHelper.火状态) 释放技能时状态 = 2;
        else 释放技能时状态 = 0;
    }

    if (los.BLM.SlotResolver.BattleData.Instance.三冰针进冰)
    {
        if (id == Skill.冰澈 || id == Skill.玄冰)
        {
            los.BLM.SlotResolver.BattleData.Instance.三冰针进冰 = false;
        }
    }

    // oGCD 施放后的前一能力技记录
    if (_ogcdSpellIds.Contains(id))
    {
        los.BLM.SlotResolver.BattleData.Instance.前一能力技 = id;
    }

    // 瞬发使用后的通用处理
    if (los.BLM.SlotResolver.BattleData.Instance.已使用瞬发)
    {
        los.BLM.SlotResolver.BattleData.Instance.需要瞬发gcd = false;
        AI.Instance.BattleData.CurrGcdAbilityCount = 2;
    }

    // ================== 从这里开始是「100级循环状态」新增部分 ==================

    var bd = los.BLM.SlotResolver.BattleData.Instance;

    // 1. 统计火四数量
    if (id == Skill.火四)
        bd.F4CountThisAF++;

    // 2. 悖论使用记录（根据当前是在冰态还是火态）
    if (id == Skill.悖论)
    {
        if (BLMHelper.冰状态)
            bd.UsedUiParadoxThisCycle = true;

        if (BLMHelper.火状态)
            bd.UsedAfParadoxThisCycle = true;
    }

    // 3. 耀星 / 绝望使用记录
    if (id == Skill.耀星)
        bd.UsedFlareThisCycle = true;

    if (id == Skill.绝望)
        bd.UsedDespairThisCycle = true;

    // 4. 按施放的 GCD 推进 LoopPhase
    switch (bd.LoopPhase)
    {
        case BlmLoopPhase.None:
            // 从冰三开始认为进入冰相核心
            if (id == Skill.冰三)
                bd.LoopPhase = BlmLoopPhase.IceCore;
            break;

        case BlmLoopPhase.IceCore:
            // 冰态下打出悖论，准备转火
            if (id == Skill.悖论 && BLMHelper.冰状态)
                bd.LoopPhase = BlmLoopPhase.ToFire;
            break;

        case BlmLoopPhase.ToFire:
            // 火三之后进入火相核心
            if (id == Skill.火三)
                bd.LoopPhase = BlmLoopPhase.FireCore;
            break;

        case BlmLoopPhase.FireCore:
            // 耀星之后进入收尾阶段
            if (id == Skill.耀星)
                bd.LoopPhase = BlmLoopPhase.Finisher;
            break;

        case BlmLoopPhase.Finisher:
            // 这里暂时不强制变化，真正的重置看下面「自愈」逻辑
            break;
    }

    // 5. 循环自愈：当我们回到「稳定冰三态 + 蓝量够高」时，认为一轮结束，重置计数器
    if (BLMHelper.冰状态
        && BLMHelper.冰层数 == 3
        && !BLMHelper.火状态
        && Core.Me.CurrentMp >= 8000)
    {
        bd.ResetLoop();
    }
}

    public void OnBattleUpdate(int currTimeInMs)
    {
        
        // 可瞬发状态下不需要即刻
        if (Helper.可瞬发()) los.BLM.SlotResolver.BattleData.Instance.需要即刻 = false;
        
        // 处理角色发呆状态
        if (BLMHelper.在发呆())
        {
            // 有可用瞬发技能时设置需要瞬发GCD标志
            if (BLMHelper.可用瞬发() != 0)
                los.BLM.SlotResolver.BattleData.Instance.需要瞬发gcd = true;
            else
            {
                los.BLM.SlotResolver.BattleData.Instance.需要即刻 = true;
            }
        }
        
        // 正在施法时重置相关状态
        if (Core.Me.IsCasting)
        {
            los.BLM.SlotResolver.BattleData.Instance.需要即刻 = false;
            los.BLM.SlotResolver.BattleData.Instance.已使用瞬发 = false;
            los.BLM.SlotResolver.BattleData.Instance.需要瞬发gcd = false;
        }
        


        // 处理特殊循环状态
        if (!los.BLM.SlotResolver.BattleData.Instance.特供循环) los.BLM.SlotResolver.BattleData.Instance.正在特殊循环中 = false;

        // 更新各种战斗状态数据
        /*BattleData.Instance.已存在黑魔纹 = Helper.有buff(737);
        BattleData.Instance.能使用耀星 = BLMHelper.能使用耀星();
        BattleData.Instance.能使用的火四个数 = BLMHelper.能使用的火四个数();
        BattleData.Instance.火循环剩余gcd = BLMHelper.火循环gcd();
        BattleData.Instance.冰循环剩余gcd = BLMHelper.冰循环gcd();
        BattleData.Instance.能星灵转冰 = BLMHelper.能星灵转冰();*/
    }


    public void OnEnterRotation()
    {
        // 输出欢迎信息
        LogHelper.Print(
            "欢迎使用Los的黑魔acr，反馈请到：");
        LogHelper.Print("建议设置提前使用gcd时间为50，使用fuckanime三插，DR能力技动画减少");

        // 检查全局设置并给出建议
        if (Helper.GlobalSettings.NoClipGCD3)
            LogHelper.PrintError("建议不要在acr全局设置中勾选【全局能力技不卡GCD】选项");
        // 重置开场标志
        los.BLM.SlotResolver.BattleData.Instance.IsInnerOpener = false;

    }


    public void OnExitRotation()
    {
        // 保存黑魔法师设置
        BlackMageSetting.Instance.Save();
        // 保存QT状态
        
        
    }

    public void OnTerritoryChanged()
    {
        // 重置QT状态
        
    }
}