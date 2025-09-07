using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Threading.Tasks;
using XIVATM.Structs;

namespace XIVATM.Helpers;

public static class PlayerHelper
{
    public static (ulong accountId, ulong contentId) GetPlayerIds(IPlayerCharacter playerCharacter)
    {
        unsafe
        {
            var bc = (BattleChara*)playerCharacter.Address;
            return (bc->AccountId, bc->ContentId);
        }
    }

    public static Player MakePlayerFromPlayerNameAndWorld(string playerName, string playerHomeworld) => new(playerName, playerHomeworld);

    public static Player? MakePlayerFromPlayerCharacterObject(IPlayerCharacter? playerCharacter)
    {
        if (playerCharacter == null) return null;

        string playerName = playerCharacter.Name.ToString();
        World? playerHomeworld = Service.DataManager.GetExcelSheet<World>()?.GetRow(playerCharacter.HomeWorld.RowId);

        if (playerName == null || playerHomeworld == null) return null;

        var playerIds = GetPlayerIds(playerCharacter);

        return new Player(playerName, playerHomeworld.Value.Name.ToString(), playerIds.accountId, playerIds.contentId);
    }

    public static WithdrawnGilsPerPlayerTimerange? TryGetWithdrawnGilsPerPlayerTimerangeFromConfiguration(Player player)
    {
        // LoggerHelper.DebugBuildLog($"Amount of WithdrawnGilsPerPlayerTimerange in configuration: {Service.Configuration!.WithdrawnGilsPerPlayerTimerange.Count}");

        // Try to get an existing player matched by account id
        if (player.AccountId != null)
        {
            WithdrawnGilsPerPlayerTimerange? timerangePlayerFoundByAccountId = Service.Configuration!.WithdrawnGilsPerPlayerTimerange.Find(timerangePlayer =>
            timerangePlayer.Player.AccountId == player.AccountId);

            if (timerangePlayerFoundByAccountId != null) return timerangePlayerFoundByAccountId;
        }

        // If not found, try to find with content id
        if (player.ContentId != null)
        {
            WithdrawnGilsPerPlayerTimerange? timerangePlayerFoundByContentId = Service.Configuration!.WithdrawnGilsPerPlayerTimerange.Find(timerangePlayer =>
                timerangePlayer.Player.ContentId == player.ContentId);

            if (timerangePlayerFoundByContentId != null) return timerangePlayerFoundByContentId;
        }

        // If not found, try to find with name and homeworld
        WithdrawnGilsPerPlayerTimerange? timerangePlayerFoundByPlayerNameAndHomeworld = Service.Configuration!.WithdrawnGilsPerPlayerTimerange.Find(timerangePlayer =>
            timerangePlayer.Player.PlayerName == player.PlayerName && timerangePlayer.Player.HomeWorld == player.HomeWorld);

        if (timerangePlayerFoundByPlayerNameAndHomeworld != null) return timerangePlayerFoundByPlayerNameAndHomeworld;

        // if not found, return null
        return null;
    }

    public static BlacklistedPlayer? TryGetBlacklistedPlayerFromConfiguration(Player player)
    {
        // Try to get an existing blacklisted player matched by content id
        if (player.ContentId != null)
        {
            BlacklistedPlayer? blacklistedPlayerFoundByContentId = Service.Configuration!.BlacklistedPlayers.Find(blacklistedPlayer =>
                blacklistedPlayer.ListedPlayer.Player.ContentId == player.ContentId);

            if (blacklistedPlayerFoundByContentId != null) return blacklistedPlayerFoundByContentId;
        }

        // If not found, try to find with name and homeworld
        BlacklistedPlayer? blacklistedPlayerFoundByPlayerNameAndHomeworld = Service.Configuration!.BlacklistedPlayers.Find(blacklistedPlayer => 
            blacklistedPlayer.ListedPlayer.Player.PlayerName == player.PlayerName && blacklistedPlayer.ListedPlayer.Player.HomeWorld == player.HomeWorld);

        if (blacklistedPlayerFoundByPlayerNameAndHomeworld != null) return blacklistedPlayerFoundByPlayerNameAndHomeworld;

        // If not found, try to find with account id
        if (player.AccountId != null)
        {
            BlacklistedPlayer? blacklistedPlayerFoundByAccountId = Service.Configuration.BlacklistedPlayers.Find(blacklistedPlayer =>
            blacklistedPlayer.ListedPlayer.Player.AccountId == player.AccountId);

            if (blacklistedPlayerFoundByAccountId != null) return blacklistedPlayerFoundByAccountId;
        }

        // if not found, return null
        return null;
    }

    public static bool IsPlayerAltOfBlacklistedPlayer(Player playerToCheck, BlacklistedPlayer blacklistedPlayer)
    {
        if (playerToCheck.AccountId != null && blacklistedPlayer.ListedPlayer.Player.AccountId != null &&
            playerToCheck.AccountId == blacklistedPlayer.ListedPlayer.Player.AccountId &&
            (playerToCheck.PlayerName != blacklistedPlayer.ListedPlayer.Player.PlayerName || playerToCheck.HomeWorld != blacklistedPlayer.ListedPlayer.Player.HomeWorld))
        {
            return true;
        }

        return false;
    }

