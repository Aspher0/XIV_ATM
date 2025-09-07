using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System.Linq;
using XIVATM.Helpers;
using ECommons.DalamudServices;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using XIVATM.Handlers;
using XIVATM.Structs;

namespace XIVATM.Events;

public static class ChatEvents
{
    public static void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (isHandled) return;

        string messageToString = message.ToString();

        // If not a SystemMessage or TellIncoming message, return
        if (type != XivChatType.TellIncoming && type != XivChatType.SystemMessage && (int)type != 313 && (int)type != 569) return;

        // If the system message is not about a trade request, trade offer, trade accepted or trade canceled, return
        if (type != XivChatType.TellIncoming && 
            !messageToString.StartsWith("Trade request sent to") && !messageToString.EndsWith("wishes to trade with you.") && 
            messageToString != "Trade complete." && messageToString != "Trade canceled.")
            return;

        // If the message is a system message, check if it is a trade complete or canceled, to reset the trade partner
        if (type != XivChatType.TellIncoming)
        {
            if (messageToString == "Trade complete.")
            {
                // If the player to trade is the same as the trade partner, one of the potential multiple trades has been completed
                if (Service.PlayerToTrade != null && Service.TradePartner != null &&
                    Service.PlayerToTrade.PlayerName == Service.TradePartner.PlayerName &&
                    Service.PlayerToTrade.HomeWorld == Service.TradePartner.HomeWorld && 
                    Service.CurrentTradedGils != null && Service.CurrentTradedGils > 0)
                {
                    // We need to update the gils sent
                    CommandHandler.UpdateGilsSent(Service.PlayerToTrade, Service.CurrentTradedGils.Value);
                    Service.CurrentTradedGils = null;
                }

                Service.TradePartner = null;
                return;
            }
            else if (messageToString == "Trade canceled.")
            {
                // If the player to trade not null, and if we were trading them, we need to cancel every future tasks
                if (Service.PlayerToTrade != null && Service.TradePartner != null && 
                    Service.PlayerToTrade.PlayerName == Service.TradePartner.PlayerName && 
                    Service.PlayerToTrade.HomeWorld == Service.TradePartner.HomeWorld)
                {
                    CommonHelper.AddToHistory($"Trade canceled with {Service.PlayerToTrade.GetPlayerNameWorld()}.");

                    // We need to cancel all future tasks
                    Service.PlayerToTrade = null;
                    Service.CurrentTradedGils = null;
                    Service.IsTransactionOngoing = false;
                    Service.TaskManager.Abort();
                    IPCHelper.RemoveHonorificTitleTransactionOngoing();
                }

                Service.TradePartner = null;
                return;
            }
        }

        var payloads = (type == XivChatType.TellIncoming) ? sender.Payloads : message.Payloads;

        if (payloads.Count == 0) return;

        // If it's not a "trade complete" or "trade canceled" message, then it means it is a tell or a trade request / offer, so it must have a player payload
        var foundPlayerPayload = payloads.FirstOrDefault(payload => payload.Type == PayloadType.Player);

        if (foundPlayerPayload == null) return;

        PlayerPayload? playerPayload = foundPlayerPayload as PlayerPayload;

        if (playerPayload == null) return;

        // We get the name of the player and its homeworld
        string playerName = playerPayload.PlayerName;
        string playerHomeWorld = playerPayload.World.Value.Name.ToString();

        // We try to get the GameObject of the player from the map, if it is not found, player will be null and playerObject will also be null
        var player = Svc.Objects.FirstOrDefault(obj => obj is IPlayerCharacter pc && pc.Name.TextValue == playerName && pc.HomeWorld.Value.Name == playerHomeWorld) as IPlayerCharacter;
        var playerObject = PlayerHelper.MakePlayerFromPlayerCharacterObject(player);

        // If the player has not been found on the map
        if (playerObject == null)
        {
            // We check if it was a trade message, if yes, we reset the trade partner
            if (type != XivChatType.TellIncoming)
                Service.TradePartner = null;

            return;
        }

        // If it was a trade message, we just set the trade partner and return
        if (type != XivChatType.TellIncoming)
        {
            Service.TradePartner = playerObject;
            return;
        }

        CommandHandler.HandleReceiveTell(playerObject, player, message.ToString());
    }
}
