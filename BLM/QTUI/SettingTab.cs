using System.Numerics;
using Dalamud.Bindings.ImGui;
using ElliotZ.ModernJobViewFramework;
using los.BLM;

namespace los.BLM.QtUI;

public static class SettingTab
{
    public static void Build(JobViewWindow instance)
    {
        // 在 JobView 上挂一个“设置”页签
        instance.AddTab("设置", _ => Draw());
    }

    private static void Draw()
    {
        DrawOpenerSection();
        // 以后你还可以在这里继续加其它设置区块
    }

    /// <summary>
    /// 起手选择区域
    /// </summary>
    private static void DrawOpenerSection()
    {
        if (!ImGui.CollapsingHeader("起手设置", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        var setting = BlackMageSetting.Instance;

        // 1 = 标准5+7, 2 = 核爆, 3 = 5+7开挂
        int openerIndex = 0;
        if (setting.标准57) openerIndex = 1;
        else if (setting.核爆起手) openerIndex = 2;
        else if (setting.开挂循环) openerIndex = 3;

        string openerLabel = openerIndex switch
        {
            1 => "标准 5+7",
            2 => "核爆起手",
            3 => "5+7 开挂循环",
            _ => "请选择"
        };

        // 下拉框
        if (ImGui.BeginCombo("起手选择", openerLabel))
        {
            // 选项：标准 5+7
            if (ImGui.Selectable("标准 5+7", openerIndex == 1))
            {
                setting.标准57   = true;
                setting.核爆起手 = false;
                setting.开挂循环 = false;
                setting.Save();
            }

            // 选项：核爆起手
            if (ImGui.Selectable("核爆起手", openerIndex == 2))
            {
                setting.标准57   = false;
                setting.核爆起手 = true;
                setting.开挂循环 = false;
                setting.Save();
            }

            // 选项：5+7 开挂循环
            if (ImGui.Selectable("5+7 开挂循环", openerIndex == 3))
            {
                setting.标准57   = false;
                setting.核爆起手 = false;
                setting.开挂循环 = true;
                setting.Save();
            }

            ImGui.EndCombo();
        }

        // 一点说明文字（可要可不要）
        ImGui.TextWrapped("说明：这里只会同时开启一个起手方案，"
                        + "实际开怪时由 BlackMageACR.GetOpener 按配置选择对应起手队列。");
    }
}