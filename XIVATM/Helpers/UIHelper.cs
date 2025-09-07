using Dalamud.Interface;
using ECommons.ImGuiMethods;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace XIVATM.Helpers;

public static class UIHelper
{
    public static void TextWrappedColored(Vector4 color, string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(color));
        ImGui.TextWrapped(text);
        ImGui.PopStyleColor();
    }

    public static void StartColoringText(Vector4 color)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(color));
    }

    public static void EndColoringText()
    {
        ImGui.PopStyleColor();
    }

    public static void DrawRedCross()
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.RedBright, "\uf00d");
        ImGui.PopFont();
    }

    public static void DrawGreenCheck()
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.GreenBright, FontAwesomeIcon.Check.ToIconString());
        ImGui.PopFont();
    }
}
