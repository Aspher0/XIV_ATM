namespace XIVATM.Structs;

public class ListedPlayer
{
    public Player Player { get; set; }
    public ListedPlayerType ListedPlayerType { get; set; }
    public ListedType ListedType { get; set; }

    public ListedPlayer(Player player, ListedPlayerType listedPlayerType, ListedType listedType = ListedType.ServiceAccount, bool enabled = true)
    {
        Player = player;
        ListedPlayerType = listedPlayerType;
        ListedType = listedType;
    }
}
