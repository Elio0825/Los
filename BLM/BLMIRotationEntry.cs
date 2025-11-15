using AEAssist.CombatRoutine;
using ElliotZ;
using los.BLM.QtUI;
using Oblivion.BLM;
using Oblivion.BLM.SlotResolver.Special;


namespace los.BLM;

public class 黑魔acr : IRotationEntry
{
    public string AuthorName { get; set; } = "Los";

    public Rotation Build(string settingFolder)
    {
        // 完全照原版入口，但 Init 里已经换成新 UI 了
        BlackMageACR.Init(settingFolder);
        return BlackMageACR.Build();
    }

    public IRotationUI GetRotationUI()
    {
        // 原版是 baseUI.UI，这里返回你新 UI 的 JobViewWindow
        return Qt.Instance;
    }

    public void OnDrawSetting()
    {
        // 你现在没单独设置面板的话，这里可以先留空
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;

        // 清理你自己的状态
        Qt.Instance.Dispose();
        los.BLM.SlotResolver.BattleData.Reset();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}