using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVATM.UI.Blacklist;

public static class BlacklistUI
{
    public static void DrawBlacklistUI()
    {
        ImGui.BeginChild("BlacklistUI##MainUI");

        BlacklistedPlayerSelector.DrawBlacklistedPlayerSelector();
        ImGui.SameLine();
        BlacklistedPlayerSettingsPannel.DrawBlacklistedPlayerSettingsPanel();
        BlacklistedPlayerActionBar.DrawBlacklistedPlayerActionBar();

        ImGui.EndChild();
    }
}
