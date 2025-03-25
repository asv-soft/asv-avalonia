namespace Asv.Avalonia;

public abstract class NoContextCommand : AsyncCommand
{
    public override bool CanExecute(
        IRoutable context,
        ICommandParameter parameter,
        out IRoutable targetContext
    )
    {
        targetContext = context;
        return true;
    }

    public override ValueTask<ICommandParameter?> Execute(
        IRoutable context,
        ICommandParameter newValue,
        CancellationToken cancel = default
    )
    {
        return InternalExecute(newValue, cancel);
    }

    protected abstract ValueTask<ICommandParameter?> InternalExecute(
        ICommandParameter newValue,
        CancellationToken cancel
    );
}
