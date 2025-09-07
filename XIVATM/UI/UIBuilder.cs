using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ECommons;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;
using XIVATM.Handlers;
using XIVATM.Helpers;
using XIVATM.Structs;
using XIVATM.UI.Blacklist;
using XIVATM.UI.DebugTesting;
using XIVATM.UI.History;
using XIVATM.UI.Settings;

namespace XIVATM.UI;

public class UIBuilder : Window, IDisposable
{
    public UIBuilder(XIVATM_Plugin plugin)
        : base("XIV ATM##XIVATMMain", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 730),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        CheckDrawWarnings();

        if (ImGui.BeginTabBar("MainWindowTabs"))
        {
            if (ImGui.BeginTabItem("Settings"))
            {
                SettingsUI.DrawSettingsUI();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Whitelist"))
            {
                // WhitelistUI.DrawWhitelistUI();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Blacklist"))
            {
                BlacklistUI.DrawBlacklistUI();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Statistics"))
            {
                // StatisticsUI.DrawStatisticsUI();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("History"))
            {
                HistoryUI.DrawHistoryUI();
                ImGui.EndTabItem();
            }

#if DEBUG
            if (ImGui.BeginTabItem("Debug & Testing"))
            {
                DebugTestingUI.DrawDebugTestingUI();
                ImGui.EndTabItem();
            }
#endif

            ImGui.EndTabBar();
        }
    }

    public void CheckDrawWarnings()
    {
        bool flagHasErrors = false;
        List<string> errors = new List<string>();

        ChecksBeforeTrade canSendGils = CommandHandler.CheckCanSendGils();

        if (!canSendGils.HasTriggerPhrases || !canSendGils.HasOneValidTriggerPhrase)
        {
            flagHasErrors = true;
            errors.Add($"- {ErrorMessages.NoValidDefaultTriggerPhrases}.");
        }

        if (!canSendGils.IsValidGilAmountToSendFixed || !canSendGils.IsValidGilAmountToSendRandomRangeBoth)
        {
            flagHasErrors = true;
            errors.Add($"- {ErrorMessages.InvalidGilAmountToSendFixedOrBothRangeInvalid}.");
        }

        if (!canSendGils.IsValidGilAmountToSendRandomRangeLow)
        {
            flagHasErrors = true;
            errors.Add($"- {ErrorMessages.InvalidGilAmountToSendRangeLowInvalid}.");
        }

        if (!canSendGils.IsValidDefaultGilAmountPlayerTimerange)
        {
            flagHasErrors = true;
            errors.Add($"- {ErrorMessages.InvalidDefaultGilAmountPlayerTimerange}.");
        }

        if (!canSendGils.IsValidGlobalGilAmountTimerange)
        {
            flagHasErrors = true;
            errors.Add($"- {ErrorMessages.InvalidGlobalGilAmountTimerange}.");
        }

        if (!canSendGils.IsValidGlobalGilAmountWithdrawable)
        {
            flagHasErrors = true;
            errors.Add($"- {ErrorMessages.InvalidGlobalGilAmountWithdrawable}.");
        }

        if (canSendGils.HasReachedTheGlobalLimitOfGils)
        {
            flagHasErrors = true;
            errors.Add($"- {ErrorMessages.HasReachedGlobalGilCap}.");
        }

        if (canSendGils.HasReachedTheGlobalLimitOfGilsPerTimerange)
        {
            flagHasErrors = true;
            errors.Add($"- {ErrorMessages.HasReachedGlobalGilCapPerTimerange}.");
        }

        if (flagHasErrors)
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, $"{errors.Count} error{(errors.Count > 1  ? "s were" : " was")} found, please solve them for the plugin to work :");
            UIHelper.StartColoringText(ImGuiColors.DalamudRed);
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(errors.Join("\n"));
            UIHelper.EndColoringText();
        } else
        {
            ImGui.TextColored(ImGuiColors.HealerGreen, "No errors were found, the plugin is ready to be used.");
        }

        ImGui.Spacing();
    }

    public void Dispose() { }
}
