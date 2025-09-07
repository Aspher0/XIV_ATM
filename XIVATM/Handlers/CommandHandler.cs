using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.Toast;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using XIVATM.Helpers;
using XIVATM.Structs;
using Player = XIVATM.Structs.Player;

namespace XIVATM.Handlers;

public static class CommandHandler
{
    public static void ToggleATMMode()
    {
        Service.Configuration!.UpdateConfiguration(() => { Service.Configuration.PluginEnabled = !Service.Configuration.PluginEnabled; });

        Svc.Toasts.ShowQuest($"ATM mode {(Service.Configuration.PluginEnabled ? "enabled" : "disabled")}",
                new QuestToastOptions() { PlaySound = true, DisplayCheckmark = true });

        CommonHelper.AddToHistory($"ATM Mode {(Service.Configuration.PluginEnabled ? "enabled" : "disabled")}.");

        IPCHelper.CheckShouldApplyAll();
    }

    public static void HandleReceiveTell(Player sender, IPlayerCharacter? senderCharacter, string message)
    {
        // Check if the ATM Mode is enabled, if false, return
        // RECHECK THIS BEFORE ACTUALLY TRADING
        if (!Service.Configuration!.PluginEnabled)
        {
            LoggerHelper.Debug("HandleReceiveTell - Plugin is disabled");
            return;
        }

        // If the character object is null, return
        // RECHECK THIS BEFORE ACTUALLY TRADING
        if (senderCharacter == null)
        {
            LoggerHelper.Debug("HandleReceiveTell - Sender character is null");
            return;
        }

        // Check if sender is yourself (Just in case), if true return
        if (sender.ContentId == ECommons.GameHelpers.Player.CID)
        {
            LoggerHelper.Debug("HandleReceiveTell - Sender is yourself, cannot tell yourself");
            return;
        }

        var distanceToOtherPlayer = ECommons.GameHelpers.Player.DistanceTo(senderCharacter);
        ChecksBeforeTrade canSendGils = CheckCanSendGils();

        // Checks if the sender is ATMMaster
        // Need to implement this in the settings UI and Configuration instead of hardcoding it
        if (sender.AccountId == 123456789)
        {
            if (distanceToOtherPlayer >= 4f)
            {
                LoggerHelper.DebugBuildLog("HandleReceiveTell - ATMMaster is too far away to initiate a trade");
                return;
            }

            // Do stuff

            // return;
        }

        // Checks if there is at least one configured trigger phrase, and that it is valid
        if (!canSendGils.HasTriggerPhrases)
        {
            LoggerHelper.Debug("HandleReceiveTell - No trigger phrases are configured");
            return;
        }

        bool isTriggerPhraseFound = false;

        // Check for trigger phrases matching
        foreach (var triggerPhrase in Service.Configuration.DefaultTriggerPhrases)
        {
            string messageFromSender = message;
            string phrase = triggerPhrase.Phrase;
            TriggerCaseSensitivity triggerCaseSensitivity = triggerPhrase.CaseSensitivity;
            TriggerPlacementInMessage triggerPlacementInMessage = triggerPhrase.PlacementInMessage;

            if (triggerCaseSensitivity == TriggerCaseSensitivity.CaseInsensitively)
            {
                phrase = phrase.ToLower();
                messageFromSender = messageFromSender.ToLower();
            }

            if (phrase == string.Empty)
                continue;

            switch (triggerPlacementInMessage)
            {
                case TriggerPlacementInMessage.StartWith:
                    {
                        if (messageFromSender.StartsWith(phrase, false, null))
                        {
                            isTriggerPhraseFound = true;
                            break;
                        }
                        continue;
                    }
                case TriggerPlacementInMessage.EndWith:
                    {
                        if (messageFromSender.EndsWith(phrase, false, null))
                        {
                            isTriggerPhraseFound = true;
                            break;
                        }
                        continue;
                    }
                case TriggerPlacementInMessage.Contain:
                    {
                        if (messageFromSender.Contains(phrase))
                        {
                            isTriggerPhraseFound = true;
                            break;
                        }
                        continue;
                    }
            }
        }

        // Checks if if a trigger phrase has been found, if not return
        if (!isTriggerPhraseFound)
        {
            LoggerHelper.Debug("HandleReceiveTell - No matching trigger phrase has been found");
            return;
        }

        // Checks if the sender is blacklisted
        // RECHECK THIS BEFORE ACTUALLY TRADING
        if (Service.Configuration.BlacklistEnabled && PlayerHelper.IsPlayerBlacklisted(sender))
        {
            CommonHelper.AddToHistory($"{sender.GetPlayerNameWorld()} tried to use you, but the player is blacklisted.");
            LoggerHelper.Debug($"HandleReceiveTell - Sender character ({sender.GetPlayerNameWorld()}) is blacklisted");
            return;
        }

        // Checks if the sender is whitelisted
        // RECHECK THIS BEFORE ACTUALLY TRADING
        if (Service.Configuration.WhitelistEnabled && !PlayerHelper.IsPlayerWhitelisted(sender))
        {
            CommonHelper.AddToHistory($"{sender.GetPlayerNameWorld()} tried to use you, but the player is not whitelisted.");
            LoggerHelper.Debug($"HandleReceiveTell - Sender character ({sender.GetPlayerNameWorld()}) is not whitelisted");
            return;
        }

        bool isValidGilAmountToSendFixed = canSendGils.IsValidGilAmountToSendFixed;
        bool isValidGilAmountToSendRandomRangeBoth = canSendGils.IsValidGilAmountToSendRandomRangeBoth;
        bool isValidGilAmountToSendRandomRangeLow = canSendGils.IsValidGilAmountToSendRandomRangeLow;
        bool isValidDefaultGilAmountPlayerTimerange = canSendGils.IsValidDefaultGilAmountPlayerTimerange;
        bool isValidGlobalGilAmountTimerange = canSendGils.IsValidGlobalGilAmountTimerange;
        bool isValidGlobalGilAmountWithdrawable = canSendGils.IsValidGlobalGilAmountWithdrawable;

        // Checks if the gil amounts tab is configured properly, meaning gil values are valid
        // RECHECK THIS BEFORE ACTUALLY TRADING
        if (!isValidGilAmountToSendFixed || !isValidGilAmountToSendRandomRangeBoth || !isValidGilAmountToSendRandomRangeLow ||
            !isValidDefaultGilAmountPlayerTimerange || !isValidGlobalGilAmountTimerange || !isValidGlobalGilAmountWithdrawable)
        {
            CommonHelper.AddToHistory($"{sender.GetPlayerNameWorld()} tried to use you, but the default gil amounts tab is not configured properly.");
            LoggerHelper.Debug("HandleReceiveTell - Gil amounts tab is not configured properly");
            return;
        }

        // Check if maximum gil cap has been reached, if yes, return
        // RECHECK THIS BEFORE ACTUALLY TRADING
        if (canSendGils.HasReachedTheGlobalLimitOfGils)
        {
            CommonHelper.AddToHistory($"{sender.GetPlayerNameWorld()} tried to use you, but the global maximum gil cap has been reached.");
            LoggerHelper.Debug("HandleReceiveTell - The global maximum gil cap has been reached");
            // If enabled, send a message to the player
            return;
        }

        // Check if maximum gil cap per timerange has been reached, if yes, return
        // RECHECK THIS BEFORE ACTUALLY TRADING
        if (canSendGils.HasReachedTheGlobalLimitOfGilsPerTimerange)
        {
            CommonHelper.AddToHistory($"{sender.GetPlayerNameWorld()} tried to use you, but the global maximum gil cap per timerange has been reached.");
            LoggerHelper.Debug("HandleReceiveTell - The global maximum gil cap per timerange has been reached");
            // If enabled, send a message to the player
            return;
        }

        // Check if the player has reached the maximum gil cap per timerange they are allowed to take
        // RECHECK THIS BEFORE ACTUALLY TRADING
        if (canSendGils.HasPlayerReachedTheGlobalLimitOfGilsPerTimerange)
        {
            CommonHelper.AddToHistory($"{sender.GetPlayerNameWorld()} tried to use you, but the player has reached the maximum gil amount they are allowed to take per timerange (Service Account wide).");
            LoggerHelper.Debug("HandleReceiveTell - This player or an alt of theirs has reached the maximum gil cap per timerange they are allowed to take");
            // If enabled, send a message to the player
            return;
        }

        // Check if sender is near the player (Can reach for trade), if false, return
        // RECHECK THIS BEFORE ACTUALLY TRADING
        if (distanceToOtherPlayer >= 4f)
        {
            CommonHelper.AddToHistory($"{sender.GetPlayerNameWorld()} tried to use you, but the player is too far away to initiate a trade.");
            LoggerHelper.Debug("HandleReceiveTell - The sender is too far away to initiate a trade");
            return;
        }

        // Calculate how much gils should be sent
        // First, we get the configured default amount of gils to send
        int defaultGilsToSend;

        if (Service.Configuration.DefaultGilAmountToSend.GilSenderMode == GilSenderMode.Fixed)
            defaultGilsToSend = Service.Configuration.DefaultGilAmountToSend.FixedAmount;
        else
            defaultGilsToSend = new Random().Next(Service.Configuration.DefaultGilAmountToSend.RangeLow, Service.Configuration.DefaultGilAmountToSend.RangeHigh);

        // Now, we have to check the maximum amount that can be sent to the player depending on the set limits (Player Timerange, Global Timerange, Global Gils Sent) and the default gils to send
        // We have already checked that the player has not reached the maximum gil cap per timerange they are allowed to take, the global limit per timerange has not been reached and the global limit has not been reached

        // We make an empty int array that will be populated with the possible gils that can be sent to the player according to limits
        List<int> possibleGilsToSend = [];

        // Now we populate the array with the possible gils that can be sent to the player (For each timerange, it corresponds to the limit of gils minus the amount already withdrawn)
        if (Service.Configuration.DefaultMaxGilsPerPlayerTimerange.MaxGils > 0)
        {
            int gilsLeft;

            var foundTimerangePlayerInConfiguration = PlayerHelper.TryGetWithdrawnGilsPerPlayerTimerangeFromConfiguration(sender);

            if (foundTimerangePlayerInConfiguration != null)
            {
                if (foundTimerangePlayerInConfiguration.WithdrawnGilsPerTimerange.TimeRangeDateTime.End >= DateTimeOffset.Now)
                {
                    gilsLeft = Service.Configuration.DefaultMaxGilsPerPlayerTimerange.MaxGils - foundTimerangePlayerInConfiguration.WithdrawnGilsPerTimerange.WithdrawnGils;
                } else
                {
                    gilsLeft = Service.Configuration.DefaultMaxGilsPerPlayerTimerange.MaxGils;
                }
            }
            else
            {
                gilsLeft = Service.Configuration.DefaultMaxGilsPerPlayerTimerange.MaxGils;
            }

            possibleGilsToSend.Add(gilsLeft);
            LoggerHelper.Debug($"HandleReceiveTell - Player Timerange Gils left ({sender.GetPlayerNameWorld()}): {gilsLeft:N0}");
        }

        if (Service.Configuration.DefaultGlobalMaxGilsPerTimerange.MaxGils > 0)
        {
            int gilsLeft;

            if (Service.Configuration.GlobalWithdrawnGilsPerTimerange != null)
            {
                if (Service.Configuration.GlobalWithdrawnGilsPerTimerange.TimeRangeDateTime.End >= DateTimeOffset.Now)
                {
                    gilsLeft = Service.Configuration.DefaultGlobalMaxGilsPerTimerange.MaxGils - Service.Configuration.GlobalWithdrawnGilsPerTimerange.WithdrawnGils;
                }
                else
                {
                    gilsLeft = Service.Configuration.DefaultGlobalMaxGilsPerTimerange.MaxGils;
                }
            }
            else
            {
                gilsLeft = Service.Configuration.DefaultGlobalMaxGilsPerTimerange.MaxGils;
            }

            possibleGilsToSend.Add(gilsLeft);
            LoggerHelper.Debug($"HandleReceiveTell - Global Timerange Gils left: {gilsLeft:N0}");
        }

        if (Service.Configuration.GlobalMaxGils > 0)
        {
            int gilsLeft = Service.Configuration.GlobalMaxGils - Service.Configuration.GlobalGilsSent;
            possibleGilsToSend.Add(gilsLeft);
            LoggerHelper.Debug($"HandleReceiveTell - Global Gils left: {gilsLeft:N0}");
        }

        // If the array is empty, it means there is no limits set, so we can send the default gils to send
        if (possibleGilsToSend.Count == 0)
        {
            CommonHelper.AddToHistory($"No limits set, initiating trade of {defaultGilsToSend:N0} gils with {sender.GetPlayerNameWorld()}.");
            LoggerHelper.Debug($"HandleReceiveTell - No limits set, proceeding to the trade of default gils ({defaultGilsToSend:N0})");
            TradeGilsToPlayer(senderCharacter, sender, defaultGilsToSend);
            return;
        }

        // If the array is not empty, we have to find the minimum value in the array and send that amount of gils
        int maxGilsSendable = possibleGilsToSend.Min();

        if (defaultGilsToSend > maxGilsSendable)
        {
            CommonHelper.AddToHistory($"Limit found, initiating trade of a maximum of {maxGilsSendable:N0} gils with {sender.GetPlayerNameWorld()}.");
            LoggerHelper.DebugBuildLog($"HandleReceiveTell - The default gils to send ({defaultGilsToSend:N0}) is higher than the maximum gils that can be sent to that player ({maxGilsSendable:N0}), sending the maximum amount of gils");
            TradeGilsToPlayer(senderCharacter, sender, maxGilsSendable);
            return;
        }

        CommonHelper.AddToHistory($"Initiating trade of {defaultGilsToSend:N0} gils with {sender.GetPlayerNameWorld()}.");
        LoggerHelper.DebugBuildLog($"HandleReceiveTell - All checks passed, proceeding to the trade of {defaultGilsToSend:N0} gils to {sender.GetPlayerNameWorld()}");
        TradeGilsToPlayer(senderCharacter, sender, defaultGilsToSend);
    }

