using System;

namespace XIVATM.Structs;

public class WhitelistedPlayer
{
    public string UniqueId { get; set; }
    public ListedPlayer ListedPlayer { get; set; }

    public WhitelistedPlayer(ListedPlayer listedPlayer, string? uniqueId = null)
    {
        UniqueId = uniqueId ?? Guid.NewGuid().ToString();
        ListedPlayer = listedPlayer;
    }
}
