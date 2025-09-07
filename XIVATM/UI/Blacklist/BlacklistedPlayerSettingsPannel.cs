using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Colors;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Dalamud.Bindings.ImGui;
using System.Linq;
using System.Numerics;
using XIVATM.Helpers;
using XIVATM.Structs;

namespace XIVATM.UI.Blacklist;

public static class BlacklistedPlayerSettingsPannel
{
    public static void DrawBlacklistedPlayerSettingsPanel()
    {
        if (ImGui.BeginChild("BlacklistUI##BlacklistedPlayerSettingsPanel", new Vector2(0.0f, -ImGui.GetFrameHeightWithSpacing()), true))
        {
            if (BlacklistedPlayerSelector.ViewModeBlacklistedPlayerSelector == "default")
            {
                ImGui.TextWrapped("Select a player or press the button in the bottom left corner to add one.");
            }
            else if (BlacklistedPlayerSelector.ViewModeBlacklistedPlayerSelector == "edit" && BlacklistedPlayerSelector.SelectedBlacklistedPlayer != null)
            {
                DrawPlayerInfos(BlacklistedPlayerSelector.SelectedBlacklistedPlayer);
            }
        }

        ImGui.EndChild();
    }

    public static void DrawPlayerInfos(BlacklistedPlayer SelectedBlacklistedPlayer)
    {
        ListedPlayerType listedPlayerType = SelectedBlacklistedPlayer.ListedPlayer.ListedPlayerType;

        ImGui.Text("Add type :");

        ImGui.SameLine();

        ImGui.TextColored(ImGuiColors.DalamudOrange, listedPlayerType.ToString());

        if (listedPlayerType == ListedPlayerType.ManuallyAdded)
        {
            UIHelper.TextWrappedColored(ImGuiColors.DalamudRed, "This character has been manually added. You won't be able to blacklist their alts unless you click on the \"Try Conversion\" button below.");
        }

        ImGui.Spacing();
        ImGui.Spacing();

        string playerName = SelectedBlacklistedPlayer.ListedPlayer.Player.PlayerName;
        string playerHomeWorld = SelectedBlacklistedPlayer.ListedPlayer.Player.HomeWorld;

        ImGui.Text("Player name :");

        ImGui.SameLine();

        if (SelectedBlacklistedPlayer.ListedPlayer.ListedPlayerType == ListedPlayerType.AddedFromTarget)
            ImGui.BeginDisabled();

        float remainingWidth = ImGui.GetContentRegionAvail().X;
        ImGui.SetNextItemWidth(remainingWidth);
        if (ImGui.InputTextWithHint("##characterName", "Enter the player name here ...", ref playerName, 30))
            Service.Configuration!.UpdateConfiguration(() => { SelectedBlacklistedPlayer.ListedPlayer.Player.PlayerName = playerName; });

        if (SelectedBlacklistedPlayer.ListedPlayer.ListedPlayerType == ListedPlayerType.AddedFromTarget)
            ImGui.EndDisabled();

        ImGui.Text("Player home world :");

        ImGui.SameLine();

        if (SelectedBlacklistedPlayer.ListedPlayer.ListedPlayerType == ListedPlayerType.AddedFromTarget)
            ImGui.BeginDisabled();

        remainingWidth = ImGui.GetContentRegionAvail().X;
        ImGui.SetNextItemWidth(remainingWidth);
        if (ImGui.InputTextWithHint("##characterHomeworld", "Enter the player home world here ...", ref playerHomeWorld, 30))
            Service.Configuration!.UpdateConfiguration(() => { SelectedBlacklistedPlayer.ListedPlayer.Player.HomeWorld = playerHomeWorld; });

        if (SelectedBlacklistedPlayer.ListedPlayer.ListedPlayerType == ListedPlayerType.AddedFromTarget)
            ImGui.EndDisabled();

        ImGui.Spacing();

        bool playerEnabled = SelectedBlacklistedPlayer.Enabled;

        UIHelper.StartColoringText(playerEnabled ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed);

        if (ImGui.Checkbox($"This player is{(playerEnabled ? "" : " not")} blacklisted", ref playerEnabled))
        {
            SelectedBlacklistedPlayer.Enabled = playerEnabled;
            Service.Configuration!.Save();

            CommonHelper.AddToHistory($"The blacklisted status of the player {SelectedBlacklistedPlayer.ListedPlayer.Player.GetPlayerNameWorld()} has been {(SelectedBlacklistedPlayer.Enabled ? "enabled" : "disabled")}.");
        }

        UIHelper.EndColoringText();

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("If this box is unchecked, the player will not be blacklisted anymore.");
            ImGui.Text("You can enabled it again at any time.");
            ImGui.EndTooltip();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (listedPlayerType == ListedPlayerType.AddedFromTarget)
        {
            UIHelper.TextWrappedColored(ImGuiColors.DalamudViolet, "Blacklist type :");

            if (ImGui.RadioButton("Service Account", SelectedBlacklistedPlayer.ListedPlayer.ListedType == ListedType.ServiceAccount))
            {
                SelectedBlacklistedPlayer.ListedPlayer.ListedType = ListedType.ServiceAccount;
                Service.Configuration!.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("This will blacklist the character and all their alts.");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();

            if (ImGui.RadioButton("Character", SelectedBlacklistedPlayer.ListedPlayer.ListedType == ListedType.Character))
            {
                SelectedBlacklistedPlayer.ListedPlayer.ListedType = ListedType.Character;
                Service.Configuration!.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("This will blacklist this character only.");
                ImGui.EndTooltip();
            }
        }
        else if (listedPlayerType == ListedPlayerType.ManuallyAdded)
        {
            var manualBlacklistedPlayerFoundOnMap = Svc.Objects.FirstOrDefault(obj => obj is IPlayerCharacter pc && pc.Name.TextValue == playerName && pc.HomeWorld.Value.Name == playerHomeWorld) as IPlayerCharacter;

            if (manualBlacklistedPlayerFoundOnMap == null)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
                ImGui.Button("Convert type of blacklist");
                ImGui.PopStyleVar();
            }
            else
            {
                if (ImGui.Button("Convert type of blacklist"))
                {
                    CommonHelper.AddToHistory($"You successfully converted the player {SelectedBlacklistedPlayer.ListedPlayer.Player.GetPlayerNameWorld()} from manual blacklist type to a full blacklist type.");

                    Service.Configuration!.UpdateConfiguration(() =>
                    {
                        SelectedBlacklistedPlayer.ListedPlayer.ListedPlayerType = ListedPlayerType.AddedFromTarget;

                        unsafe
                        {
                            var bc = (BattleChara*)manualBlacklistedPlayerFoundOnMap.Address;

                            SelectedBlacklistedPlayer.ListedPlayer.Player.AccountId = bc->AccountId;
                            SelectedBlacklistedPlayer.ListedPlayer.Player.ContentId = bc->ContentId;
                        }
                    });
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("This button will convert this manually-added character to a character added from a GameObject.");
                ImGui.Text("This will allow you to blacklist their alts too.");

                if (manualBlacklistedPlayerFoundOnMap == null)
                    ImGui.TextColored(ImGuiColors.DalamudRed, "The player is not on your map. Cannot proceed with conversion.");

                ImGui.EndTooltip();
            }
        }
    }
}
