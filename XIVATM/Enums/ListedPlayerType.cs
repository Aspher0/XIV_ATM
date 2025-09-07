namespace XIVATM.Structs;

// Determines if the player has been added manually by the user (With their name and homeworld, no AccountId or ContentId) or from a targeted player (Accurate, with Ids)
public enum ListedPlayerType
{
    AddedFromTarget,
    ManuallyAdded,
}
