using System;

namespace XIVATM.Structs;

public class TimeRangeDateTime
{
    // A timestamp representing the start of the time range.
    public DateTime Start { get; set; }
    // A timestamp representing the end of the time range.
    public DateTime End { get; set; }
    public string? TimerId { get; set; }

    // Constructor for the TimeRangeDateTime class.
    public TimeRangeDateTime(DateTime start, DateTime end, string? timerId = null)
    {
        Start = start;
        End = end;
        TimerId = timerId;
    }
}
