using Dalamud.Utility;
using ECommons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using XIVATM.Models;
using XIVATM.Structs;

namespace XIVATM.Helpers;

public static class CommonHelper
{
    private static readonly char[] AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_".ToCharArray();

    public static string GenerateRandomString(int length = 50, bool unique = false)
    {
        length = (length <= 0) ? 50 : length;

        var random = new Random();
        var result = new StringBuilder(length);

        if (unique)
        {
            string uniquePrefix = Guid.NewGuid().ToString("N").Substring(0, 8);
            result.Append(uniquePrefix).Append("-");
        }

        for (int i = 0; i < length; i++)
        {
            result.Append(AllowedChars[random.Next(AllowedChars.Length)]);
        }

        return result.ToString();
    }

    public static bool RegExpMatch(string text, string regexp)
    {
        if (string.IsNullOrWhiteSpace(regexp))
            return true;

        try
        {
            return Regex.IsMatch(text, regexp, RegexOptions.IgnoreCase);
        }
        catch (Exception)
        {
            LoggerHelper.Error("Invalid RegEXP: " + regexp);
            return false;
        }
    }

    // Opens a URL in the default browser
    public static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            LoggerHelper.Error($"Failed to open url {url}: " + ex.Message);
        }
    }

    // Converts a timerange UI values to total seconds
    public static int GetTotalSecondsFromTimerangeValues(TimerangeValues timerangeValues)
    {
        int seconds = timerangeValues.Seconds;
        int minutes_in_second = timerangeValues.Minutes * 60;
        int hours_in_second = timerangeValues.Hours * 60 * 60;
        int days_in_second = timerangeValues.Days * 24 * 60 * 60;

        return seconds + minutes_in_second + hours_in_second + days_in_second;
    }

    // Creates a datetime that corresponds to the current time plus the total seconds to add
    public static DateTime MakeDateTimeFromNowToTimeInSeconds(int totalSecondsToAdd) => DateTimeOffset.Now.AddSeconds(totalSecondsToAdd).DateTime;

    // Returns the configured delay between actions
    public static int GetDelayBetweenActions()
    {
        if (Service.Configuration!.DelayBetweenActions.DelayBetweenActionsMode == DelayBetweenActionsMode.RandomRange)
            return new Random().Next(Service.Configuration.DelayBetweenActions.RangeLowValueMilliseconds, Service.Configuration.DelayBetweenActions.RangeHighValueMilliseconds);
        else
            return Service.Configuration.DelayBetweenActions.FixedValueMilliseconds;
    }

    // If outTimer is null, it means that the callback has already happened, because the target time was in the past
    public static (WithdrawnGilsPerTimerange timerange, Timer? outTimer) MakeNewGlobalWithdrawnGilsPerTimerange(int withdrawnGils, bool hasTimer, string? timerUniqueId, Action? timerCallback, bool shouldAutomaticallyDisposeTimer = true)
    {
        // Make new TimeRangeDateTime base off of Service.Configuration.DefaultGlobalMaxGilsPerTimerange.TimeRange values
        int total_seconds = CommonHelper.GetTotalSecondsFromTimerangeValues(Service.Configuration!.DefaultGlobalMaxGilsPerTimerange.TimeRange);
        DateTime targetDateTime = CommonHelper.MakeDateTimeFromNowToTimeInSeconds(total_seconds);

        TimeRangeDateTime newTimeRangeDateTime = new(DateTimeOffset.Now.DateTime, targetDateTime, timerUniqueId);
        WithdrawnGilsPerTimerange newWithdrawnGilsPerTimerange = new(newTimeRangeDateTime, withdrawnGils);

        return (newWithdrawnGilsPerTimerange, hasTimer ? TimerCallbackHelper.CreateTimerWithCallbackWhenReachesTime(targetDateTime, timerCallback ?? (() => { }), shouldAutomaticallyDisposeTimer) : null);
    }

    // If outTimer is null, it means that the callback has already happened, because the target time was in the past
    public static (WithdrawnGilsPerPlayerTimerange playerTimerange, Timer? outTimer) MakeNewWithdrawnGilsPerPlayerTimerange(Player player, int withdrawnGils, bool hasTimer, string? timerUniqueId, Action? timerCallback, bool shouldAutomaticallyDisposeTimer = true)
    {
        // Make new TimeRangeDateTime base off of Service.Configuration!.DefaultMaxGilsPerPlayerTimerange.TimeRange values
        int total_seconds = CommonHelper.GetTotalSecondsFromTimerangeValues(Service.Configuration!.DefaultMaxGilsPerPlayerTimerange.TimeRange);
        DateTime targetDateTime = CommonHelper.MakeDateTimeFromNowToTimeInSeconds(total_seconds);

        TimeRangeDateTime newTimeRangeDateTime = new(DateTimeOffset.Now.DateTime, targetDateTime, timerUniqueId);
        WithdrawnGilsPerTimerange newWithdrawnGilsPerTimerange = new(newTimeRangeDateTime, withdrawnGils);
        WithdrawnGilsPerPlayerTimerange newWithdrawnGilsPerPlayerTimerange = new(player, newWithdrawnGilsPerTimerange);

        return (newWithdrawnGilsPerPlayerTimerange, hasTimer ? TimerCallbackHelper.CreateTimerWithCallbackWhenReachesTime(targetDateTime, timerCallback ?? (() => { }), shouldAutomaticallyDisposeTimer) : null);
    }

    public static void AddToHistory(string message) => Service.HistoryEntriesList.Add(new HistoryEntry(message));

    // From DynamicBridge/Utils.cs
    public static List<PathInfo> BuildPathes(List<string> rawPathes)
    {
        var ret = new List<PathInfo>();
        try
        {
            var pathes = TrimPathes(rawPathes);
            pathes.Sort();
            foreach (var x in pathes)
            {
                var parts = x.Split('/');
                if (parts.Length == 0) continue;
                for (var i = 1; i <= parts.Length; i++)
                {
                    var part = parts[..i].Join("/");
                    var info = new PathInfo(part, i - 1);
                    if (!ret.Contains(info)) ret.Add(info);
                }
            }
        }
        catch (Exception e)
        {
            e.LogError();
        }
        return ret;
    }

    // From DynamicBridge/Utils.cs
    public static List<string> TrimPathes(IEnumerable<string> origPathes)
    {
        var pathes = new List<string>();
        foreach (var path in origPathes)
        {
            var nameParts = path.Split('/');
            if (nameParts.Length > 1)
            {
                var pathParts = nameParts[..^1];
                pathes.Add(pathParts.Join("/"));
            }
        }
        return pathes;
    }
}
