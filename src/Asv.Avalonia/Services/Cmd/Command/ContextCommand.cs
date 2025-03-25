namespace Asv.Avalonia;

public abstract class ContextCommand<TContext> : AsyncCommand
    where TContext : IRoutable
{
    public override bool CanExecute(
        IRoutable context,
        ICommandParameter parameter,
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

    public override ValueTask<ICommandParameter?> Execute(
        IRoutable context,
        ICommandParameter newValue,
        CancellationToken cancel = default
    )
    {
        if (context is TContext page)
        {
            return InternalExecute(page, newValue, cancel);
        }

        throw new CommandNotSupportedContextException(Info, context, typeof(TContext));
    }

    protected abstract ValueTask<ICommandParameter?> InternalExecute(
        TContext context,
        ICommandParameter newValue,
        CancellationToken cancel
    );
}
