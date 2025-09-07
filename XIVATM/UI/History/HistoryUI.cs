using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;
using System.Linq;
using System.Numerics;

namespace XIVATM.UI.History;

public static class HistoryUI
{
    public static void DrawHistoryUI()
    {
        ImGui.AlignTextToFramePadding();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "History Logs (latest shows on top) :");

        ImGui.SameLine();

        bool ctrlKeyPressed = ImGui.GetIO().KeyCtrl;

        if (ctrlKeyPressed)
        {
            if (ImGui.SmallButton("Clear"))
                Service.HistoryEntriesList.Clear();
        } else
        {
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
            ImGui.SmallButton("Clear");
            ImGui.PopStyleVar();
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Hold CTRL and click to clear the history logs.");

        if (ImGui.BeginChild("History_UI##HistoryLogs", new Vector2(-1,-1), true))
        {
            var historyEntriesSortedByDate = Service.HistoryEntriesList.OrderByDescending(x => x.Timestamp).ToList();

            foreach (var historyEntry in historyEntriesSortedByDate)
            {
                ImGui.TextWrapped($"[{historyEntry.Timestamp.ToString("HH:mm:ss")}] - {historyEntry.Entry}");
            }
        }

        ImGui.EndChild();
    }
}
