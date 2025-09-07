using Dalamud.Game.ClientState.Objects;
using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Objects.SubKinds;
using XIVATM.IPC.Honorific;
using XIVATM.Structs;
using XIVATM.Helpers;
using System.Threading;
using System.Collections.Generic;
using System;
using ECommons.Automation.LegacyTaskManager;
using ECommons.EzSharedDataManager;
using ECommons.Throttlers;
using ECommons;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ECommons.Automation.UIInput;
using XIVATM.Models;
using XIVATM.IPC.Moodles;
using System.Threading.Tasks;

namespace XIVATM;

public class Service
{
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IPluginLog Logger { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] public static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IObjectTable Objects { get; private set; } = null!;
    [PluginService] public static ICondition Condition { get; private set; } = null!;
    [PluginService] public static IGameConfig GameConfig { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    // The distance from a player at which a trade can be initiated
    public static readonly float TradeDistance = 4.0f;

    public static XIVATM_Plugin Plugin { get; set; }

    public static Configuration? Configuration { get; set; }
    public static IPlayerCharacter? ConnectedPlayerObject { get; set; }
    public static Player? ConnectedPlayer { get; set; }
    public static HonorificIPC_Caller HonorificIPC_Caller = new();
    public static MoodlesIPC_Caller MoodlesIPC_Caller = new();

    public static List<HistoryEntry> HistoryEntriesList = [];

    public static TaskManager TaskManager = new()
    {
        AbortOnTimeout = true,
        TimeLimitMS = 60000,
    };
    public static int TradeDelay = 12;
    public static int TradeThrottle = 3000;

    public static bool YesAlreadyStopRequired;
    public static bool[] IsActive = EzSharedData.GetOrCreate("XIVATM.IsProcessingTasks", new bool[1]);

    // The current trade partner (Does not mean this person is using the ATM, it only defines the name, if any, of the player that you are currently trading with)
    public static Player? TradePartner { get; set; }
    // The player that is currently using the ATM
    public static Player? PlayerToTrade { get; set; }
    // Determines whether a transaction is currently ongoing, true means a player is using the ATM
    public static bool IsTransactionOngoing = false;
    public static int? CurrentTradedGils = null;

    // A dictionary of timers that are currently running, associated with their unique id as the key
    public static Dictionary<string, Timer> TimersList = [];

    public static void InitializeService()
    {
        TaskManager.CallbackOnTimeout = () =>
        {
            CommonHelper.AddToHistory($"Task timed out, aborting all current and queued tasks.");

            PlayerToTrade = null;
            IsTransactionOngoing = false;
            TaskManager.Abort();

            unsafe
            {
                if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Trade", out var addon) && GenericHelpers.IsAddonReady(addon))
                {
                    var check = addon->UldManager.NodeList[31]->GetAsAtkComponentNode()->Component->UldManager.NodeList[0]->GetAsAtkImageNode();

                    var cancelTradeButton = (AtkComponentButton*)addon->UldManager.NodeList[2]->GetComponent();

                    if (EzThrottler.Check("TradeArtificialThrottle") && FrameThrottler.Check("TradeArtificialThrottle") && cancelTradeButton->IsEnabled && EzThrottler.Throttle("Delay", 200) && EzThrottler.Throttle("CancelTrade", 2000))
                    {
                        (*cancelTradeButton).ClickAddonButton(addon);
                    }
                }
            }

            IPCHelper.RemoveHonorificTitleTransactionOngoing();
        };

        InitializeConfig();
        GetConnectedPlayer();
        InitializeTimers();
    }

    public static void InitializeConfig()
    {
        Configuration = XIVATM_Plugin.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Save();
    }

    public static void InitializeTimers()
    {
        if (Configuration!.GlobalWithdrawnGilsPerTimerange != null)
        {
            string timerUniqueId = Guid.NewGuid().ToString();

            // If the target date time is in the past, the timer has already elapsed, but it doesnt matter because the callback will be called immediately
            DateTime targetDateTime = Configuration.GlobalWithdrawnGilsPerTimerange.TimeRangeDateTime.End;

            var newTimer = TimerCallbackHelper.CreateTimerWithCallbackWhenReachesTime(targetDateTime, () =>
            {
                Configuration.UpdateConfiguration(() => { Configuration.GlobalWithdrawnGilsPerTimerange = null; });

                // If the timer is in the list, dispose it and remove it from the list
                // This will prevent trying to dispose a timer that has not been created, because the target time was in the past
                if (TimersList.ContainsKey(timerUniqueId))
                {
                    TimersList[timerUniqueId].Dispose();
                    TimersList.Remove(timerUniqueId);
                }
            }, false);

            if (newTimer != null)
            {
                TimersList[timerUniqueId] = newTimer;
                Configuration.UpdateConfiguration(() => { Configuration.GlobalWithdrawnGilsPerTimerange.TimeRangeDateTime.TimerId = timerUniqueId; });
            }
            // Else statement is not needed because the callback will be called immediately if the target time is in the past
        }

        // Do not Foreach a List if you are going to modify it in the loop
        for (int i = 0; i < Configuration.WithdrawnGilsPerPlayerTimerange.Count; i++)
        {
            var playerTimerange = Configuration.WithdrawnGilsPerPlayerTimerange[i];

            string timerUniqueId = Guid.NewGuid().ToString();

            // If the target date time is in the past, the timer has already elapsed, but it doesnt matter because the callback will be called immediately
            DateTime targetDateTime = playerTimerange.WithdrawnGilsPerTimerange.TimeRangeDateTime.End;

            var newTimer = TimerCallbackHelper.CreateTimerWithCallbackWhenReachesTime(targetDateTime, () =>
            {
                Configuration.UpdateConfiguration(() => { Configuration.WithdrawnGilsPerPlayerTimerange.Remove(playerTimerange); });

                // If the timer is in the list, dispose it and remove it from the list
                // This will prevent trying to dispose a timer that has not been created, because the target time was in the past
                if (TimersList.ContainsKey(timerUniqueId))
                {
                    TimersList[timerUniqueId].Dispose();
                    TimersList.Remove(timerUniqueId);
                }
            }, false);

            if (newTimer != null)
            {
                TimersList[timerUniqueId] = newTimer;
                Configuration.UpdateConfiguration(() => { playerTimerange.WithdrawnGilsPerTimerange.TimeRangeDateTime.TimerId = timerUniqueId; });
            }
            // Else statement is not needed because the callback will be called immediately if the target time is in the past
        }
    }

    public async static void GetConnectedPlayer()
    {
        IPlayerCharacter? playerCharacter = await GetPlayerCharacterAsync();

        ConnectedPlayerObject = playerCharacter;

        if (playerCharacter != null)
            Configuration!.UpdateConfiguration(() => { Configuration.SeenCharacters[ECommons.GameHelpers.Player.CID] = ECommons.GameHelpers.Player.NameWithWorld; });

        var playerObject = PlayerHelper.MakePlayerFromPlayerCharacterObject(playerCharacter);

        if (playerObject == null) return;

        ConnectedPlayer = playerObject;
    }

    public static void ClearConnectedPlayer()
    {
        ConnectedPlayer = null;
        ConnectedPlayerObject = null;
    }

    public static void Dispose()
    {
        ClearConnectedPlayer();
        DisposeTimers();
        ClearTimerIdsFromConfiguration();
    }

    public static void DisposeTimers()
    {
        foreach (var timer in TimersList)
        {
            timer.Value.Dispose();
        }
        TimersList.Clear();
    }

    public static void ClearTimerIdsFromConfiguration()
    {
        Service.Configuration!.UpdateConfiguration(() =>
        {
            if (Configuration.GlobalWithdrawnGilsPerTimerange != null && Configuration.GlobalWithdrawnGilsPerTimerange.TimeRangeDateTime.TimerId != null)
                Configuration.GlobalWithdrawnGilsPerTimerange.TimeRangeDateTime.TimerId = null;

            foreach (var playerTimerange in Configuration.WithdrawnGilsPerPlayerTimerange)
            {
                if (playerTimerange.WithdrawnGilsPerTimerange.TimeRangeDateTime.TimerId != null)
                    playerTimerange.WithdrawnGilsPerTimerange.TimeRangeDateTime.TimerId = null;
            }
        });
    }

    public async static Task<IPlayerCharacter?> GetPlayerCharacterAsync() => await Service.Framework.RunOnFrameworkThread(() => Service.ClientState!.LocalPlayer);
}
