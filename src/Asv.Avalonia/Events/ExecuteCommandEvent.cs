﻿using System.Diagnostics;

namespace Asv.Avalonia;

public class ExecuteCommandEvent(IRoutable source, string commandId, CommandArg commandArg)
    : AsyncRoutedEvent(source, RoutingStrategy.Bubble)
{
    public string CommandId { get; } = commandId;
    public CommandArg CommandArg { get; } = commandArg;
}

public static class ExecuteCommandEventMixin
{
    public static ValueTask ExecuteCommand(
        this IRoutable src,
        string commandId,
        CommandArg commandArg
    )
    {
        // this is only a safety check at debug to ensure that the command is executed from a valid context
        Debug.Assert(
            src.GetRoot() is IShell,
            "ExecuteCommand should be called from IShell or its children"
        );
        return src.Rise(new ExecuteCommandEvent(src, commandId, commandArg));
    }

    public static ValueTask ExecuteCommand(this IRoutable src, string commandId)
    {
        return src.Rise(new ExecuteCommandEvent(src, commandId, CommandArg.Empty));
    }
}
