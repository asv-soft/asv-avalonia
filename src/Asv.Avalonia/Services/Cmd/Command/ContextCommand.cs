namespace Asv.Avalonia;

public abstract class ContextCommand<TContext> : AsyncCommand
    where TContext : IRoutable
{
    public override bool CanExecute(
        IRoutable context,
        IPersistable parameter,
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

    public override ValueTask<IPersistable?> Execute(
        IRoutable context,
        IPersistable newValue,
        CancellationToken cancel = default
    )
    {
        if (context is TContext page)
        {
            return InternalExecute(page, newValue, cancel);
        }

        throw new CommandNotSupportedContextException(Info, context, typeof(TContext));
    }

    protected abstract ValueTask<IPersistable?> InternalExecute(
        TContext context,
        IPersistable newValue,
        CancellationToken cancel
    );
}
