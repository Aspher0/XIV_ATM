using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using XIVATM.Helpers;
using XIVATM.IPC.Honorific;

namespace XIVATM.UI.Settings;

public static class HonorificIntegrationTab
{
    public static void DrawHonorificIntegrationTab()
    {
        if (ImGui.BeginChild("Settings_UI##HonorificIntegrationTab", new(-1f, -1f), true))
        {
            if (IPCHelper.IsHonorificAPIAvailable())
            {
                bool applyHonorificTitleOnATMMode = Service.Configuration!.ApplyHonorificTitleOnATMModeEnabled;

                if (ImGui.Checkbox($"Apply title when ATM mode is enabled", ref applyHonorificTitleOnATMMode))
                {
                    Service.Configuration.UpdateConfiguration(() => { Service.Configuration.ApplyHonorificTitleOnATMModeEnabled = applyHonorificTitleOnATMMode; });

                    if (Service.ConnectedPlayer != null)
                        Service.HonorificIPC_Caller.SetTitle(Service.Configuration.PluginEnabled && applyHonorificTitleOnATMMode ? Service.Configuration!.HonorificTitleOnATMModeEnabled!.Title : null);
                }

                ImGui.SameLine();

                bool applyHonorificTitleOnTransactionOngoing = Service.Configuration.ApplyHonorificTitleOnTransactionOngoing;

                if (ImGui.Checkbox($"Apply title when transaction is ongoing", ref applyHonorificTitleOnTransactionOngoing))
                    Service.Configuration.UpdateConfiguration(() => { Service.Configuration.ApplyHonorificTitleOnTransactionOngoing = applyHonorificTitleOnTransactionOngoing; });

                ImGui.Spacing();

                // A checkbox to enable or disable the display of the honorific title from other characters
                bool showHonorificTitlesFromAllCharacters = Service.Configuration.ShowHonorificTitlesFromAllCharacters;

                if (ImGui.Checkbox($"Show honorific titles from all characters", ref showHonorificTitlesFromAllCharacters))
                    Service.Configuration.UpdateConfiguration(() => { Service.Configuration.ShowHonorificTitlesFromAllCharacters = showHonorificTitlesFromAllCharacters; });

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("When disabled, you will only be able to see titles assigned to your current character.");
                    ImGui.Text("If enabled, honorific titles from all characters will be displayed in the list below.");
                    ImGui.TextColored(ImGuiColors.DalamudOrange, "Warning: Only the titles of YOUR characters will be displayed.");
                    ImGui.TextColored(ImGuiColors.DalamudOrange, "Furthermore, you will need to login at least once on each character while this plugin is enabled.");
                    ImGui.EndTooltip();
                }

                ImGui.Spacing();

                // Calculate remaining height
                float remainingHeight = ImGui.GetContentRegionAvail().Y - 50f;

                List<TitleData> allTitles = [];

                foreach (var CID in Service.Configuration.SeenCharacters.Keys)
                {
                    if ((!showHonorificTitlesFromAllCharacters && CID == ECommons.GameHelpers.Player.CID) || showHonorificTitlesFromAllCharacters)
                        allTitles.AddRange(Service.HonorificIPC_Caller.GetTitleData([CID]).OrderBy(x => x.Title));
                }

                // If the currently selected title to apply is not in the list variable, add it to it (To display it even if not logged in)
                if (Service.Configuration.HonorificTitleOnATMModeEnabled != null && allTitles.Find(title => Service.Configuration.HonorificTitleOnATMModeEnabled.Equals(title)) == null)
                    allTitles.Add(Service.Configuration.HonorificTitleOnATMModeEnabled);

                UIHelper.TextWrappedColored(ImGuiColors.DalamudViolet, "Title to apply when ATM mode is enabled :");

                if (ImGui.BeginChild("Settings_UI##HonorificIntegrationTab##HonorificTitles", new(-1f, remainingHeight / 2), true))
                {
                    Vector4 headerColor = Service.Configuration.HonorificTitleOnATMModeEnabled == null
                        ? new Vector4(0f, 1f, 0.0f, 0.2f)
                        : new Vector4(1f, 1f, 1f, 0.1f);

                    ImGui.PushStyleColor(ImGuiCol.Header, headerColor);

                    if (ImGui.Selectable("None", true))
                    {
                        Service.Configuration.UpdateConfiguration(() => { Service.Configuration.HonorificTitleOnATMModeEnabled = null; });
                        IPCHelper.CheckShouldApplyHonorificTitle();
                    }

                    ImGui.PopStyleColor();

                    foreach (var title in allTitles)
                    {
                        headerColor = Service.Configuration.HonorificTitleOnATMModeEnabled?.Equals(title) ?? false
                            ? new Vector4(0f, 1f, 0.0f, 0.2f)
                            : new Vector4(1f, 1f, 1f, 0.1f);

                        ImGui.PushStyleColor(ImGuiCol.Header, headerColor);

                        if (ImGui.Selectable(title.Title, true))
                        {
                            Service.Configuration.UpdateConfiguration(() => { Service.Configuration.HonorificTitleOnATMModeEnabled = title; });
                            IPCHelper.CheckShouldApplyHonorificTitle();
                        }

                        ImGui.PopStyleColor();
                    }
                }

                ImGui.EndChild();

                // Title to apply when a transaction is ongoing
                UIHelper.TextWrappedColored(ImGuiColors.DalamudViolet, "Title to apply when a transaction is ongoing :");

                if (ImGui.BeginChild("Settings_UI##HonorificIntegrationTab##HonorificTitlesTransaction", new(-1f, remainingHeight / 2), true))
                {
                    Vector4 headerColor = Service.Configuration.HonorificTitleOnTransactionOngoing == null
                        ? new Vector4(0f, 1f, 0.0f, 0.2f)
                        : new Vector4(1f, 1f, 1f, 0.1f);

                    ImGui.PushStyleColor(ImGuiCol.Header, headerColor);

                    if (ImGui.Selectable("None", true))
                        Service.Configuration.UpdateConfiguration(() => { Service.Configuration.HonorificTitleOnTransactionOngoing = null; });

                    if (ImGui.IsItemHovered())
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

                    ImGui.PopStyleColor();

                    foreach (var title in allTitles)
                    {
                        headerColor = Service.Configuration.HonorificTitleOnTransactionOngoing?.Equals(title) ?? false
                            ? new Vector4(0f, 1f, 0.0f, 0.2f)
                            : new Vector4(1f, 1f, 1f, 0.1f);

                        ImGui.PushStyleColor(ImGuiCol.Header, headerColor);

                        if (ImGui.Selectable(title.Title, true))
                            Service.Configuration.UpdateConfiguration(() => { Service.Configuration.HonorificTitleOnTransactionOngoing = title; });

                        if (ImGui.IsItemHovered())
                            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

                        ImGui.PopStyleColor();
                    }
                }

                ImGui.EndChild();
            }
            else
            {
                // Honorific is not available, display a message
                UIHelper.TextWrappedColored(ImGuiColors.DalamudRed, "Honorific is not available. Please install the latest version of Honorific.");
            }
        }

        ImGui.EndChild();
    }
}
