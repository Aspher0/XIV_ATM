namespace XIVATM.Structs;

public class ChecksBeforeTrade
{
    public bool IsValidGilAmountToSendFixed;
    public bool IsValidGilAmountToSendRandomRangeBoth;
    public bool IsValidGilAmountToSendRandomRangeLow;
    public bool IsValidDefaultGilAmountPlayerTimerange;
    public bool IsValidGlobalGilAmountTimerange;
    public bool IsValidGlobalGilAmountWithdrawable;
    public bool HasTriggerPhrases;
    public bool HasOneValidTriggerPhrase;
    public bool HasReachedTheGlobalLimitOfGils;
    public bool HasReachedTheGlobalLimitOfGilsPerTimerange;
    public bool HasPlayerReachedTheGlobalLimitOfGilsPerTimerange;

    public ChecksBeforeTrade(bool isValidGilAmountToSendFixed, bool isValidGilAmountToSendRandomRangeBoth, bool isValidGilAmountToSendRandomRangeLow,
        bool isValidDefaultGilAmountPlayerTimerange, bool isValidGlobalGilAmountTimerange, bool isValidGlobalGilAmountWithdrawable,
        bool hasTriggerPhrases, bool hasOneValidTriggerPhrase,
        bool hasReachedTheGlobalLimitOfGils, bool hasReachedTheGlobalLimitOfGilsPerTimerange, bool hasPlayerReachedTheGlobalLimitOfGilsPerTimerange)
    {
        IsValidGilAmountToSendFixed = isValidGilAmountToSendFixed;
        IsValidGilAmountToSendRandomRangeBoth = isValidGilAmountToSendRandomRangeBoth;
        IsValidGilAmountToSendRandomRangeLow = isValidGilAmountToSendRandomRangeLow;
        IsValidDefaultGilAmountPlayerTimerange = isValidDefaultGilAmountPlayerTimerange;
        IsValidGlobalGilAmountTimerange = isValidGlobalGilAmountTimerange;
        IsValidGlobalGilAmountWithdrawable = isValidGlobalGilAmountWithdrawable;
        HasTriggerPhrases = hasTriggerPhrases;
        HasOneValidTriggerPhrase = hasOneValidTriggerPhrase;
        HasReachedTheGlobalLimitOfGils = hasReachedTheGlobalLimitOfGils;
        HasReachedTheGlobalLimitOfGilsPerTimerange = hasReachedTheGlobalLimitOfGilsPerTimerange;
        HasPlayerReachedTheGlobalLimitOfGilsPerTimerange = hasPlayerReachedTheGlobalLimitOfGilsPerTimerange;
    }
}
