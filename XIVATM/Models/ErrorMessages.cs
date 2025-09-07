namespace XIVATM.Structs;

public static class ErrorMessages
{
    public static readonly string InvalidGilAmountToSendFixedOrBothRangeInvalid = "The default amount of gils to send is set to 0. The ATM will not be able to send any gils to anyone";
    public static readonly string InvalidGilAmountToSendRangeLowInvalid = "The minimum amount of gils to send is set to 0. The ATM will not be able to send gils in a random range with a minimum of 0. It needs to be at least 1 gil";
    public static readonly string InvalidDefaultGilAmountPlayerTimerange = "The amount of gils a player can withdraw from you within a timerange is set to 0. The ATM will not be able to send any gils to anyone";
    public static readonly string InvalidGlobalGilAmountTimerange = "The amount of gils a player can withdraw from you within a timerange is set to 0. The ATM will not be able to send any gils to anyone";
    public static readonly string InvalidGlobalGilAmountWithdrawable = "The maximum amount of gils withdrawable from the ATM is set to 0. The ATM will not be able to send any gils to anyone";
    public static readonly string NoValidDefaultTriggerPhrases = "You have no default trigger phrases set, or all of them are invalid. The ATM will not trigger on any tells";
    public static readonly string HasReachedGlobalGilCapPerTimerange = "You have reached the maximum amount of gils that you can spend during the ongoing timerange";
    public static readonly string HasReachedGlobalGilCap = "You have reached the global maximum amount of gils that you can spend";
}
