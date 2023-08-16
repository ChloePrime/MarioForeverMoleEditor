#nullable enable
using System;
using System.Diagnostics;
using Godot;

namespace MixelTools.Util.Extensions;

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
            debugger(cause is not null ? new object[] { formatted, cause } : new object[] { formatted });
        }
        else if (cause is not null)
        {
            debugger(new object[] { cause });
        }
    }

    private static string Format(Node node, string message, string? level = null)
    {
        int id = node.Multiplayer.GetUniqueId();
        string header = (id, level) switch
        {
            (1, null) => "[Server] ",
            (1, { })  => $"[Server/{level}] ",
            (_, null) => $"[{id}] ",
            (_, { })  => $"[{id}/{level}] ",
        };
        return header + message;
    }
}
