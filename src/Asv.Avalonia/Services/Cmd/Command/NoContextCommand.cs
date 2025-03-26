namespace Asv.Avalonia;

public abstract class NoContextCommand : AsyncCommand
{
    public override bool CanExecute(
        IRoutable context,
        ICommandArg parameter,
        out IRoutable targetContext
    )
    {
        targetContext = context;
        return true;
    }

    public override ValueTask<ICommandArg?> Execute(
        IRoutable context,
        ICommandArg newValue,
        CancellationToken cancel = default
    )
    {
        return InternalExecute(newValue, cancel);
    }

    protected abstract ValueTask<ICommandArg?> InternalExecute(
        ICommandArg newValue,
        CancellationToken cancel
    );
}
