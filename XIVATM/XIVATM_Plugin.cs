using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using System.Collections.Generic;
using System;
using XIVATM.Helpers;
using XIVATM.Events;
using XIVATM.UI;
using ECommons;
using ECommons.DalamudServices;
using XIVATM.Handlers;
using System.Linq;
using ECommons.EzSharedDataManager;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ECommons.Throttlers;
using ECommons.Automation.UIInput;
using ECommons.Logging;
using ECommons.UIHelpers.AddonMasterImplementations;
using ECommons.Automation.LegacyTaskManager;

namespace XIVATM;

/// <summary>
/// The main plugin class for XIVATM.
/// </summary>
public sealed class XIVATM_Plugin : IDalamudPlugin
{
    /// <summary>
    /// Injects the plugin interface for Dalamud.
    /// </summary>
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    /// <summary>
    /// List of command names (aliases) and their descriptions.
    /// </summary>
    private readonly List<Tuple<string, string>> commandNames = [
        new Tuple<string, string>("/xivatm", "Opens XIV ATM."),
        new Tuple<string, string>("/xatm", "Alias of /xivatm."),
        new Tuple<string, string>("/atm", "Toggles the ATM mode on and off."),
    ];

    public readonly WindowSystem WindowSystem = new("XIVATM");
    private UIBuilder MainWindow { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XIVATM_Plugin"/> class.
    /// </summary>
    public XIVATM_Plugin()
    {
        ECommonsMain.Init(PluginInterface, this, Module.DalamudReflector);

        PluginInterface.Create<Service>();

        CommonHelper.AddToHistory($"Initializing plugin...");

        Service.Plugin = this;

        Service.InitializeService();

        MainWindow = new UIBuilder(this);
        WindowSystem.AddWindow(MainWindow);

        SetupUI();
        SetupCommands();
        EventsManager.RegisterAllEvents();

        if (Service.Configuration!.OpenPluginOnLoad) MainWindow.IsOpen = true;

        SetupAPIs();

        CommonHelper.AddToHistory($"Plugin initialized!");

        if (Service.Configuration!.PluginEnabled)
            CommonHelper.AddToHistory("Plugin was enabled last session, ATM Mode enabled.");
    }

    /// <summary>
    /// Sets up the APIs used by the plugin.
    /// </summary>
    private static void SetupAPIs()
    {
        // Ensure the correct behavior for moodles, without delaying the IPCHelper.CheckShouldApplyAll, Moodles throws an error.
        TaskManager taskManager = new();
        taskManager.EnqueueImmediate(() => { IPCHelper.CheckShouldApplyAll(true); });
    }

    /// <summary>
    /// Sets up the UI components of the plugin.
    /// </summary>
    private void SetupUI()
    {
        PluginInterface.UiBuilder.Draw += () => WindowSystem.Draw();
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        PluginInterface.UiBuilder.OpenConfigUi += OpenSettings;
    }

    /// <summary>
    /// Sets up the commands for the plugin.
    /// </summary>
    private void SetupCommands()
    {
        foreach (var CommandName in commandNames)
        {
            Service.CommandManager.AddHandler(CommandName.Item1, new CommandInfo(OnCommand)
            {
                HelpMessage = CommandName.Item2
            });
        }
    }

    /// <summary>
    /// Handles the commands issued to the plugin.
    /// </summary>
    /// <param name="command">The command issued.</param>
    /// <param name="args">The arguments for the command.</param>
    private void OnCommand(string command, string args)
    {
        string[] splitArgs = args.Split(' ');

        if (splitArgs.Length > 0)
        {
            // For a possible future, not yet planned
            if (new[] { "config", "c", "settings", "s" }.Any(x => x == splitArgs[0]))
            {
                MainWindow.IsOpen = true;
                return;
            }
        }

        if (command == "/atm")
        {
            CommandHandler.ToggleATMMode();
            return;
        }

        ToggleMainUI();
    }

    /// <summary>
    /// Called on Framework Update.
    /// </summary>
    public unsafe void Framework_Update(object framework)
    {
        if (Service.TaskManager.IsBusy)
        {
            HashSet<string> Data;

            if (EzSharedData.TryGet<HashSet<string>>("YesAlready.StopRequests", out Data))
            {
                Service.YesAlreadyStopRequired = true;
                Data.Add(Svc.PluginInterface.InternalName);
            }
        }
        else if (Service.YesAlreadyStopRequired)
        {
            HashSet<string> Data;

            if (EzSharedData.TryGet<HashSet<string>>("YesAlready.StopRequests", out Data))
                Data.Remove(Svc.PluginInterface.InternalName);

            Service.YesAlreadyStopRequired = false;
        }
        Service.IsActive[0] = Service.TaskManager.IsBusy;

        // If trade is not open, stop
        if (!Svc.Condition[ConditionFlag.TradeOpen])
            return;

        if (!TradeHelper.CanAutoAcceptTrade() && !Service.TaskManager.IsBusy)
            return;

        if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Trade", out var addon) && GenericHelpers.IsAddonReady(addon))
        {
            var check = addon->UldManager.NodeList[31]->GetAsAtkComponentNode()->Component->UldManager.NodeList[0]->GetAsAtkImageNode();
            var ready = check->AtkResNode.Color.A == 0xFF;

            var tradeButton = (AtkComponentButton*)(addon->UldManager.NodeList[3]->GetComponent());

            if (TradeHelper.IsActive)
                ready = TradeHelper.ConfirmAllowed;

            if (ready)
            {
                if (EzThrottler.Check("TradeArtificialThrottle") && FrameThrottler.Check("TradeArtificialThrottle") && tradeButton->IsEnabled && EzThrottler.Throttle("Delay", 200) && EzThrottler.Throttle("ReadyTrade", 2000))
                {
                    (*tradeButton).ClickAddonButton(addon);
                }
            }
            else
            {
                EzThrottler.Throttle("TradeArtificialThrottle", CommonHelper.GetDelayBetweenActions(), true);
                FrameThrottler.Throttle("TradeArtificialThrottle", 8, true);
            }
        }
        else
        {
            EzThrottler.Throttle("TradeArtificialThrottle", CommonHelper.GetDelayBetweenActions(), true);
            FrameThrottler.Throttle("TradeArtificialThrottle", 8, true);
        }

        var addonYesNo = TradeHelper.GetSpecificYesno(TradeHelper.TradeText);
        if (addonYesNo != null && EzThrottler.Throttle("Delay", 200) && EzThrottler.Throttle("SelectYes", 2000))
        {
            PluginLog.Information($"Confirming trade");
            new AddonMaster.SelectYesno(addonYesNo).Yes();
        }
    }

    /// <summary>
    /// Toggles the main UI of the plugin.
    /// </summary>
    public void ToggleMainUI() => MainWindow.Toggle();
    public void OpenSettings() => MainWindow.Toggle();

    /// <summary>
    /// Disposes the plugin and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        CommonHelper.AddToHistory($"Disposing plugin...");

        if (!Service.Configuration!.PluginEnabledPersistence)
            Service.Configuration.UpdateConfiguration(() => { Service.Configuration.PluginEnabled = false; });

        IPCHelper.Dispose();

        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();
        EventsManager.UnregisterAllEvents();

        Service.Dispose();

        foreach (var CommandName in commandNames)
        {
            Service.CommandManager.RemoveHandler(CommandName.Item1);
        }

        ECommonsMain.Dispose();

        CommonHelper.AddToHistory($"Plugin disposed.");
    }
}
