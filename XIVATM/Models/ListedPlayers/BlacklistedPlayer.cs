using System;

namespace XIVATM.Structs;

public class BlacklistedPlayer
{
    public string UniqueId { get; set; }
    public bool Enabled { get; set; }
    public ListedPlayer ListedPlayer { get; set; }

    public BlacklistedPlayer(ListedPlayer listedPlayer, bool enabled = true, string? uniqueId = null)
    {
        UniqueId = uniqueId ?? Guid.NewGuid().ToString();
        Enabled = enabled;
        ListedPlayer = listedPlayer;
    }
}
