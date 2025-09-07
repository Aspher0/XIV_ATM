using XIVATM.Helpers;

namespace XIVATM.Events;

public static class CharacterLogEvents
{
    public static void OnCharacterLogin()
    {
        LoggerHelper.Information("Character logged in.");

        Service.GetConnectedPlayer();

        IPCHelper.CheckShouldApplyAll();
    }

    public static void OnCharacterLogout(int type, int code)
    {
        LoggerHelper.Information("Character logged out.");

        Service.HonorificIPC_Caller.SetTitle();

        Service.ClearConnectedPlayer();
    }
}
