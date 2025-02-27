namespace Asv.Avalonia;

public abstract class AsyncCommand : IAsyncCommand
{
    protected const string BaseId = "cmd";
    public abstract ICommandInfo Info { get; }
    public abstract bool CanExecute(
        IRoutable context,
        IPersistable parameter,
        out IRoutable targetContext
    );

    public abstract ValueTask<IPersistable?> Execute(
        IRoutable context,
        IPersistable newValue,
        CancellationToken cancel = default
    );
    public IExportInfo Source => Info.Source;
}
