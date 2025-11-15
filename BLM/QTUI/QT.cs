using AEAssist.Helper;
using ElliotZ;
using ElliotZ.ModernJobViewFramework;
using Los;

namespace los.BLM.QtUI
{
    public static class Qt
    {
        public static JobViewWindow Instance { get; private set; }

        private static Dictionary<string, bool> _currQtStatesDict =
            BlackMageSetting.Instance.QtStatesCasual;

        // ===========================
        // ⭐ BlackMage 全新纯字符串 Key 体系
        // ===========================
        private static readonly List<QtInfo> _qtKeys =
        [
            new("通晓", "Polyglot", true, null, ""),
            new("爆发药", "Potion", false, null, ""),
            new("黑魔纹", "LeyLines", true, null, ""),
            new("墨泉", "Sharpcast", true, null, ""),
            new("Dot", "Dot", true, null, ""),
            new("智能AOE", "SmartAOE", false, null, ""),
            new("AOE", "AOE", true, null, "开关所有 AOE"),
            new("倾泻资源", "Dump", false, null, "清空通晓"),
            new("Boss上天", "BossFly", false, null, "Boss 上天逻辑"),
            new("TTK", "TTK", false, null, ""),
            new("起手不三连", "NoTriple", false, null, "只对普通循环有效"),
            new("双星灵魔泉", "DoubleSharpcast", true, null, ""),
        ];

        // 暂不使用热键系统
        private static readonly List<HotKeyInfo> _hkResolvers = [];

        public static void Build()
        {
            Instance = new JobViewWindow(
                BlackMageSetting.Instance.JobViewSave,
                BlackMageSetting.Instance.Save,
                "BlackMage"
            );

            Instance.SetUpdateAction(OnUIUpdate);

            new MacroManager(Instance, "/BLM", _qtKeys, _hkResolvers, true)
                .BuildCommandList();
            SettingTab.Build(Instance);

            LoadQtStates();
        }

        private static void OnUIUpdate()
        {
            _currQtStatesDict = BlackMageSetting.Instance.IsHardCoreMode
                ? BlackMageSetting.Instance.QtStatesHardCore
                : BlackMageSetting.Instance.QtStatesCasual;
        }

        public static void SaveQtStates()
        {
            foreach (string key in Instance.GetQtArray())
                _currQtStatesDict[key] = Instance.GetQt(key);

            BlackMageSetting.Instance.Save();
            LogHelper.Print("BlackMage QT 已保存");
        }

        public static void LoadQtStates()
        {
            foreach (var kv in _currQtStatesDict)
                Instance.SetQt(kv.Key, kv.Value);

            if (BlackMageSetting.Instance.Debug)
                LogHelper.Print("BlackMage QT 已加载");
        }
    }

    /// <summary>
    /// 兼容旧 BlackMageQT 写法，但内部使用新字符串 Key
    /// </summary>
    public static class BlackMageQT
    {
        public static bool GetQt(string name)
            => Qt.Instance.GetQt(name);

        public static void SetQt(string name, bool value)
            => Qt.Instance.SetQt(name, value);

        public static bool ReverseQt(string name)
        {
            bool v = Qt.Instance.GetQt(name);
            Qt.Instance.SetQt(name, !v);
            return !v;
        }

        public static string[] GetQtArray()
            => Qt.Instance.GetQtArray();

        public static void Reset()
        {
            Qt.Instance.SetQt("LeyLines", true);
            Qt.Instance.SetQt("Sharpcast", true);
            Qt.Instance.SetQt("Dot", true);
            Qt.Instance.SetQt("AOE", true);
            Qt.Instance.SetQt("Polyglot", true);
            Qt.Instance.SetQt("SmartAOE", false);
            Qt.Instance.SetQt("Dump", false);
            Qt.Instance.SetQt("BossFly", true);
            Qt.Instance.SetQt("TTK", false);
            Qt.Instance.SetQt("NoTriple", false);
            Qt.Instance.SetQt("DoubleSharpcast", false);
        }

        // 默认功能你未来可以补，这里留空
        public static void NewDefault(string name, bool value)
        {
        }

        public static void SetDefaultFromNow()
        {
        }
    }
}