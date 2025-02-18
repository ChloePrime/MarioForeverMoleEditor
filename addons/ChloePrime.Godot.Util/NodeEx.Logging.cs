#nullable enable
using System;
using System.Diagnostics;
using Godot;

namespace ChloePrime.Godot.Util;

public static partial class NodeEx
{
    private static readonly Action<object[]> EmptyDebugCall = _ => { };

    [StackTraceHidden]
    public static void Log(this Node self, string message)
    {
        FormatAndPrint(self, message, null, null, GD.Print, EmptyDebugCall);
    }
    
    [StackTraceHidden]
    public static void LogWarn(this Node self, string message, Exception? cause = null)
    {
        FormatAndPrint(self, message, "Warn", cause, GD.Print, GD.PushWarning);
    }

    [StackTraceHidden]
    public static void LogError(this Node self, string message, Exception? cause = null)
    {
        FormatAndPrint(self, message, "Error", cause, GD.PrintErr, GD.PushError);
    }

    [StackTraceHidden]
    public static void LogException(this Node self, Exception cause)
    {
        FormatAndPrint(self, "", "Error", cause, GD.PrintErr, GD.PushError);
    }

    [StackTraceHidden]
    private static void FormatAndPrint(
        this Node self, string? message, string? level, Exception? cause, 
        Action<string> printer, Action<object[]> debugger)
    {
        if (message != null)
        {
            string formatted = Format(self, message, level);
            printer(formatted);
            debugger(cause is not null ? [formatted, cause] : [formatted]);
        }
        else if (cause is not null)
        {
            debugger([cause]);
        }
    }

    private static string Format(Node node, string message, string? level = null)
    {
        var id = node.Multiplayer.GetUniqueId();
        var header = (id, level) switch
        {
            (1, null) => "[Server] ",
            (1, not null)  => $"[Server/{level}] ",
            (_, null) => $"[{id}] ",
            (_, not null)  => $"[{id}/{level}] ",
        };
        return header + message;
    }
}
