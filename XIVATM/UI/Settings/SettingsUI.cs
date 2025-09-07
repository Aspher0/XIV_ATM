using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;
using XIVATM.Handlers;
using XIVATM.Helpers;
using XIVATM.Structs;

namespace XIVATM.UI.Settings;

public class SettingsUI
{
    public static void DrawSettingsUI()
    {
        DrawGeneralSettingsCheckboxes();

        DrawGeneralSettingsDelayBetweenActions();

        ImGui.Spacing();

        if (ImGui.BeginTabBar("Settings_UI##TabBar"))
        {
            if (ImGui.BeginTabItem("Default Gil Amounts"))
            {
                DefaultGilAmountsTab.DrawGilAmountsTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Default Trigger Phrases"))
            {
                DefaultTriggerPhrasesTab.DrawDefaultTriggerPhrasesTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Honorific Integration"))
            {
                HonorificIntegrationTab.DrawHonorificIntegrationTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Moodles Integration"))
            {
                MoodlesIntegrationTab.DrawMoodlesIntegrationTab();
                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();
    }

    public static void DrawGeneralSettingsCheckboxes()
    {
        if (ImGui.BeginChild("Settings_UI##GeneralCheckboxes", new Vector2(-1f, 70f), true))
        {
            bool pluginEnabled = Service.Configuration!.PluginEnabled;

            UIHelper.StartColoringText(pluginEnabled ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed);

            if (ImGui.Checkbox($"ATM Mode {(pluginEnabled ? "Enabled" : "Disabled")}", ref pluginEnabled))
                CommandHandler.ToggleATMMode();

            UIHelper.EndColoringText();

            ImGui.SameLine();

            bool pluginEnabledPersistence = Service.Configuration.PluginEnabledPersistence;

            if (ImGui.Checkbox("Remember enabled state on load", ref pluginEnabledPersistence))
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.PluginEnabledPersistence = pluginEnabledPersistence; });

            ImGui.SameLine();

            bool openPluginOnLoad = Service.Configuration.OpenPluginOnLoad;

            if (ImGui.Checkbox("Open Plugin on Load", ref openPluginOnLoad))
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.OpenPluginOnLoad = openPluginOnLoad; });

            bool whitelistEnabled = Service.Configuration.WhitelistEnabled;

            if (ImGui.Checkbox("Whitelist Enabled", ref whitelistEnabled))
            {
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.WhitelistEnabled = whitelistEnabled; });
                CommonHelper.AddToHistory($"Whitelist {(Service.Configuration.WhitelistEnabled ? "enabled" : "disabled")}.");
            }

            ImGui.SameLine();

            bool blacklistEnabled = Service.Configuration.BlacklistEnabled;

            if (ImGui.Checkbox("Blacklist Enabled", ref blacklistEnabled))
            {
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.BlacklistEnabled = blacklistEnabled; });
                CommonHelper.AddToHistory($"Blacklist {(Service.Configuration.WhitelistEnabled ? "enabled" : "disabled")}.");
            }
        }

        ImGui.EndChild();
    }

    public static void DrawGeneralSettingsDelayBetweenActions()
    {
        if (ImGui.BeginChild("Settings_UI##DelayBetweenActions", new Vector2(-1f, 110f), true))
        {
            ImGui.TextColored(ImGuiColors.DalamudViolet, "Delay between actions (milliseconds) :");

            if (ImGui.RadioButton("Fixed Delay", Service.Configuration!.DelayBetweenActions.DelayBetweenActionsMode == DelayBetweenActionsMode.Fixed))
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DelayBetweenActions.DelayBetweenActionsMode = DelayBetweenActionsMode.Fixed; });

            ImGui.SameLine();

            if (ImGui.RadioButton("Random Delay", Service.Configuration.DelayBetweenActions.DelayBetweenActionsMode == DelayBetweenActionsMode.RandomRange))
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DelayBetweenActions.DelayBetweenActionsMode = DelayBetweenActionsMode.RandomRange; });

            switch (Service.Configuration.DelayBetweenActions.DelayBetweenActionsMode)
            {
                case DelayBetweenActionsMode.Fixed:
                    {
                        int fixedValueMilliseconds = Service.Configuration.DelayBetweenActions.FixedValueMilliseconds;

                        ImGui.SetNextItemWidth(200f);

                        if (ImGui.InputInt("##delayBetweenActionsFixedValueMilliseconds", ref fixedValueMilliseconds, 100))
                        {
                            if (fixedValueMilliseconds < 0)
                                fixedValueMilliseconds = 0;

                            Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DelayBetweenActions.FixedValueMilliseconds = fixedValueMilliseconds; });
                        }

                        UIHelper.TextWrappedColored(ImGuiColors.DalamudOrange, $"A fixed delay of {Math.Round((float)fixedValueMilliseconds / 1000, 1)}s will be applied between actions.");

                        break;
                    }
                case DelayBetweenActionsMode.RandomRange:
                    {
                        int rangeLowValueMilliseconds = Service.Configuration.DelayBetweenActions.RangeLowValueMilliseconds;

                        ImGui.SetNextItemWidth(200f);

                        if (ImGui.InputInt("##delayBetweenActionsRangeLowValueMilliseconds", ref rangeLowValueMilliseconds, 100))
                        {
                            if (rangeLowValueMilliseconds < 0)
                                rangeLowValueMilliseconds = 0;

                            Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DelayBetweenActions.RangeLowValueMilliseconds = rangeLowValueMilliseconds; });
                        }

                        ImGui.SameLine();

                        int rangeHighValueMilliseconds = Service.Configuration.DelayBetweenActions.RangeHighValueMilliseconds;

                        ImGui.SetNextItemWidth(200f);

                        if (ImGui.InputInt("##delayBetweenActionsRangeHighValueMilliseconds", ref rangeHighValueMilliseconds, 100))
                        {
                            if (rangeHighValueMilliseconds < 0)
                                rangeHighValueMilliseconds = 0;

                            Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DelayBetweenActions.RangeHighValueMilliseconds = rangeHighValueMilliseconds; });
                        }

                        if (rangeHighValueMilliseconds < rangeLowValueMilliseconds)
                        {
                            rangeHighValueMilliseconds = rangeLowValueMilliseconds;

                            Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DelayBetweenActions.RangeHighValueMilliseconds = rangeHighValueMilliseconds; });
                        }

                        UIHelper.TextWrappedColored(ImGuiColors.DalamudOrange, $"A random delay between {Math.Round((float)rangeLowValueMilliseconds / 1000, 1)}s and {Math.Round((float)rangeHighValueMilliseconds / 1000, 1)}s will be applied between actions.");

                        break;
                    }
            }
        }

        ImGui.EndChild();
    }
}
