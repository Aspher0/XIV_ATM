using Dalamud.Interface.Colors;
using ECommons.Automation.LegacyTaskManager;
using Dalamud.Bindings.ImGui;
using System.Collections.Generic;
using System.Numerics;
using XIVATM.Helpers;

namespace XIVATM.UI.Settings;

public static class MoodlesIntegrationTab
{
    public static void DrawMoodlesIntegrationTab()
    {
        if (ImGui.BeginChild("Settings_UI##MoodlesIntegrationTab", new(-1f, -1f), true))
        {
            if (IPCHelper.IsMoodlesAPIAvailable())
            {
                bool applyMoodlesOnATMMode = Service.Configuration!.ApplyMoodlesOnATMModeEnabled;

                if (ImGui.Checkbox($"Apply moodles when ATM mode is enabled", ref applyMoodlesOnATMMode))
                {
                    Service.Configuration.UpdateConfiguration(() => { Service.Configuration.ApplyMoodlesOnATMModeEnabled = applyMoodlesOnATMMode; });

                    if (Service.Configuration.PluginEnabled)
                    {
                        if (Service.Configuration.ApplyMoodlesOnATMModeEnabled)
                            IPCHelper.CheckShouldApplyMoodles();
                        else
                            IPCHelper.RemoveAllAppliedMoodles();
                    }
                }

                // Calculate remaining height
                float remainingHeight = ImGui.GetContentRegionAvail().Y - 50f;

                List<MoodlesMoodleInfo> allMoodles = Service.MoodlesIPC_Caller.GetMoodles();

                UIHelper.TextWrappedColored(ImGuiColors.DalamudViolet, "Singular Moodles to apply when ATM mode is enabled :");

                if (ImGui.BeginChild("Settings_UI##MoodlesIntegrationTab##SingularMoodles", new(-1f, remainingHeight / 2), true))
                {
                    foreach (var moodle in allMoodles)
                    {
                        bool selected = Service.Configuration.MoodlesOnATMModeEnabled.Contains(moodle);

                        // Green if needs to be applied, grey if not
                        Vector4 headerColor = selected
                            ? new Vector4(0f, 1f, 0.0f, 0.2f)
                            : new Vector4(1f, 1f, 1f, 0.1f);

                        ImGui.PushStyleColor(ImGuiCol.Header, headerColor);

                        if (ImGui.Selectable($"{(selected ? "[Selected] " : "")}{moodle.FullPath}", true))
                        {
                            Service.Configuration.UpdateConfiguration(() => {
                                if (selected)
                                {
                                    Service.Configuration.MoodlesOnATMModeEnabled.Remove(moodle);

                                    if (Service.Configuration.PluginEnabled && Service.Configuration.ApplyMoodlesOnATMModeEnabled)
                                        Service.MoodlesIPC_Caller.RemoveMoodle(moodle.ID);
                                }
                                else
                                {
                                    Service.Configuration.MoodlesOnATMModeEnabled.Add(moodle);

                                    if (Service.Configuration.PluginEnabled && Service.Configuration.ApplyMoodlesOnATMModeEnabled)
                                        Service.MoodlesIPC_Caller.ApplyMoodle(moodle.ID);
                                }
                            });

                            if (Service.Configuration.PluginEnabled && Service.Configuration.ApplyMoodlesOnATMModeEnabled)
                            {
                                TaskManager taskManager = new();
                                taskManager.EnqueueImmediate(() => { IPCHelper.CheckShouldApplyMoodles(); });
                            }
                        }

                        ImGui.PopStyleColor();
                    }
                }

                ImGui.EndChild();


                List<MoodlesPresetInfo> allMoodlesPresets = Service.MoodlesIPC_Caller.GetPresets();

                UIHelper.TextWrappedColored(ImGuiColors.DalamudViolet, "Moodles Presets to apply when ATM mode is enabled :");

                if (ImGui.BeginChild("Settings_UI##MoodlesIntegrationTab##MoodlesPresets", new(-1f, remainingHeight / 2), true))
                {
                    foreach (var moodlePreset in allMoodlesPresets)
                    {
                        bool selected = Service.Configuration.MoodlesPresetsOnATMModeEnabled.Contains(moodlePreset);
                        // Green if needs to be applied, grey if not
                        Vector4 headerColor = selected
                            ? new Vector4(0f, 1f, 0.0f, 0.2f)
                            : new Vector4(1f, 1f, 1f, 0.1f);

                        ImGui.PushStyleColor(ImGuiCol.Header, headerColor);

                        if (ImGui.Selectable($"{(selected ? "[Selected] " : "")}{moodlePreset.FullPath}", true))
                        {
                            Service.Configuration.UpdateConfiguration(() => {
                                if (selected)
                                {
                                    Service.Configuration.MoodlesPresetsOnATMModeEnabled.Remove(moodlePreset);

                                    if (Service.Configuration.PluginEnabled && Service.Configuration.ApplyMoodlesOnATMModeEnabled)
                                        Service.MoodlesIPC_Caller.RemovePreset(moodlePreset.ID);
                                }
                                else
                                {
                                    Service.Configuration.MoodlesPresetsOnATMModeEnabled.Add(moodlePreset);

                                    if (Service.Configuration.PluginEnabled && Service.Configuration.ApplyMoodlesOnATMModeEnabled)
                                        Service.MoodlesIPC_Caller.ApplyPreset(moodlePreset.ID);
                                }
                            });

                            if (Service.Configuration.PluginEnabled && Service.Configuration.ApplyMoodlesOnATMModeEnabled)
                            {
                                TaskManager taskManager = new();
                                taskManager.EnqueueImmediate(() => { IPCHelper.CheckShouldApplyMoodles(); });
                            }
                        }

                        ImGui.PopStyleColor();
                    }
                }

                ImGui.EndChild();
            }
            else
            {
                // Moodles is not available, display a message
                UIHelper.TextWrappedColored(ImGuiColors.DalamudRed, "Moodles is not available. Please install the latest version of Moodles.");
            }
        }

        ImGui.EndChild();
    }
}
