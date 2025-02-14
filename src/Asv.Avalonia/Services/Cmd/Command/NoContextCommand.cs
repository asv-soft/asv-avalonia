namespace Asv.Avalonia;

public abstract class NoContextCommand : AsyncCommand
{
    public override bool CanExecute(
        IRoutable context,
        IPersistable parameter,
        out IRoutable targetContext
    )
    {
        targetContext = context;
        return true;
    }

    public override ValueTask<IPersistable?> Execute(
        IRoutable context,
        IPersistable newValue,
        CancellationToken cancel = default
    )
    {
        return InternalExecute(newValue, cancel);
    }

    protected abstract ValueTask<IPersistable?> InternalExecute(
        IPersistable newValue,
        CancellationToken cancel
    );
}
