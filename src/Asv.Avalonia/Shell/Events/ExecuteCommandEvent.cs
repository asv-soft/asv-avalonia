namespace Asv.Avalonia;

public class ExecuteCommandEvent(IRoutable source, string commandId, IPersistable? commandParameter)
    : AsyncRoutedEvent(source, RoutingEventStrategy.Bubble)
{
    public string CommandId { get; } = commandId;
    public IPersistable? CommandParameter { get; } = commandParameter;
}

public class FocusedEvent(IRoutable source)
    : AsyncRoutedEvent(source, RoutingEventStrategy.Bubble) { }

public static class ExecuteCommandEventMixin
{
    public static ValueTask ExecuteCommand(
        this IRoutable src,
        string commandId,
        IPersistable? commandParameter
    )
    {
        return src.Rise(new ExecuteCommandEvent(src, commandId, commandParameter));
    }
}
