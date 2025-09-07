using ECommons.ExcelServices;
using ECommons.EzIpcManager;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using XIVATM.Helpers;
using ECommons;

namespace XIVATM.IPC.Honorific;

// From DynamicBridge/IPC/Honorific/HonorificManager.cs

public class HonorificIPC_Caller
{
    [EzIPC] private readonly Func<string, uint, TitleData[]> GetCharacterTitleList;
    [EzIPC] private readonly Action<int> ClearCharacterTitle;
    [EzIPC] private readonly Action<int, string> SetCharacterTitle;

    public HonorificIPC_Caller()
    {
        EzIPC.Init(this, "Honorific", SafeWrapper.AnyException);
    }

    public bool WasSet = false;
    public List<TitleData> GetTitleData(IEnumerable<ulong> CIDs)
    {
        try
        {
            List<TitleData> ret = [];
            CIDs ??= Service.Configuration!.SeenCharacters.Keys;
            foreach (var c in CIDs)
            {
                var nameWithWorld = PlayerHelper.GetCharaNameFromCID(c);
                if (nameWithWorld != null)
                {
                    var parts = nameWithWorld.Split("@");
                    if (parts.Length == 2)
                    {
                        var name = parts[0];
                        var world = ExcelWorldHelper.Get(parts[1]);
                        ret.AddRange(GetCharacterTitleList(name, world?.RowId ?? 0) ?? []);
                    }
                }
            }
            return ret;
        }
        catch (Exception e)
        {
            e.LogError();
            return [];
        }
    }

    public void SetTitle(string? title = null)
    {
        try
        {
            if (GenericHelpers.IsNullOrEmpty(title))
            {
                WasSet = false;
                ClearCharacterTitle(ECommons.GameHelpers.Player.Object.ObjectIndex);
            }
            else
            {
                WasSet = true;
                if (GetTitleData(Service.Configuration!.ShowHonorificTitlesFromAllCharacters ? null : [ECommons.GameHelpers.Player.CID]).TryGetFirst(x => x.Title == title, out var t))
                {
                    SetCharacterTitle(ECommons.GameHelpers.Player.Object.ObjectIndex, JsonConvert.SerializeObject(t));
                }
                else
                {
                    throw new KeyNotFoundException($"Could not find title preset {title}");
                }
            }
        }
        catch (Exception e)
        {
            e.LogError();
        }
    }

    public static void Dispose()
    {
        if (Service.ConnectedPlayer != null)
            Service.HonorificIPC_Caller.SetTitle();
    }
}
