namespace Asv.Avalonia;

public abstract class ContextCommand<TContext, TArg> : ContextCommand<TContext>
    where TContext : IRoutable
    where TArg : CommandArg
{
    public override bool CanExecute(
        IRoutable context,
        CommandArg parameter,
        out IRoutable targetContext
    )
    {
        if (parameter is TArg)
        {
            return base.CanExecute(context, parameter, out targetContext);
        }

        targetContext = context;
        return false;
    }

    protected override async ValueTask<CommandArg?> InternalExecute(
        TContext context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is TArg arg)
        {
            return await InternalExecute(context, arg, cancel);
        }

        throw new CommandNotSupportedContextException(Info, context, typeof(TArg));
    }

    public abstract ValueTask<TArg?> InternalExecute(
        TContext context,
        TArg arg,
        CancellationToken cancel
    );
}

public abstract class ContextCommand<TContext, TArg1, TArg2> : ContextCommand<TContext>
    where TContext : IRoutable
    where TArg1 : CommandArg
    where TArg2 : CommandArg
{
    public override bool CanExecute(
        IRoutable context,
        CommandArg parameter,
        out IRoutable targetContext
    )
    {
        if (parameter is TArg1 or TArg2)
        {
            return base.CanExecute(context, parameter, out targetContext);
        }

        targetContext = context;
        return false;
    }

    protected override async ValueTask<CommandArg?> InternalExecute(
        TContext context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        return newValue switch
        {
            TArg1 arg => await InternalExecute(context, arg, cancel),
            TArg2 arg2 => await InternalExecute(context, arg2, cancel),
            _ => throw new CommandException(
                $"Command {Info.Id} does not support argument type {newValue.GetType().Name} in context {context.GetType().Name}"
            ),
        };
    }

    public abstract ValueTask<CommandArg?> InternalExecute(
        TContext context,
        TArg1 arg,
        CancellationToken cancel
    );

    public abstract ValueTask<CommandArg?> InternalExecute(
        TContext context,
        TArg2 arg,
        CancellationToken cancel
    );
}

public abstract class ContextCommand<TContext> : AsyncCommand
    where TContext : IRoutable
{
    public override bool CanExecute(
        IRoutable context,
        CommandArg parameter,
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

    public override ValueTask<CommandArg?> Execute(
        IRoutable context,
        CommandArg newValue,
        CancellationToken cancel = default
    )
    {
        if (context is TContext page)
        {
            return InternalExecute(page, newValue, cancel);
        }

        throw new CommandNotSupportedContextException(Info, context, typeof(TContext));
    }

    protected abstract ValueTask<CommandArg?> InternalExecute(
        TContext context,
        CommandArg newValue,
        CancellationToken cancel
    );
}