    public static bool IsPlayerBlacklisted(Player player)
    {
        // 1 : Check all blacklisted players with the same AccountId (If player.AccountId is not null)
        if (player.AccountId != null)
        {
            List<BlacklistedPlayer> blacklistedPlayerFoundByAccountIdList = Service.Configuration!.BlacklistedPlayers.FindAll(blacklistedPlayer => blacklistedPlayer.ListedPlayer.Player.AccountId == player.AccountId);

            foreach (var blacklistedPlayerFoundByAccountId in blacklistedPlayerFoundByAccountIdList)
            {
                if (blacklistedPlayerFoundByAccountId.Enabled == false) continue;

                switch (blacklistedPlayerFoundByAccountId.ListedPlayer.ListedType)
                {
                    case ListedType.ServiceAccount:
                        {
                            // The player has been service account banned, he is blacklisted
                            return true;
                        }
                    case ListedType.Character:
                        {
                            // We check that the content id is not null, just in case (It shouldn't be null since the player has been added via target cause we have their AccountID)
                            if (blacklistedPlayerFoundByAccountId.ListedPlayer.Player.ContentId != null)
                            {
                                bool flagContentIdMatch = blacklistedPlayerFoundByAccountId.ListedPlayer.Player.ContentId == player.ContentId;

                                if (flagContentIdMatch)
                                    return true;
                                else
                                    continue;
                            }
                            else // If the player has been added manually, we want to check the player name and homeworld
                            {
                                bool flagPlayerNameMatch = blacklistedPlayerFoundByAccountId.ListedPlayer.Player.PlayerName == player.PlayerName;
                                bool flagPlayerHomeworldMatch = blacklistedPlayerFoundByAccountId.ListedPlayer.Player.HomeWorld == player.HomeWorld;

                                if (flagPlayerNameMatch && flagPlayerHomeworldMatch)
                                    return true;
                                else
                                    continue;
                            }
                        }
                }
            }
        }

        // 2 : Check all blacklisted players with the same ContentId (Might be useless, but is added for more security)
        if (player.ContentId != null)
        {
            List<BlacklistedPlayer> blacklistedPlayerFoundByContentIdList = Service.Configuration!.BlacklistedPlayers.FindAll(blacklistedPlayer => blacklistedPlayer.ListedPlayer.Player.ContentId == player.ContentId);

            foreach (var blacklistedPlayerFoundByContentId in blacklistedPlayerFoundByContentIdList)
            {
                if (blacklistedPlayerFoundByContentId.Enabled == false) continue;

                return true;
            }
        }

        // 3 : Check all blacklisted players with the same PlayerName and Homeworld
        List<BlacklistedPlayer> blacklistedPlayerFoundByPlayerNameAndHomeworldList = Service.Configuration!.BlacklistedPlayers.FindAll(blacklistedPlayer => blacklistedPlayer.ListedPlayer.Player.PlayerName == player.PlayerName && blacklistedPlayer.ListedPlayer.Player.HomeWorld == player.HomeWorld);

        foreach (var blacklistedPlayerFoundByPlayerNameAndHomeworld in blacklistedPlayerFoundByPlayerNameAndHomeworldList)
        {
            if (blacklistedPlayerFoundByPlayerNameAndHomeworld.Enabled == false) continue;

            return true;
        }

        // If none of the above checks returned true, the player is not blacklisted
        return false;
    }

    // TODO: Implement this
    public static bool IsPlayerWhitelisted(Player player)
    {
        // return Service.Configuration.WhitelistedPlayers.Any(whitelistedPlayer => whitelistedPlayer.Player.AccountId == player.AccountId);
        return true;
    }

    public static string GetCharaNameFromCID(ulong CID)
    {
        if (Service.Configuration!.SeenCharacters.TryGetValue(CID, out var name)) return name;
        return $"Unknown character {CID:X16}";
    }

    public async static Task<(bool isTargetValid, string text, Player? outTargetedPlayer)> IsTargetValid(bool allowSelfTarget = false)
    {
        string buttonText;
        bool isTargetValid = true;
        Player? targetedPlayer = null;

        if (Svc.Targets.Target == null)
        {
            buttonText = "Target a player to add";
            isTargetValid = false;
        }
        else if (Svc.Targets.Target is not IPlayerCharacter)
        {
            buttonText = "Target a player to add";
            isTargetValid = false;
        }
        else if (!allowSelfTarget && Svc.Targets.Target is IPlayerCharacter pc && pc.Equals(await Service.GetPlayerCharacterAsync()))
        {
            buttonText = "Cannot add yourself";
            isTargetValid = false;
        }
        else if (Svc.Targets.Target is IPlayerCharacter)
        {
            targetedPlayer = MakePlayerFromPlayerCharacterObject(Svc.Targets.Target as IPlayerCharacter);

            if (targetedPlayer == null)
            {
                buttonText = "Target a player to add";
                isTargetValid = false;
            }
            else
            {
                buttonText = $"Add {targetedPlayer.PlayerName}@{targetedPlayer.HomeWorld}";
            }
        }
        else
        {
            buttonText = "The target is invalid";
            isTargetValid = false;
        }

        return (isTargetValid, buttonText, targetedPlayer);
    }
}
