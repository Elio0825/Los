using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Opener;
using ElliotZ;
using los.BLM;
using los.BLM.Helper; // 你的新 BlackMageSetting 在这个命名空间里

using Los.BLM.SlotResolver.GCD;
using Los.BLM.SlotResolver.GCD.AOE;
using los.BLM.SlotResolver.GCD.单体;
using Los.BLM.SlotResolver.GCD.单体;
using Oblivion.BLM.SlotResolver.Ability;
using Oblivion.BLM.SlotResolver.GCD;
using Oblivion.BLM.SlotResolver.GCD.单体;
using Oblivion.BLM.SlotResolver.Opener;
using Oblivion.BLM.SlotResolver.Special; // 如果 TriggerActionQt 等在这个命名空间，没有的话这行删掉或改成正确的

namespace Oblivion.BLM;

public static class BlackMageACR
{
    private const long Version = 202410172317;
    public static string settingFolderPath = "";

    // ✅ 唯一的一份 SlotResolver 列表，之前“占位那一行”删掉
    public static readonly List<SlotResolverData> SlotResolverData =
    [
        // GCD
        new(new TTK(), SlotMode.Gcd),
        new(new 异言(), SlotMode.Gcd),
        new(new 秽浊(), SlotMode.Gcd),
        new(new 雷1(), SlotMode.Gcd),
        new(new 雷2(), SlotMode.Gcd),
        new(new 瞬发gcd触发器(), SlotMode.Gcd),
        new(new 火群(), SlotMode.Gcd),
        new(new 冰群(), SlotMode.Gcd),
        new(new 冰单100(), SlotMode.Gcd),
        new(new 火单100(), SlotMode.Gcd),
        new(new 冰单90(), SlotMode.Gcd),
        new(new 火单90(), SlotMode.Gcd),
        new(new 冰单80(), SlotMode.Gcd),
        new(new 火单80(), SlotMode.Gcd),
        new(new 冰单70(), SlotMode.Gcd),
        new(new 火单70(), SlotMode.Gcd),
        new(new 核爆补耀星(), SlotMode.Gcd),
        new(new 即刻三连(), SlotMode.Always),

        // Ability
        new(new 星灵移位(), SlotMode.OffGcd),
        new(new 即刻(), SlotMode.OffGcd),
        new(new 三连咏唱(), SlotMode.OffGcd),
        new(new 醒梦(), SlotMode.OffGcd),
        new(new 墨泉(), SlotMode.OffGcd),
        new(new 详述(), SlotMode.OffGcd),
        new(new 黑魔纹(), SlotMode.OffGcd),
    ];

    // ====================
    // Init：加载设置 + 新 UI
    // ====================
    public static void Init(string settingFolder)
    {
        settingFolderPath = settingFolder;

        // ⚠ GlobalSetting.Build 在你这版只接受两个参数
        // 第二个参数通常是：是否高难模式 / 是否高端 ACR（参考 RprRotationEntry）
        GlobalSetting.Build(settingFolder, false);

        // 你的新 BlackMageSetting（第二份）
        BlackMageSetting.Build(settingFolder);

        // 新 UI（JobView + MacroManager）
        los.BLM.QtUI.Qt.Build();
    }

    // ====================
    // Build：构建 Rotation
    // ====================
    public static Rotation Build()
    {
        return new Rotation(SlotResolverData)
        {
            TargetJob = Jobs.BlackMage,
            AcrType   = AcrType.HighEnd,
            MinLevel  = 70,
            MaxLevel  = 100,
            Description = "请打开悬浮窗查看设置",
        }
        .AddOpener(GetOpener)                                   // 起手逻辑
        .SetRotationEventHandler(new BLMEvetHandle())           // 事件处理器（含 PreCombat/NoTarget）
        .AddSlotSequences(特殊序列.Build())                     // 特殊固定序列

        // 如果你有 TriggerActionQt / TriggerActionHotkey / TriggerActionNewQt，可以加上：
        

        // 高优先级插入检测
        .AddCanUseHighPrioritySlotCheck(Helper.HighPrioritySlotCheckFunc);
    }

    // ====================
    // GetOpener：根据等级 & 设置返回起手
    // ====================
    private static IOpener? GetOpener(uint level)
    {
        // 如果要用 QT 控制是否启用起手，可以在这里加判断：
        // if (!BlackMageQT.GetQt("起手")) return null;

        if (level == 100)
        {
            if (BlackMageSetting.Instance.标准57)   return new Opener57();
            if (BlackMageSetting.Instance.核爆起手) return new Opener核爆();
            if (BlackMageSetting.Instance.开挂循环) return new Opener57开挂循环();
        }

        if (level >= 90 && level < 100) return new Opener_lv90();
        if (level >= 80 && level < 90)  return new Opener_lv80();
        if (level >= 70 && level < 80)  return new Opener_lv70();
        return null;
    }
}