    public static ChecksBeforeTrade CheckCanSendGils(Player? player = null)
    {
        // Check that the configuration contains at least one trigger
        bool hasTriggerPhrases = true;
        if (Service.Configuration!.DefaultTriggerPhrases.Count == 0)
            hasTriggerPhrases = false;

        // Check that the configuration contains at least one valid trigger phrase (triggerPhrase.Phrase is not empty)
        bool hasOneValidTriggerPhrase = false;
        foreach (var triggerPhrase in Service.Configuration!.DefaultTriggerPhrases)
        {
            if (triggerPhrase.Phrase != string.Empty)
            {
                hasOneValidTriggerPhrase = true;
                break;
            }
        }

        // Check that the default gil amount to send is a valid value
        bool isValidGilAmountToSendFixed = true;
        bool isValidGilAmountToSendRandomRangeBoth = true;
        bool isValidGilAmountToSendRandomRangeLow = true;
        switch (Service.Configuration!.DefaultGilAmountToSend.GilSenderMode)
        {
            case GilSenderMode.Fixed:
                {
                    if (Service.Configuration!.DefaultGilAmountToSend.FixedAmount == 0)
                        isValidGilAmountToSendFixed = false;

                    break;
                }
            case GilSenderMode.RandomRange:
                {
                    if (Service.Configuration!.DefaultGilAmountToSend.RangeLow == 0 && Service.Configuration!.DefaultGilAmountToSend.RangeHigh == 0)
                        isValidGilAmountToSendRandomRangeBoth = false;
                    else if (Service.Configuration!.DefaultGilAmountToSend.RangeLow == 0 || Service.Configuration!.DefaultGilAmountToSend.RangeHigh == 0)
                        isValidGilAmountToSendRandomRangeLow = false;

                    break;
                }
        }

        // Check that the default gil amount per player timerange is a valid value
        bool isValidDefaultGilAmountPlayerTimerange = true;
        if (Service.Configuration.DefaultMaxGilsPerPlayerTimerange.MaxGils == 0)
            isValidDefaultGilAmountPlayerTimerange = false;

        // Check that the global gil amount per timerange is a valid value
        bool isValidGlobalGilAmountTimerange = true;
        if (Service.Configuration.DefaultGlobalMaxGilsPerTimerange.MaxGils == 0)
            isValidGlobalGilAmountTimerange = false;

        // Check that the global gil amount withdrawable is a valid value
        bool isValidGlobalGilAmountWithdrawable = true;
        if (Service.Configuration.GlobalMaxGils == 0)
            isValidGlobalGilAmountWithdrawable = false;

        // Check whether the amount of gil spent has reached the global limit
        bool hasReachedTheGlobalLimitOfGils = false;
        if (Service.Configuration.GlobalMaxGils >= 0 && Service.Configuration.GlobalGilsSent >= Service.Configuration.GlobalMaxGils)
            hasReachedTheGlobalLimitOfGils = true;

        // Checks for the current ongoing global timerange
        // Check if withdrawable amount during timerange not unlimited (if unlimited, skip) and if the global timerange is not null
        bool hasReachedTheGlobalLimitOfGilsPerTimerange = false;
        if (Service.Configuration.DefaultGlobalMaxGilsPerTimerange.MaxGils == 0)
        {
            hasReachedTheGlobalLimitOfGilsPerTimerange = true;
        }
        else if (Service.Configuration.DefaultGlobalMaxGilsPerTimerange.MaxGils > 0 && Service.Configuration.GlobalWithdrawnGilsPerTimerange != null)
        {
            // Check if the global timerange is ongoing
            if (Service.Configuration.GlobalWithdrawnGilsPerTimerange.TimeRangeDateTime.End >= DateTimeOffset.Now)
            {
                // Check if the gils sent during the timerange reached the limit
                if (Service.Configuration.GlobalWithdrawnGilsPerTimerange.WithdrawnGils >= Service.Configuration.DefaultGlobalMaxGilsPerTimerange.MaxGils)
                    hasReachedTheGlobalLimitOfGilsPerTimerange = true;
            }
        }

        bool hasPlayerReachedTheGlobalLimitOfGilsPerTimerange = false;
        if (player != null)
        {
            var foundTimerangePlayerInConfiguration = PlayerHelper.TryGetWithdrawnGilsPerPlayerTimerangeFromConfiguration(player);

            if (foundTimerangePlayerInConfiguration != null)
            {
                if (foundTimerangePlayerInConfiguration.WithdrawnGilsPerTimerange.TimeRangeDateTime.End >= DateTimeOffset.Now)
                {
                    if (foundTimerangePlayerInConfiguration.WithdrawnGilsPerTimerange.WithdrawnGils >= Service.Configuration.DefaultMaxGilsPerPlayerTimerange.MaxGils)
                        hasPlayerReachedTheGlobalLimitOfGilsPerTimerange = true;
                }
            }
        }

        // Return all the checks results
        return new(isValidGilAmountToSendFixed, isValidGilAmountToSendRandomRangeBoth, isValidGilAmountToSendRandomRangeLow,
                   isValidDefaultGilAmountPlayerTimerange, isValidGlobalGilAmountTimerange, isValidGlobalGilAmountWithdrawable,
                   hasTriggerPhrases, hasOneValidTriggerPhrase, 
                   hasReachedTheGlobalLimitOfGils, hasReachedTheGlobalLimitOfGilsPerTimerange, hasPlayerReachedTheGlobalLimitOfGilsPerTimerange);
    }

