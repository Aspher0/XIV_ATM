using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using XIVATM.IPC.Honorific;
using XIVATM.Models;
using XIVATM.Structs;

namespace XIVATM;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool OpenPluginOnLoad { get; set; } = false;

    public bool PluginEnabled { get; set; } = false;
    // Determines whether the plugin should save its enabled state between sessions
    public bool PluginEnabledPersistence { get; set; } = false;
    public bool WhitelistEnabled { get; set; } = false;
    public bool BlacklistEnabled { get; set; } = false;

    // The default amount of gils that will be sent when someone uses the ATM player
    // Can be a fixed amount or a random amount between a range
    public GilAmountToSend DefaultGilAmountToSend { get; set; } = new();
    // The message that will be sent to the player when a transaction is made
    // Sending can be disabled
    public MessageToSend DefaultMessageToSendAfterTransaction { get; set; } = new();

    // The default amount of gils that can be sent to a specific person within a certain time range
    public MaxGilsPerTimerange DefaultMaxGilsPerPlayerTimerange { get; set; } = new();
    // A list of ongoing timeranges of the amount of gils that can be sent to a specific person in a period of time
    // If a player isn't inside, it should mean they have not been sent gils to yet, and therefore the timerange has not started for them
    public List<WithdrawnGilsPerPlayerTimerange> WithdrawnGilsPerPlayerTimerange { get; set; } = new();
    // The message that will be sent to the player when they try to retrieve more gils than allowed within a certain time range
    // Sending can be disabled
    public MessageToSend DefaultMessageToSendForMaxGilsPerPlayerTimerange { get; set; } = new();

    // The default amount of gils that can be sent in total, globally, within a certain time range
    public MaxGilsPerTimerange DefaultGlobalMaxGilsPerTimerange { get; set; } = new();

    // The ongoing timerange of the global amount of gils that can be spent in a period of time
    // null means that no gils have been spent yet, and therefore the timerange has not started
    public WithdrawnGilsPerTimerange? GlobalWithdrawnGilsPerTimerange { get; set; } = null;
    // The message that will be sent to the player when they try to retrieve more gils than allowed globally within a certain time range
    // Sending can be disabled
    public MessageToSend DefaultMessageToSendForGlobalMaxGilsPerTimerange { get; set; } = new();

    // The amount of gils that can be sent globally before the plugin stops allowing gils sending (bankrupt protection)
    // Can be reset by the user with the press of a button
    public int GlobalMaxGils { get; set; } = -1;
    // The track of the amount of gils that have been sent globally since the last reset
    public int GlobalGilsSent { get; set; } = 0;
    // The message that will be sent to the player when they try to retrieve more gils than allowed globally
    // Sending can be disabled
    public MessageToSend DefaultMessageToSendForGlobalMaxGils { get; set; } = new();

    // A list of TriggerPhrase objects that will be used to trigger gil sending by the ATM player
    public List<TriggerPhrase> DefaultTriggerPhrases { get; set; } = new();

    // The delay in milliseconds between actions (Trading, sending gils, etc.)
    // Can be a fixed amount or a random amount between a range
    public DelayBetweenActions DelayBetweenActions { get; set; } = new();

    // A list of WhitelistedPlayer objects that will be used to prevent gil sending to specific players
    public List<BlacklistedPlayer> BlacklistedPlayers { get; set; } = new();
    // A list of ListedPlayer objects that will be used to allow gil sending to specific players
    public List<WhitelistedPlayer> WhitelistedPlayers { get; set; } = new();

    // Statistics about the plugin and its usage
    public Statistics Statistics { get; set; } = new();

    // A list of characters that has been loged in to
    public Dictionary<ulong, string> SeenCharacters = [];

    // Enables the honorific title integration when the ATM Mode is enabled
    public bool ApplyHonorificTitleOnATMModeEnabled { get; set; } = false;
    // The Title to display when ATM mode is enabled
    public TitleData? HonorificTitleOnATMModeEnabled { get; set; } = null;

    // Enables the honorific title integration when a transaction is ongoing
    public bool ApplyHonorificTitleOnTransactionOngoing { get; set; } = false;
    // The Title to display when ATM mode is enabled
    public TitleData? HonorificTitleOnTransactionOngoing { get; set; } = null;

    // Allow the plugin to fetch Honorific Titles for other characters
    public bool ShowHonorificTitlesFromAllCharacters { get; set; } = false;

    // Enables the Moodles integration when the ATM Mode is enabled
    public bool ApplyMoodlesOnATMModeEnabled { get; set; } = false;
    // The Moodles to apply when ATM mode is enabled
    public List<MoodlesMoodleInfo> MoodlesOnATMModeEnabled { get; set; } = [];
    public List<MoodlesPresetInfo> MoodlesPresetsOnATMModeEnabled { get; set; } = [];

    public void UpdateConfiguration(Action updateAction)
    {
        updateAction();
        Save();
    }

    public void Save()
    {
        XIVATM_Plugin.PluginInterface.SavePluginConfig(this);
    }
}
