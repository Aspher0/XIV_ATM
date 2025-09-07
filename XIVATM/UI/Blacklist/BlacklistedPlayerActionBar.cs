using Dalamud.Bindings.ImGui;
using XIVATM.Helpers;
using XIVATM.Structs;

namespace XIVATM.UI.Blacklist;

public static class BlacklistedPlayerActionBar
{
    public async static void DrawBlacklistedPlayerActionBar()
    {
        (bool isTargetValid, string text, Player? outTargetedPlayer) targetData = await PlayerHelper.IsTargetValid();

        if (!targetData.isTargetValid)
        {
            ImGui.BeginDisabled();
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
            ImGui.Button(targetData.text);
            ImGui.PopStyleVar();
            ImGui.EndDisabled();
        }
        else
        {
            var buttonText = targetData.text;
            bool flagIsTargetAltOfFoundBlacklistedPlayer = false;
            BlacklistedPlayer? foundBlacklistedPlayerInConfiguration = null;

            if (targetData.isTargetValid)
            {
                foundBlacklistedPlayerInConfiguration = PlayerHelper.TryGetBlacklistedPlayerFromConfiguration(targetData.outTargetedPlayer!);

                if (foundBlacklistedPlayerInConfiguration != null)
                {
                    flagIsTargetAltOfFoundBlacklistedPlayer = PlayerHelper.IsPlayerAltOfBlacklistedPlayer(targetData.outTargetedPlayer!, foundBlacklistedPlayerInConfiguration);
                    buttonText = $"Edit {targetData.outTargetedPlayer!.PlayerName}@{targetData.outTargetedPlayer!.HomeWorld}{(flagIsTargetAltOfFoundBlacklistedPlayer ? " (Alt Account)" : "")}";
                }
            }

            if (ImGui.Button(buttonText))
            {
                if (foundBlacklistedPlayerInConfiguration != null)
                {
                    BlacklistedPlayerSelector.SelectedBlacklistedPlayer = foundBlacklistedPlayerInConfiguration;
                    BlacklistedPlayerSelector.ViewModeBlacklistedPlayerSelector = "edit";
                }
                else
                {
                    CommonHelper.AddToHistory($"The player {targetData.outTargetedPlayer!.GetPlayerNameWorld()} has been added to the blacklist manually.");

                    Service.Configuration!.UpdateConfiguration(() =>
                    {
                        ListedPlayer listedPlayer = new ListedPlayer(targetData.outTargetedPlayer!, ListedPlayerType.AddedFromTarget);
                        BlacklistedPlayer blacklistedPlayer = new BlacklistedPlayer(listedPlayer);
                        Service.Configuration.BlacklistedPlayers.Add(blacklistedPlayer);
                        BlacklistedPlayerSelector.SelectedBlacklistedPlayer = blacklistedPlayer;
                        BlacklistedPlayerSelector.ViewModeBlacklistedPlayerSelector = "edit";
                    });
                }
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Add a player manually"))
        {
            CommonHelper.AddToHistory("A player has been added to the blacklist manually.");

            Service.Configuration!.UpdateConfiguration(() =>
            {
                Player newPlayer = new Player("", "");
                ListedPlayer listedPlayer = new ListedPlayer(newPlayer, ListedPlayerType.ManuallyAdded, ListedType.Character);
                BlacklistedPlayer blacklistedPlayer = new BlacklistedPlayer(listedPlayer);
                Service.Configuration.BlacklistedPlayers.Add(blacklistedPlayer);
                BlacklistedPlayerSelector.SelectedBlacklistedPlayer = blacklistedPlayer;
                BlacklistedPlayerSelector.ViewModeBlacklistedPlayerSelector = "edit";
            });
        }

        if (BlacklistedPlayerSelector.SelectedBlacklistedPlayer != null)
        {
            ImGui.SameLine();

            bool ctrlPressed = ImGui.GetIO().KeyCtrl;

            if (ctrlPressed)
            {
                if (ImGui.Button("Delete"))
                {
                    CommonHelper.AddToHistory($"The player {BlacklistedPlayerSelector.SelectedBlacklistedPlayer.ListedPlayer.Player.GetPlayerNameWorld()} has been removed from the blacklist.");

                    Service.Configuration!.UpdateConfiguration(() =>
                    {
                        Service.Configuration.BlacklistedPlayers.Remove(BlacklistedPlayerSelector.SelectedBlacklistedPlayer);
                        BlacklistedPlayerSelector.SelectedBlacklistedPlayer = null;
                        BlacklistedPlayerSelector.ViewModeBlacklistedPlayerSelector = "default";
                    });
                }
            }
            else
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
                ImGui.Button("Delete");
                ImGui.PopStyleVar();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Hold CTRL to delete.");
                ImGui.EndTooltip();
            }
        }
    }
}
