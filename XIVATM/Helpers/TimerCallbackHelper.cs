using System;
using System.Threading;

namespace XIVATM.Helpers;

public static class TimerCallbackHelper
{
    public static Timer? CreateTimerWithCallbackWhenReachesTime(DateTime targetTime, Action callback, bool shouldAutomaticallyDisposeTimer = true)
    {
        // Calculate the time span between now and the target time
        TimeSpan timeToWait = targetTime - DateTime.Now;

        // If the target time is in the past, invoke the callback immediately
        if (timeToWait <= TimeSpan.Zero)
        {
            callback();
            return null;
        }

        // Declare the timer variable
        Timer? timer = null;

        // Create a timer that will call the callback when the time span elapses
        timer = new Timer(_ =>
        {
            callback();
            // Dispose the timer after the callback is executed
            if (shouldAutomaticallyDisposeTimer)
                timer?.Dispose();
        }, null, timeToWait, Timeout.InfiniteTimeSpan);

        return timer;
    }
}
