using System;
using System.Collections.Generic;
using XIVATM.Helpers;
using XIVATM.Structs;

namespace XIVATM.Events;

public static class EventsManager
{
    // For modularity in the future
    private static List<List<GlobalEvent>> EventsListsToProcess = new List<List<GlobalEvent>>
    {
        GlobalEventsList.MandatoryEvents
    };

    public static void RegisterAllEvents()
    {
        LoggerHelper.Information("Registering Global Events.");

        foreach (var list in EventsListsToProcess)
        {
            RegisterEventsList(list);
        }
    }

    public static void UnregisterAllEvents()
    {
        LoggerHelper.Information("Unregistering Global Events.");

        foreach (var list in EventsListsToProcess)
        {
            UnregisterEventsList(list);
        }
    }

    public static void RegisterEventsList(List<GlobalEvent> list)
    {
        foreach (var globalEvent in list)
        {
            try
            {
                globalEvent.Register.Invoke();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"Failed to register event: {ex.Message}");
            }
        }
    }

    public static void UnregisterEventsList(List<GlobalEvent> list)
    {
        foreach (var globalEvent in list)
        {
            try
            {
                globalEvent.Unregister.Invoke();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"Failed to unregister event: {ex.Message}");
            }
        }
    }
}
