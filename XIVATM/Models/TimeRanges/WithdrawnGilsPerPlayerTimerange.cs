using System;

namespace XIVATM.Structs;

public class WithdrawnGilsPerPlayerTimerange
{
    public string UniqueId { get; set; }
    public Player Player { get; set; }
    public WithdrawnGilsPerTimerange WithdrawnGilsPerTimerange { get; set; }

    public WithdrawnGilsPerPlayerTimerange(Player player, WithdrawnGilsPerTimerange withdrawnGilsPerTimerange, string? uniqueId = null)
    {
        Player = player;
        WithdrawnGilsPerTimerange = withdrawnGilsPerTimerange;
        UniqueId = uniqueId ?? Guid.NewGuid().ToString();
    }
}
