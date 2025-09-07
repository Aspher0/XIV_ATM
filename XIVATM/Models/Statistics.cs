using System.Collections.Generic;

namespace XIVATM.Structs;

public class Statistics
{
    public int TotalGilsSent { get; set; }
    public int GilsSentThisSession { get; set; }
    public int TotalGilsSentSinceGlobalLimitReset { get; set; }
    public int TotalTransactionsMade { get; set; }
    public List<Player> ListOfPlayersWhoUsedTheATM { get; set; }

    public Statistics(int totalGilsSent = 0, int gilsSentThisSession = 0, int totalGilsSentSinceGlobalLimitReset = 0, int totalTransactionsMade = 0, List<Player>? listOfPlayersWhoUsedTheATM = null)
    {
        TotalGilsSent = totalGilsSent;
        GilsSentThisSession = gilsSentThisSession;
        TotalGilsSentSinceGlobalLimitReset = totalGilsSentSinceGlobalLimitReset;
        TotalTransactionsMade = totalTransactionsMade;
        ListOfPlayersWhoUsedTheATM = listOfPlayersWhoUsedTheATM ?? new();
    }
}
