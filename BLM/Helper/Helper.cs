using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ;
using los.BLM.SlotResolver.Data;
namespace los.BLM.Helper;

public static class Helper
{
    public const string AuthorName = "Los";
    private static int _GcdDuration = 0;


    public static bool 是否在副本中()
    {
        return Core.Resolve<MemApiCondition>().IsBoundByDuty();
    }
    public static long 蓝量 => Core.Me.CurrentMp;
    public static bool 可读条()
    {
        return !IsMove || 可瞬发();
    }

    public static double 复唱时间() => Core.Resolve<MemApiSpell>().GetGCDDuration();
    public static bool 可瞬发() => Core.Me.HasAura(Buffs.即刻Buff) || Core.Me.HasAura(Buffs.三连Buff);
    public static bool 是否在战斗中()
    {
        return Core.Me.InCombat();
    }
    /// <summary>
    /// 获取自身buff的剩余时间
    /// </summary>
    /// <param name="buffId"></param>
    /// <returns></returns>
    public static int GetAuraTimeLeft(uint buffId) => Core.Resolve<MemApiBuff>().GetAuraTimeleft(Core.Me, buffId, true);

    /// <summary>显示一个文本提示，用于在游戏中显示简短的消息。</summary>
    /// <param name="msg">要显示的消息文本。</param>
    /// <param name="s">文本提示的样式。支持蓝色提示（1）和红色提示（2）两种</param>
    /// <param name="time">文本提示显示的时间（单位毫秒）。如显示3秒，填写3000即可</param>
    public static void SendTips(string msg, int s = 1, int time = 3000) => Core.Resolve<MemApiChatMessage>()
        .Toast2(msg, s, time);

    public static bool IsMove => MoveHelper.IsMoving();

    /// <summary>
    /// 全局设置
    /// </summary>
    public static GeneralSettings GlobalSettings => SettingMgr.GetSetting<GeneralSettings>();

    /// <summary>
    /// 当前地图id
    /// </summary>
    public static uint GetTerritoyId => Core.Resolve<MemApiMap>().GetCurrTerrId();

