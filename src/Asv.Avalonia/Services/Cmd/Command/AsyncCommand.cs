namespace Asv.Avalonia;

public abstract class AsyncCommand : IAsyncCommand
{
    protected const string BaseId = "cmd";
    public abstract ICommandInfo Info { get; }
    public abstract bool CanExecute(
        IRoutable context,
        ICommandArg parameter,
        out IRoutable targetContext
    );

    public abstract ValueTask<ICommandArg?> Execute(
        IRoutable context,
        ICommandArg newValue,
        CancellationToken cancel = default
    );
    public IExportInfo Source => Info.Source;

    public override string ToString()
    {
        return $"{Info.Name}[{Info.Id}]";
    }
}