    public static void TradeGilsToPlayer(IPlayerCharacter playerCharacter, Player player, int totalGilsToSend)
    {
        // We set the PlayerToTrade to the player we have to trade to
        Service.PlayerToTrade = player;

        // TradeHelper.BeginTrading(playerCharacter, gilsSent, () => { UpdateGilsSent(player, gilsSent); });
        TradeHandler.BeginTrading(playerCharacter, player, totalGilsToSend);

        // TODO: Update Statistics
        // TODO: If enabled, send a message to the player after each transaction
    }

    public static void UpdateGilsSent(Player player, int gilsSent)
    {
        // Update the global gils sent amount
        Service.Configuration!.UpdateConfiguration(() => {
            Service.Configuration.GlobalGilsSent += gilsSent;
        });

        // Update the Player Timerange
        if (Service.Configuration.DefaultMaxGilsPerPlayerTimerange.MaxGils > 0)
        {
            var foundTimerangePlayerInConfiguration = PlayerHelper.TryGetWithdrawnGilsPerPlayerTimerangeFromConfiguration(player);

            if (foundTimerangePlayerInConfiguration == null || foundTimerangePlayerInConfiguration.WithdrawnGilsPerTimerange.TimeRangeDateTime.End < DateTimeOffset.Now)
            {
                string timerUniqueId = Guid.NewGuid().ToString();

                var newPlayerWithdrawnGilsPerTimerange = CommonHelper.MakeNewWithdrawnGilsPerPlayerTimerange(player, gilsSent, true, timerUniqueId, () =>
                {
                    // Delete the player from the list if it exists
                    var foundTimerangePlayerInConfigurationForDeletion = PlayerHelper.TryGetWithdrawnGilsPerPlayerTimerangeFromConfiguration(player);

                    Service.Configuration.UpdateConfiguration(() => {
                        if (foundTimerangePlayerInConfigurationForDeletion != null)
                        {
                            Service.Configuration.WithdrawnGilsPerPlayerTimerange.Remove(foundTimerangePlayerInConfigurationForDeletion);
                        }
                    });

                    if (Service.TimersList.ContainsKey(timerUniqueId))
                    {
                        Service.TimersList[timerUniqueId].Dispose();
                        Service.TimersList.Remove(timerUniqueId);
                    }
                }, false);

                Timer? timer = newPlayerWithdrawnGilsPerTimerange.outTimer;

                if (timer != null)
                {
                    Service.TimersList[timerUniqueId] = timer;

                    Service.Configuration.UpdateConfiguration(() => {
                        if (foundTimerangePlayerInConfiguration == null)
                        {
                            Service.Configuration.WithdrawnGilsPerPlayerTimerange.Add(newPlayerWithdrawnGilsPerTimerange.playerTimerange);
                        }
                        else
                        {
                            foundTimerangePlayerInConfiguration = newPlayerWithdrawnGilsPerTimerange.playerTimerange;
                        }
                    });
                }

            }
            else
            {
                Service.Configuration.UpdateConfiguration(() => { foundTimerangePlayerInConfiguration.WithdrawnGilsPerTimerange.WithdrawnGils += gilsSent; });
            }
        }

        // Update the Global Timerange
        if (Service.Configuration.DefaultGlobalMaxGilsPerTimerange.MaxGils > 0)
        {
            if (Service.Configuration.GlobalWithdrawnGilsPerTimerange == null || Service.Configuration.GlobalWithdrawnGilsPerTimerange.TimeRangeDateTime.End < DateTimeOffset.Now)
            {
                string timerUniqueId = Guid.NewGuid().ToString();

                var newGlobalWithdrawnGilsPerTimerange = CommonHelper.MakeNewGlobalWithdrawnGilsPerTimerange(gilsSent, true, timerUniqueId, () =>
                {
                    Service.Configuration.UpdateConfiguration(() => { Service.Configuration.GlobalWithdrawnGilsPerTimerange = null; });

                    if (Service.TimersList.ContainsKey(timerUniqueId))
                    {
                        Service.TimersList[timerUniqueId].Dispose();
                        Service.TimersList.Remove(timerUniqueId);
                    }
                }, false);

                Timer? timer = newGlobalWithdrawnGilsPerTimerange.outTimer;

                if (timer != null)
                {
                    Service.TimersList[timerUniqueId] = timer;
                    Service.Configuration.UpdateConfiguration(() => { Service.Configuration.GlobalWithdrawnGilsPerTimerange = newGlobalWithdrawnGilsPerTimerange.timerange; });
                }
            }
            else
            {
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.GlobalWithdrawnGilsPerTimerange.WithdrawnGils += gilsSent; });
            }
        }
    }
}
