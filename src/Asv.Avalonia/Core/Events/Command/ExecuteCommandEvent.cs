using System.Diagnostics;
using Asv.Common;

namespace Asv.Avalonia;

public class ExecuteCommandEvent(
    IRoutable source,
    string commandId,
    CommandArg commandArg,
    CancellationToken cancel = default
) : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble)
{
    public string CommandId { get; } = commandId;
    public CommandArg CommandArg { get; } = commandArg;

    public CancellationToken Cancel { get; } = cancel;
}

public static class ExecuteCommandEventMixin
{
    public static ValueTask ExecuteCommand(
        this IRoutable src,
        string commandId,
        CommandArg commandArg,
        CancellationToken cancel = default
    )
    {
        // this is only a safety check at debug to ensure that the command is executed from a valid context
        Debug.Assert(
            src.GetRoot() is IShell,
            "ExecuteCommand should be called from IShell or its children"
        );
        return src.Rise(new ExecuteCommandEvent(src, commandId, commandArg, cancel), cancel);
    }

    public static ValueTask ExecuteCommand(
        this IRoutable src,
        string commandId,
        CancellationToken cancel = default
    )
    {
        return src.Rise(new ExecuteCommandEvent(src, commandId, CommandArg.Empty, cancel), cancel);
    }
}
