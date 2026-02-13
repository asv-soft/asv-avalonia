namespace Asv.Avalonia;

public abstract class AsyncCommand : IAsyncCommand
{
    public const string BaseId = "cmd";
    public abstract ICommandInfo Info { get; }
    public abstract bool CanExecute(
        IRoutable context,
        CommandArg parameter,
        out IRoutable targetContext
    );

    public abstract ValueTask<CommandArg?> Execute(
        IRoutable context,
        CommandArg newValue,
        CancellationToken cancel = default
    );
    public IExportInfo Source => Info.Source;

    public override string ToString()
    {
        return $"{Info.Name}[{Info.Id}]";
    }
}