    /// <summary>
    /// 返回可变技能的当前id
    /// </summary>
    public static uint GetActionChange(this uint spellId) => Core.Resolve<MemApiSpell>().CheckActionChange(spellId);
    public static uint AdaptiveId(this uint spellId) => Core.Resolve<MemApiSpell>()
        .CheckActionChange(spellId);
    public static bool IsUnlockWithRoleSkills(this Spell spell) {
        // dirty fix for now; need better ways to detect if a role skill is unlocked
        return SpellsDef.RoleSkills.Contains(spell.Id)
               || spell.IsUnlock();
    }
    public static bool CheckInHPQueue(this Spell spell) {
        if (spell.IsAbility()) {
            var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_OffGCD);
            return all.Contains(spell.Name);
        } else {
            var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_GCD);
            return all.Contains(spell.Name);
        }
    }
    private static List<string> HPQueueToStrList(Queue<Slot> src) {
        List<string> result = [];
        result.AddRange(collection: src.SelectMany(slot => slot.Actions)
            .Select(slotAction => slotAction.Spell.Name));
        return result;
    }
    public static bool CheckInHPQueueTop(this Spell spell) {
        if (spell.IsAbility()) {
            var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_OffGCD);
            return all.Count > 0 && all[0] == spell.Name;
        } else {
            var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_GCD);
            return all.Count > 0 && all[0] == spell.Name;
        }
    }

    /// <summary>
    /// 高优先级插入条件检测函数
    /// </summary>
    public static bool TargetIsBoss => Core.Me.GetCurrTarget().IsBoss();

    public static bool TargetIsDummy => Core.Me.GetCurrTarget().IsDummy();
    public static bool TargetIsBossOrDummy => TargetIsBoss || TargetIsDummy;
    public static uint GetTerritoryId => Core.Resolve<MemApiMap>().GetCurrTerrId();
    public static bool InCasualDutyNonBoss =>
        Core.Resolve<MemApiDuty>().InMission
        && Core.Resolve<MemApiDuty>().DutyMembersNumber() is 4 or 24 
        && GetTerritoryId is not (1048 or 1045 or 1046 or 1047)
        && !TargetIsBossOrDummy
        && !Core.Resolve<MemApiDuty>().InBossBattle;
    public static int HighPrioritySlotCheckFunc(SlotMode mode, Slot slot)
    {
        if (mode != SlotMode.OffGcd) return 1;
        //限制高优先能力技插入，只能在g窗口前半和后半打
        if (GCDHelper.GetGCDCooldown() is > 750 and < 1500) return -1;
        //连续的两个高优先能力技插入，在gcd前半窗口打，以免卡gcd
        if (slot.Actions.Count > 1 && GCDHelper.GetGCDCooldown() < 1500) return -1;
        return 1;
    }

    public static double 连击剩余时间 => Core.Resolve<MemApiSpell>().GetComboTimeLeft().TotalMilliseconds;

    public static bool 在近战范围内 =>
        Core.Me.Distance(Core.Me.GetCurrTarget()!) <= SettingMgr.GetSetting<GeneralSettings>().AttackRange;

    public static bool 在背身位 => Core.Resolve<MemApiTarget>().IsBehind;
    public static bool 在侧身位 => Core.Resolve<MemApiTarget>().IsFlanking;

    /// <summary>
    /// 充能技能还有多少冷却时间才可用
    /// </summary>
    /// <param name="skillId">技能id</param>
    /// <returns></returns>
    public static int 充能技能冷却时间(uint skillId)
    {
        var spell = skillId.GetSpell();
        return (int)(spell.Cooldown.TotalMilliseconds -
                     (spell.RecastTime.TotalMilliseconds / spell.MaxCharges) * (spell.MaxCharges - 1));
    }
    public static bool AnyAuraTimerLessThan(List<uint> auras, int timeLeft) {
        return Core.Me.StatusList.Any(aura => (aura.StatusId != 0)
                                              && (Math.Abs(aura.RemainingTime) * 1000.0 <= timeLeft)
                                              && auras.Contains(aura.StatusId));
    }

    public static bool 有buff(uint buffId) => Core.Me.HasAura(buffId);

    /// <summary>
    /// 自身有buff且时间小于
    /// </summary>
    public static bool Buff时间小于(uint buffId, int timeLeft)
    {
        if (!Core.Me.HasAura(buffId)) return false;
        return GetAuraTimeLeft(buffId) <= timeLeft;
    }

    /// <summary>
    /// 目标有buff且时间小于，有buff参数如果为false，则当目标没有玩家的buff是也返回true
    /// </summary>
    public static bool 目标Buff时间小于(uint buffId, int timeLeft, bool 有buff = true)
    {
        var target = Core.Me.GetCurrTarget();
        if (target == null) return false;

        if (有buff)
        {
            if (!target.HasLocalPlayerAura(buffId)) return false;
        }
        else
        {
            if (!target.HasLocalPlayerAura(buffId)) return true;
        }

        var time = Core.Resolve<MemApiBuff>().GetAuraTimeleft(target, buffId, true);
        return time <= timeLeft;
    }

    /// <summary>
    /// 在list中添加一个唯一的元素
    /// </summary>
    public static bool TryAdd<T>(this List<T> list, T item)
    {
        if (list.Contains(item)) return false;
        list.Add(item);
        return true;
    }

    public static bool 目标有任意我的buff(List<uint> buffs) =>
        buffs.Any(buff => Core.Me.GetCurrTarget()!.HasLocalPlayerAura(buff));


    public static IBattleChara? 最优aoe目标(this uint spellId, int count)
    {
        return TargetHelper.GetMostCanTargetObjects(spellId, count);
    }

    public static int 目标周围可选中敌人数量(this IBattleChara? target, int range)
    {
        return TargetHelper.GetNearbyEnemyCount(target, 25, range);
    }
    /// <summary>
    /// 获取非战斗状态时开了盾姿的人
    /// </summary>
    /// <returns></returns>
    public static IBattleChara? GetMt()
    {
        PartyHelper.UpdateAllies();
        return PartyHelper.CastableTanks
            .FirstOrDefault(p => p.HasAnyAura([743, 1833, 79, 91]));
    }

    public static bool In团辅()
    {
        //检测目标团辅
        List<uint> 目标团辅 = [背刺, 连环计];
        if (目标团辅.Any(buff => 目标Buff时间小于(buff, 15000))) return true;

        //检测自身团辅
        List<uint> 自身团辅 = [灼热之光, 星空, 占卜, 义结金兰, 战斗连祷, 大舞, 战斗之声, 鼓励, 神秘环];
        return 自身团辅.Any(buff => Buff时间小于(buff, 15000));
    }

    private static uint
        背刺 = 3849,
        强化药 = 49,
        灼热之光 = 2703,
        星空 = 3685,
        占卜 = 1878,
        义结金兰 = 1185,
        战斗连祷 = 786,
        大舞 = 1822,
        战斗之声 = 141,
        鼓励 = 1239,
        神秘环 = 2599,
        连环计 = 2617;
}