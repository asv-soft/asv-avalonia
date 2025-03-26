namespace Asv.Avalonia;

public abstract class ContextCommand<TContext> : AsyncCommand
    where TContext : IRoutable
{
    public override bool CanExecute(
        IRoutable context,
        ICommandArg parameter,
        out IRoutable targetContext
    )
    {
        var target = context.FindParentOfType<TContext>();
        if (target != null)
        {
            targetContext = target;
            return true;
        }

        targetContext = context;
        return false;
    }

    public override ValueTask<ICommandArg?> Execute(
        IRoutable context,
        ICommandArg newValue,
        CancellationToken cancel = default
    )
    {
        if (context is TContext page)
        {
            return InternalExecute(page, newValue, cancel);
        }

        throw new CommandNotSupportedContextException(Info, context, typeof(TContext));
    }

    protected abstract ValueTask<ICommandArg?> InternalExecute(
        TContext context,
        ICommandArg newValue,
        CancellationToken cancel
    );
}
