using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using ECommons;
using ECommons.Automation;
using ECommons.ChatMethods;
using ECommons.DalamudServices;
using ECommons.Logging;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System;
using System.Linq;
using XIVATM.Structs;
using Action = System.Action;

namespace XIVATM.Helpers;

public static class TradeHelper
{
    internal static volatile bool ConfirmAllowed = false;
    public static int MaxGil = 1000000;
    internal static bool IsActive => Service.TaskManager.IsBusy;

    public static string TradeText => Svc.Data.GetExcelSheet<Addon>().GetRow(102223).Text.ExtractText();

    internal static bool? UseTradeOn(string player)
    {
        IGameObject? igameObject = Svc.Objects.Where(x => x is IPlayerCharacter pc && pc.IsTargetable && new Sender(pc).ToString() == player).FirstOrDefault();

        if (igameObject != null)
        {
            if (Svc.Targets.Target?.Address == igameObject.Address)
            {
                if (GenericThrottle() && EzThrottler.Throttle("TradeOpen", Service.TradeThrottle))
                {
                    Chat.Instance.SendMessage("/trade");
                    return true;
                }
            }
            else if (GenericThrottle())
                Svc.Targets.Target = igameObject;
        }

        return false;
    }

    internal static unsafe bool? OpenGilInput()
    {
        if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Trade", out var addon) && GenericHelpers.IsAddonReady(addon))
        {
            if (GenericThrottle())
            {
                Callback.Fire(addon, true, 2, Callback.ZeroAtkValue);
                return true;
            }
        }
        else
            GenericThrottle(true);

        return false;
    }

    internal static bool GenericThrottle(bool rethrottle = false) => FrameThrottler.Throttle("TaskThrottle", Math.Max(2, Service.TradeDelay), rethrottle);
    internal static bool? WaitUntilTradeOpen() => Svc.Condition[ConditionFlag.TradeOpen];
    internal static bool? WaitUntilTradeNotOpen() => !Svc.Condition[ConditionFlag.TradeOpen];

    internal static unsafe bool? SetNumericInput(int num)
    {
        if (num < 0 || num > 1000000) throw new ArgumentOutOfRangeException(nameof(num));

        if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("InputNumeric", out var addon) && GenericHelpers.IsAddonReady(addon))
        {
            if (GenericThrottle())
            {
                Callback.Fire(addon, true, num);
                return true;
            }
        }
        else
            GenericThrottle(true);

        return false;
    }

    // If the plugin is enabled, and there is an ongoing transaction (someone used ATM), and the current player we are trading is the same as the person we need to send gils to
    // then we can auto accept trade
    public static unsafe bool CanAutoAcceptTrade() => Service.Configuration!.PluginEnabled && Service.IsTransactionOngoing && Service.TradePartner != null && Service.PlayerToTrade != null &&
                                                Service.TradePartner.PlayerName == Service.PlayerToTrade.PlayerName && Service.TradePartner.HomeWorld == Service.PlayerToTrade.HomeWorld;

    public static unsafe AtkUnitBase* GetSpecificYesno(params string[] s)
    {
        for (int i = 1; i < 100; i++)
        {
            try
            {
                var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", i).Address;

                if (addon == null)
                    return null;

                if (GenericHelpers.IsAddonReady(addon))
                {
                    var textNode = addon->UldManager.NodeList[15]->GetAsAtkTextNode();
                    var text = MemoryHelper.ReadSeString(&textNode->NodeText).ExtractText();
                    if (text.EqualsAny(s))
                    {
                        PluginLog.Verbose($"SelectYesno {s.Print()} addon {i}");
                        return addon;
                    }
                }
            }
            catch (Exception e)
            {
                e.Log();
                return null;
            }
        }
        return null;
    }
}
