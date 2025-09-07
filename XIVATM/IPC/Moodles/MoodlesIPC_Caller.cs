using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;
using XIVATM.Helpers;
using XIVATM.Models;

namespace XIVATM.IPC.Moodles;

// From DynamicBridge/IPC/Moodles/MoodlesManager.cs

public class MoodlesIPC_Caller
{
    [EzIPC] private readonly Func<List<MoodlesMoodleInfo>> GetRegisteredMoodles;
    [EzIPC] private readonly Func<List<MoodlesPresetInfo>> GetRegisteredProfiles;
    [EzIPC] private readonly Action<Guid, IPlayerCharacter> AddOrUpdateMoodleByGUID;
    [EzIPC] private readonly Action<Guid, IPlayerCharacter> ApplyPresetByGUID;
    [EzIPC] private readonly Action<Guid, IPlayerCharacter> RemoveMoodleByGUID;
    [EzIPC] private readonly Action<Guid, IPlayerCharacter> RemovePresetByGUID;

    public MoodlesIPC_Caller()
    {
        EzIPC.Init(this, "Moodles");
    }

    private List<PathInfo>? PathInfos = null;
    public List<PathInfo> GetCombinedPathes()
    {
        PathInfos ??= CommonHelper.BuildPathes(GetRawPathes());
        return PathInfos;
    }

    public List<string> GetRawPathes()
    {
        var ret = new List<string>();
        try
        {
            foreach (var x in GetMoodles())
            {
                var path = x.FullPath;
                if (path != null)
                {
                    ret.Add(path);
                }
            }
            foreach (var x in GetPresets())
            {
                var path = x.FullPath;
                if (path != null)
                {
                    ret.Add(path);
                }
            }
        }
        catch (Exception e)
        {
            e.LogError();
        }
        return ret;
    }

    public List<MoodlesMoodleInfo> GetMoodles()
    {
        try
        {
            return GetRegisteredMoodles();
        }
        catch (Exception e)
        {
            e.LogError();
        }

        return [];
    }

    public List<MoodlesPresetInfo> GetPresets()
    {
        try
        {
            return GetRegisteredProfiles();
        }
        catch (Exception e)
        {
            e.LogError();
        }

        return [];
    }

    public void ApplyMoodle(Guid guid)
    {
        try
        {
            AddOrUpdateMoodleByGUID(guid, ECommons.GameHelpers.Player.Object);
        }
        catch (Exception e)
        {
            e.LogError();
        }
    }

    public void RemoveMoodle(Guid guid)
    {
        try
        {
            RemoveMoodleByGUID(guid, ECommons.GameHelpers.Player.Object);
        }
        catch (Exception e)
        {
            e.LogError();
        }
    }

    public void ApplyPreset(Guid guid)
    {
        try
        {
            ApplyPresetByGUID(guid, ECommons.GameHelpers.Player.Object);
        }
        catch (Exception e)
        {
            e.LogError();
        }
    }

    public void RemovePreset(Guid guid)
    {
        try
        {
            RemovePresetByGUID(guid, ECommons.GameHelpers.Player.Object);
        }
        catch (Exception e)
        {
            e.LogError();
        }
    }

    public static void Dispose()
    {
        if (Service.ConnectedPlayer != null && Service.Configuration!.ApplyMoodlesOnATMModeEnabled && Service.Configuration.PluginEnabled)
            IPCHelper.RemoveAllAppliedMoodles();
    }
}
