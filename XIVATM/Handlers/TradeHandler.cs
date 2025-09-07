using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.ChatMethods;
using System;
using XIVATM.Helpers;
using XIVATM.Structs;

namespace XIVATM.Handlers;

public static class TradeHandler
{
    public static unsafe void BeginTrading(IPlayerCharacter playerCharacterToSendTo, Player playerToSendTo, int totalGilsToSend, Action? callbackOnTradeSuccess = null)
    {
        int gils = totalGilsToSend;

        while (gils > 0)
        {
            int gil = Math.Min(TradeHelper.MaxGil, gils);
            gils -= gil;
            TradeEnqueue(playerCharacterToSendTo, playerToSendTo, gil, callbackOnTradeSuccess);
        }
    }

    public static unsafe void TradeEnqueue(IPlayerCharacter playerCharacterToSendTo, Player playerToSendTo, int gil, Action? callbackOnTradeSuccess = null)
    {
        Service.TaskManager.Enqueue(() => { Service.IsTransactionOngoing = true; Service.CurrentTradedGils = gil; }, $"Service.IsTransactionOngoing = true, Service.CurrentTradedGils = {gil}");
        Service.TaskManager.Enqueue(() => { TradeHelper.ConfirmAllowed = false; }, "ConfirmAllowed = false");

        // We check if a trade is already ongoing
        // If yes, and the player we are currently trading is someone else, we just wait for it to finish
        if (Service.TradePartner != null && (Service.TradePartner.PlayerName != playerToSendTo.PlayerName || Service.TradePartner.HomeWorld != playerToSendTo.HomeWorld))
        {
            Service.TaskManager.Enqueue(() => { CommonHelper.AddToHistory($"Waiting for the trade with {Service.TradePartner.GetPlayerNameWorld()} to finish."); });
            Service.TaskManager.Enqueue(TradeHelper.WaitUntilTradeNotOpen);
            Service.TaskManager.Enqueue(() => TradeHelper.UseTradeOn(new Sender(playerCharacterToSendTo).ToString()), $"UseTradeOn({playerCharacterToSendTo})");
            Service.TaskManager.Enqueue(() => { CommonHelper.AddToHistory($"Waiting for the trade window with {playerToSendTo.GetPlayerNameWorld()} to open..."); });
            Service.TaskManager.Enqueue(TradeHelper.WaitUntilTradeOpen);
        }
        // Otherwise, if we are not currently trading, we can proceed to trading
        else if (Service.TradePartner == null)
        {
            Service.TaskManager.Enqueue(() => TradeHelper.UseTradeOn(new Sender(playerCharacterToSendTo).ToString()), $"UseTradeOn({playerCharacterToSendTo})");
            Service.TaskManager.Enqueue(() => { CommonHelper.AddToHistory($"Waiting for the trade window with {playerToSendTo.GetPlayerNameWorld()} to open..."); });
            Service.TaskManager.Enqueue(TradeHelper.WaitUntilTradeOpen);
        }

        Service.TaskManager.Enqueue(() => { CommonHelper.AddToHistory($"Trade window opened."); });

        // Check if title on transaction ongoing needs to be applied, if yes, apply honorific title transaction ongoing
        if (Service.Configuration!.ApplyHonorificTitleOnTransactionOngoing && Service.Configuration.HonorificTitleOnTransactionOngoing != null)
        {
            Service.TaskManager.Enqueue(() => { CommonHelper.AddToHistory($"Applying honorific title for transaction ongoing."); });
            Service.TaskManager.Enqueue(() => { Service.HonorificIPC_Caller.SetTitle(Service.Configuration.HonorificTitleOnTransactionOngoing.Title); }, $"Service.HonorificIPC_Caller.SetTitle({Service.Configuration.HonorificTitleOnTransactionOngoing.Title})");
        }

        // If we are already trading the player we have to send gils to, we should only have to input the gils to send since the trade window is open

        if (gil > 0)
        {
            Service.TaskManager.Enqueue(() => { CommonHelper.AddToHistory($"Inputing {gil:N0} gils."); });
            Service.TaskManager.Enqueue(TradeHelper.OpenGilInput);
            Service.TaskManager.Enqueue(() => TradeHelper.SetNumericInput(gil), $"SetNumericInput({gil})");
        }

        Service.TaskManager.Enqueue(() => { TradeHelper.ConfirmAllowed = true; }, "ConfirmAllowed = true");
        Service.TaskManager.Enqueue(() => { CommonHelper.AddToHistory($"Waiting for the trade with {playerToSendTo.GetPlayerNameWorld()} to complete."); });
        Service.TaskManager.Enqueue(TradeHelper.WaitUntilTradeNotOpen);
        Service.TaskManager.Enqueue(() => { CommonHelper.AddToHistory($"Trade with {playerToSendTo.GetPlayerNameWorld()} complete."); });

        if (Service.Configuration!.ApplyHonorificTitleOnTransactionOngoing && Service.Configuration.HonorificTitleOnTransactionOngoing != null)
            Service.TaskManager.Enqueue(() => { CommonHelper.AddToHistory($"Removing honorific title for transaction ongoing."); });

        Service.TaskManager.Enqueue(IPCHelper.RemoveHonorificTitleTransactionOngoing, $"IPCHelper.RemoveHonorificTitleTransactionOngoing");
        Service.TaskManager.Enqueue(() => { Service.IsTransactionOngoing = false; }, "Service.IsTransactionOngoing = false");

        if (callbackOnTradeSuccess != null)
            Service.TaskManager.Enqueue(callbackOnTradeSuccess);

        Service.TaskManager.DelayNext(Math.Max(15, 15), true);
    }
}
