namespace Asv.Avalonia;

public abstract class AsyncCommand : IAsyncCommand
{
    public const string BaseId = "cmd";
    public abstract ICommandInfo Info { get; }
    public abstract bool CanExecute(
        IViewModel context,
        CommandArg parameter,
        out IViewModel targetContext
    );

    public abstract ValueTask<CommandArg?> Execute(
        IViewModel context,
        CommandArg newValue,
        CancellationToken cancel = default
    );

    public override string ToString()
    {
        return $"{Info.Name}[{Info.Id}]";
    }
}
