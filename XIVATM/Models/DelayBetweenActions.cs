namespace XIVATM.Structs;

public class DelayBetweenActions
{
    public DelayBetweenActionsMode DelayBetweenActionsMode { get; set; }
    public int FixedValueMilliseconds { get; set; }
    public int RangeLowValueMilliseconds { get; set; }
    public int RangeHighValueMilliseconds { get; set; }

    public DelayBetweenActions(DelayBetweenActionsMode delayBetweenActionsMode = DelayBetweenActionsMode.Fixed, int fixedValueMilliseconds = 0, int rangeLowValueMilliseconds = 0, int rangeHighValueMilliseconds = 0)
    {
        DelayBetweenActionsMode = delayBetweenActionsMode;
        FixedValueMilliseconds = fixedValueMilliseconds;
        RangeLowValueMilliseconds = rangeLowValueMilliseconds;
        RangeHighValueMilliseconds = rangeHighValueMilliseconds;
    }
}
