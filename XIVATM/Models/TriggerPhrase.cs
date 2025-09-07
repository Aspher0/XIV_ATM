using System;
using XIVATM.Structs;

namespace XIVATM.Models;

public class TriggerPhrase
{
    public string UniqueId { get; set; }
    public string Phrase { get; set; }
    public TriggerPlacementInMessage PlacementInMessage { get; set; }
    public TriggerCaseSensitivity CaseSensitivity { get; set; }

    public TriggerPhrase(string phrase = "",
                         TriggerPlacementInMessage placementInMessage = TriggerPlacementInMessage.Contain,
                         TriggerCaseSensitivity caseSensitivity = TriggerCaseSensitivity.CaseInsensitively,
                         string? uniqueId = null)
    {
        Phrase = phrase;
        PlacementInMessage = placementInMessage;
        CaseSensitivity = caseSensitivity;
        UniqueId = uniqueId ?? Guid.NewGuid().ToString();
    }
}
