using Dalamud.Plugin.Services;
using System.Collections.Generic;
using XIVATM.Events;
using XIVATM.Helpers;

namespace XIVATM.Structs;

internal class GlobalEventsList
{
    public static List<GlobalEvent> MandatoryEvents = new List<GlobalEvent> // The list of events
    {
        // Login and Logout game events
        new GlobalEvent(
            () => Service.ClientState.Login += CharacterLogEvents.OnCharacterLogin,
            () => Service.ClientState.Login -= CharacterLogEvents.OnCharacterLogin
        ),
        new GlobalEvent(
            () => Service.ClientState.Logout += CharacterLogEvents.OnCharacterLogout,
            () => Service.ClientState.Logout -= CharacterLogEvents.OnCharacterLogout
        ),
        // OnChatMessage event
        new GlobalEvent(
            () => Service.ChatGui.ChatMessage += ChatEvents.OnChatMessage,
            () => Service.ChatGui.ChatMessage -= ChatEvents.OnChatMessage
        ),
        // OnFrameworkUpdate event
        new GlobalEvent(
            () => Service.Framework.Update += Service.Plugin.Framework_Update,
            () => Service.Framework.Update -= Service.Plugin.Framework_Update
        ),
    };
}

