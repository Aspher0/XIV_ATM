namespace XIVATM.Structs;

public class TimerangeValues
{
    public int Days { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }

    public TimerangeValues(int days = 0, int hours = 0, int minutes = 0, int seconds = 0)
    {
        Days = days;
        Hours = hours;
        Minutes = minutes;
        Seconds = seconds;
    }
}
