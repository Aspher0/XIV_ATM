using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Bindings.ImGui;
using XIVATM.Helpers;

namespace XIVATM.UI.DebugTesting;

public static class DebugTestingUI
{
    public static void DrawDebugTestingUI()
    {
        if (ImGui.BeginChild("DebugTestingUI##MainUI"))
        {
            ImGui.Text($"Honorific available: {IPCHelper.IsHonorificAPIAvailable()}");

            if (ImGui.Button("Apply title"))
            {
                if (Service.ConnectedPlayer != null)
                    Service.HonorificIPC_Caller.SetTitle(Service.HonorificIPC_Caller.GetTitleData([ECommons.GameHelpers.Player.CID])[0].Title);
            }

            if (ImGui.Button("Reset Title"))
            {
                if (Service.ConnectedPlayer != null)
                    Service.HonorificIPC_Caller.SetTitle();
            }

            UIHelper.DrawGreenCheck();
            UIHelper.DrawRedCross();

            ImGui.Text("Target: ");
            ImGui.SameLine();

            if (Service.TargetManager.Target != null && Service.TargetManager.Target is IPlayerCharacter)
            {
                var player = PlayerHelper.MakePlayerFromPlayerCharacterObject(Service.TargetManager.Target as IPlayerCharacter);

                ImGui.Text(player!.PlayerName + "@" + player!.HomeWorld + "\nContent: " + player.ContentId + "\nAccount: " + player.AccountId);
            } else
            {
                ImGui.Text("Not a player.");
            }
        }

        ImGui.EndChild();
    }
}
