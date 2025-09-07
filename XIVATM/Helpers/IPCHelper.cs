using System;
using XIVATM.IPC.Honorific;
using XIVATM.IPC.Moodles;
using XIVATM.Structs;

namespace XIVATM.Helpers;

public static class IPCHelper
{
    public static bool IsHonorificAPIAvailable() => PluginInterfaceHelper.IsPluginAvailable("Honorific", "1.4.2.0") == PluginAvailability.Available;
    public static bool IsMoodlesAPIAvailable() => PluginInterfaceHelper.IsPluginAvailable("Moodles", "1.0.0.15") == PluginAvailability.Available;

    public static void RemoveHonorificTitleTransactionOngoing()
    {
        if (!IsHonorificAPIAvailable())
            return;

        if (!Service.Configuration!.ApplyHonorificTitleOnTransactionOngoing) return;

        if (Service.ConnectedPlayer == null) return;

        if (!Service.Configuration.PluginEnabled || !Service.Configuration!.ApplyHonorificTitleOnATMModeEnabled || Service.Configuration!.HonorificTitleOnATMModeEnabled == null)
        {
            Service.HonorificIPC_Caller.SetTitle();
        } else
        {
            CheckShouldApplyHonorificTitle();
        }
    }

    public static void CheckShouldApplyHonorificTitle()
    {
        if (!IsHonorificAPIAvailable())
            return;

        if (!Service.Configuration!.ApplyHonorificTitleOnATMModeEnabled) return;

        if (Service.ConnectedPlayer == null) return;

        if (Service.Configuration.PluginEnabled)
        {
            // ATM Mode Enabled
            if (Service.Configuration.HonorificTitleOnATMModeEnabled != null)
                Service.HonorificIPC_Caller.SetTitle(Service.Configuration.HonorificTitleOnATMModeEnabled.Title);
            else
                Service.HonorificIPC_Caller.SetTitle();
        }
        else
        {
            // ATM Mode Disabled
            Service.HonorificIPC_Caller.SetTitle();
        }
    }

    public static void RemoveAllAppliedMoodles()
    {
        try
        {
            foreach (var moodle in Service.Configuration!.MoodlesOnATMModeEnabled)
            {
                Service.MoodlesIPC_Caller.RemoveMoodle(moodle.ID);
            }

            foreach (var preset in Service.Configuration!.MoodlesPresetsOnATMModeEnabled)
            {
                Service.MoodlesIPC_Caller.RemovePreset(preset.ID);
            }
        }
        catch (Exception e)
        {
            e.LogError();
        }
    }
    
    public static void ApplyAllMoodles()
    {
        try
        {
            foreach (var moodle in Service.Configuration!.MoodlesOnATMModeEnabled)
            {
                Service.MoodlesIPC_Caller.ApplyMoodle(moodle.ID);
            }

            foreach (var preset in Service.Configuration!.MoodlesPresetsOnATMModeEnabled)
            {
                Service.MoodlesIPC_Caller.ApplyPreset(preset.ID);
            }
        }
        catch (Exception e)
        {
            e.LogError();
        }
    }

    public static void CheckShouldApplyMoodles(bool isPluginLoad = false)
    {
        if (!IsMoodlesAPIAvailable())
            return;

        if (!Service.Configuration!.ApplyMoodlesOnATMModeEnabled) return;

        if (Service.ConnectedPlayer == null) return;

        if (Service.Configuration.PluginEnabled) // ATM Mode Enabled
            ApplyAllMoodles();
        else // ATM Mode Disabled
            if (!isPluginLoad)
                RemoveAllAppliedMoodles();
    }

    public static void CheckShouldApplyAll(bool isPluginLoad = false)
    {
        CheckShouldApplyHonorificTitle();
        CheckShouldApplyMoodles(isPluginLoad);
    }

    public static void Dispose()
    {
        if (IsHonorificAPIAvailable())
        {
            HonorificIPC_Caller.Dispose();
        }

        if (IsMoodlesAPIAvailable())
        {
            MoodlesIPC_Caller.Dispose();
        }
    }
}
