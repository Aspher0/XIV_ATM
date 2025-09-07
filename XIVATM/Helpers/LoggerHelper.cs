using Dalamud.Game.Text;
using System;

namespace XIVATM.Helpers;

public static class LoggerHelper
{
    public static void LogError(this Exception e)
    {
        Service.Logger.Error($"[XIVATM] - {e.Message}\n{e.StackTrace ?? ""}");
    }

    public static void Verbose(string message)
    {
        Service.Logger.Verbose("[XIVATM] - " + message);
    }

    public static void DebugBuildLog(string message)
    {
#if DEBUG
        Service.Logger.Debug("[XIVATM] - " + message);
#endif
    }

    public static void Debug(string message)
    {
        Service.Logger.Debug("[XIVATM] - " + message);
    }

    public static void Information(string message)
    {
        Service.Logger.Information("[XIVATM] - " + message);
    }

    public static void Warning(string message)
    {
        Service.Logger.Warning("[XIVATM] - " + message);
    }

    public static void Error(string message)
    {
        Service.Logger.Error("[XIVATM] - " + message);
    }

    public static void Fatal(string message)
    {
        Service.Logger.Fatal("[XIVATM] - " + message);
    }

    public static void Print(string message, bool printPluginName = true)
    {
        Service.ChatGui.Print(printPluginName ? "[XIVATM] - " + message : message);
    }

    public static void Print(XivChatEntry chatEntry, bool printPluginName = true)
    {
        if (printPluginName)
            chatEntry.Message = "[XIVATM] - " + chatEntry.Message;

        Service.ChatGui.Print(chatEntry);
    }

    public static void PrintErrorChannel(string message, bool printPluginName = true)
    {
        XivChatEntry chat = new XivChatEntry();
        chat.Type = XivChatType.ErrorMessage;
        chat.Message = printPluginName ? "[XIVATM] - " + message : message;

        Service.ChatGui.Print(chat);
    }

    public static void PrintError(string message, bool printPluginName = true)
    {
        Service.ChatGui.Print((printPluginName ? "[XIVATM] - " : "") + "ERROR - " + message);
    }
}
