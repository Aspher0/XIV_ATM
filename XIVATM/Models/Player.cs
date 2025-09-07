namespace XIVATM.Structs;

public class Player
{
    public string PlayerName; // The paired player first and last name
    public string HomeWorld; // The paired player world
    public ulong? AccountId; // The paired player account id
    public ulong? ContentId; // The paired player content id

    public Player(string playerName, string homeWorld, ulong? accountId = null, ulong? contentId = null)
    {
        PlayerName = playerName;
        HomeWorld = homeWorld;
        AccountId = accountId;
        ContentId = contentId;
    }

    public string GetPlayerNameWorld() => $"{PlayerName}@{HomeWorld}";
}
