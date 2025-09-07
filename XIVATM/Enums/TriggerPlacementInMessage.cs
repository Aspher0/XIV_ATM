namespace XIVATM.Structs;

public enum TriggerPlacementInMessage
{
    StartWith, // Message must start with the trigger
    Contain, // Message must contain the trigger
    EndWith, // Message must end with the trigger
}
