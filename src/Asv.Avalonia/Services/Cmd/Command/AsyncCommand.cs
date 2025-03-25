namespace Asv.Avalonia;

public abstract class AsyncCommand : IAsyncCommand
{
    protected const string BaseId = "cmd";
    public abstract ICommandInfo Info { get; }
    public abstract bool CanExecute(
        IRoutable context,
        ICommandParameter parameter,
        out IRoutable targetContext
    );

    public abstract ValueTask<ICommandParameter?> Execute(
        IRoutable context,
        ICommandParameter newValue,
        CancellationToken cancel = default
    );
    public IExportInfo Source => Info.Source;

    public override string ToString()
    {
        return $"{Info.Name}[{Info.Id}]";
    }
}
