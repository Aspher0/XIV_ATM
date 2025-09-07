using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;
using XIVATM.Helpers;
using XIVATM.Structs;

namespace XIVATM.UI.Blacklist;

public static class BlacklistedPlayerSelector
{
    public static BlacklistedPlayer? SelectedBlacklistedPlayer;
    public static string CurrentBlacklistedPlayerSelectorSearch = string.Empty;
    public static string ViewModeBlacklistedPlayerSelector = "default";
    public static int CurrentDraggedBlacklistedPlayerIndex = -1;

    public static void DrawBlacklistedPlayerSelector(bool displayDisabledText = true)
    {
        string? selectedBlacklistedPlayerId = SelectedBlacklistedPlayer?.UniqueId;
        List<BlacklistedPlayer> blacklistedPlayers = Service.Configuration!.BlacklistedPlayers;

        if (ImGui.BeginChild("BlacklistUI##PlayerSelector", new Vector2(225f, -ImGui.GetFrameHeightWithSpacing()), true))
        {
            float availableWidth = ImGui.GetContentRegionAvail().X;

            ImGui.SetNextItemWidth(availableWidth);
            ImGui.InputTextWithHint("##playerFilter", "Search for a player ...", ref CurrentBlacklistedPlayerSelectorSearch, 300);

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Filter by name and/or world.");
                ImGui.EndTooltip();
            }

            ImGui.Spacing();

            foreach (var player in blacklistedPlayers)
            {
                bool isEnabled = player.Enabled;

                string id = player.UniqueId;

                string displayName = (player.ListedPlayer.Player.PlayerName.Trim() == String.Empty && player.ListedPlayer.Player.HomeWorld.Trim() == String.Empty) ? 
                    id : $"{player.ListedPlayer.Player.PlayerName}@{(player.ListedPlayer.Player.HomeWorld.Trim() == String.Empty ? "MissingWorld" : player.ListedPlayer.Player.HomeWorld)}";

                string displayText = (!isEnabled && displayDisabledText ? "[Disabled] " : "") + displayName;

                bool nameMatch = CommonHelper.RegExpMatch(displayText, CurrentBlacklistedPlayerSelectorSearch);

                if (nameMatch)
                {
                    if (ImGui.Selectable($"{displayText}##{id}", id == selectedBlacklistedPlayerId))
                    {
                        SelectedBlacklistedPlayer = player;
                        ViewModeBlacklistedPlayerSelector = "edit";
                    }

                    HandleDragDrop(displayName, blacklistedPlayers.IndexOf(player));
                }
            }

            ImGui.EndChild();
        }
    }

    private static void HandleDragDrop(string DisplayName, int index)
    {
        if (ImGui.BeginDragDropSource())
        {
            CurrentDraggedBlacklistedPlayerIndex = index;
            ImGui.Text("Dragging: " + DisplayName);

            ImGui.SetDragDropPayload("DRAG_BLACKLISTPLAYER", null, 0);

            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            var acceptPayload = ImGui.AcceptDragDropPayload("DRAG_BLACKLISTPLAYER");
            bool isDropping = !acceptPayload.IsNull;

            if (isDropping)
            {
                var temp = Service.Configuration!.BlacklistedPlayers[CurrentDraggedBlacklistedPlayerIndex];
                Service.Configuration.BlacklistedPlayers.RemoveAt(CurrentDraggedBlacklistedPlayerIndex);
                Service.Configuration.BlacklistedPlayers.Insert(index, temp);
                Service.Configuration.Save();
                CurrentDraggedBlacklistedPlayerIndex = -1;
            }

            ImGui.EndDragDropTarget();
        }
    }
}
