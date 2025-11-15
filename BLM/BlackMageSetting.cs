using AEAssist.Helper;
using AEAssist.IO;
using ElliotZ.ModernJobViewFramework;

namespace los.BLM
{
    public class BlackMageSetting
    {
        public static BlackMageSetting Instance;

        private static string _path;

        // ===============================
        // 构建 / 加载 / 保存（以第二份为基础）
        // ===============================
        public static void Build(string settingPath)
        {
            _path = Path.Combine(settingPath, $"{nameof(BlackMageSetting)}.json");

            if (!File.Exists(_path))
            {
                Instance = new BlackMageSetting();
                Instance.Save();
                return;
            }

            try
            {
                Instance = JsonHelper.FromJson<BlackMageSetting>(File.ReadAllText(_path));
            }
            catch (Exception e)
            {
                Instance = new BlackMageSetting();
                LogHelper.Error(e.ToString());
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonHelper.ToJson(this));
        }

        // ======================================
        // General Settings（原第二份的基础设置）
        // ======================================
        public int AnimLock = 550;
        public bool ForceCast = false;
        public bool ForceNextSlotsOnHKs = false;

        public bool NoPosDrawInTN = false;
        public int PosDrawStyle = 2;

        public bool RestoreQtSet = true;
        public bool AutoSetCasual = true;
        public bool IsHardCoreMode = false;

        public bool CommandWindowOpen = true;
        public bool ShowToast = false;

        /// <summary>统一调试开关（旧字段“调试窗口”会映射到这个）</summary>
        public bool Debug = false;

        /// <summary>时间轴调试（旧字段 TimeLinesDebug 会映射到这个）</summary>
        public bool TimelineDebug = false;

        // ======================================
        // ⬇ 从第一份迁移过来的配置字段
        // ======================================

        // —— UI / 调试相关 ——
        /// <summary>旧版的“调试窗口”，内部直接映射到 Debug，避免旧代码爆红</summary>
        public bool 调试窗口
        {
            get => Debug;
            set => Debug = value;
        }

        public bool 锁定以太步窗口 = false;
        public int 以太步IconSize = 47;
        public bool 以太步窗口显示 = true;

        /// <summary>旧版的 TimeLinesDebug，映射到 TimelineDebug</summary>
        public bool TimeLinesDebug
        {
            get => TimelineDebug;
            set => TimelineDebug = value;
        }

        // —— 战斗逻辑相关（从第一份搬过来） ——
        public double 起手预读时间 = 3.5;
        public bool 核爆起手 = false;
        public bool 标准57 = false;
        public bool 开挂循环 = false;

        public bool Autotarget = false;
        public int AutoTargetMode = 0;
        public bool 提前黑魔纹 = false;
        public int TTK阈值 = 12000;

        // —— 第二份自带的“黑魔职业自动逻辑开关”（保留） ——
        public bool 自动墨泉 = true;
        public bool 自动黑魔纹 = true;
        public bool 自动瞬发 = true;
        public bool 自动悖论 = true;
        public bool 自动雷 = true;
        public bool 自动通晓 = true;

        // ======================================
        // QT 存档：采用第二份的 HardCore / Casual 双字典结构
        // ======================================
        public Dictionary<string, bool> QtStatesHardCore;
        public Dictionary<string, bool> QtStatesCasual;

        /// <summary>
        /// 初始化时给两套 QT 字典赋默认值
        /// </summary>
        private BlackMageSetting()
        {
            ResetQtStates(true);
            ResetQtStates(false);
        }

        /// <summary>
        /// 初始化 / 重置 QT 状态（完全使用第二份那套中文 Key）
        /// </summary>
        public void ResetQtStates(bool isHardCoreMode)
        {
            if (isHardCoreMode)
            {
                QtStatesHardCore = new Dictionary<string, bool>
                {
                    ["通晓"] = true,
                    ["爆发药"] = false,
                    ["黑魔纹"] = true,
                    ["墨泉"] = true,
                    ["Dot"] = true,
                    ["智能AOE"] = false,
                    ["AOE"] = false,
                    ["倾泻资源"] = false,
                    ["Boss上天"] = true,
                    ["TTK"] = false,
                    ["起手不三连"] = false
                };
            }
            else
            {
                QtStatesCasual = new Dictionary<string, bool>
                {
                    ["通晓系技能"] = true,
                    ["爆发药"] = false,
                    ["黑魔纹"] = true,
                    ["墨泉"] = true,
                    ["Dot"] = true,
                    ["智能AOE"] = true,
                    ["AOE"] = true,
                    ["倾泻资源"] = false,
                    ["Boss上天"] = false,
                    ["TTK"] = false,
                    ["起手不三连"] = false
                };
            }
        }

        // ======================================
        // UI 皮肤配置（JobViewSave，采用第二份 + 第一份的需求）
        // ======================================
        public JobViewSave JobViewSave { get; set; } = new()
        {
            // 来自第一份：保留自动重置 & 显示 QT / Hotkey
            ShowHotkey = true,

            // 来自第二份：使用黑魔主题 + 行数
            CurrentTheme = ModernTheme.ThemePreset.BLM,
            QtLineCount = 2,

            // 可以在这里预先隐藏某些 QT（可留空）
            QtUnVisibleList =
            [
                // "倾泻资源",
                // "起手不三连"
            ],
        };
    }
}