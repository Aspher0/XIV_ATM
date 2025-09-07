namespace XIVATM.Structs;

public class MaxGilsPerTimerange
{
    public int MaxGils { get; set; }
    public TimerangeValues TimeRange { get; set; }

    public MaxGilsPerTimerange(int maxGils = -1, TimerangeValues? timeRange = null)
    {
        MaxGils = maxGils;
        TimeRange = timeRange ?? new TimerangeValues();
    }
}
