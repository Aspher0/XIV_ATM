namespace XIVATM.Structs;

public class WithdrawnGilsPerTimerange
{
    public TimeRangeDateTime TimeRangeDateTime { get; set; }
    public int WithdrawnGils { get; set; }

    public WithdrawnGilsPerTimerange(TimeRangeDateTime timeRangeDateTime, int globalWithdrawnGils = 0)
    {
        TimeRangeDateTime = timeRangeDateTime;
        WithdrawnGils = globalWithdrawnGils;
    }
}
