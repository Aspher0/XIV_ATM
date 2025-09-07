namespace XIVATM.Structs;

public class GilAmountToSend
{
    public GilSenderMode GilSenderMode { get; set; }
    public int FixedAmount { get; set; }
    public int RangeLow { get; set; }
    public int RangeHigh { get; set; }

    public GilAmountToSend(GilSenderMode gilSenderMode = GilSenderMode.Fixed, int fixedAmount = 0, int rangeLow = 0, int rangeHigh = 0)
    {
        GilSenderMode = gilSenderMode;
        FixedAmount = fixedAmount;
        RangeLow = rangeLow;
        RangeHigh = rangeHigh;
    }
}
