namespace Asv.Avalonia;

public abstract class StatelessCommand : AsyncCommand
{
    public override bool CanExecute(
        IRoutable context,
        CommandArg parameter,
        out IRoutable targetContext
    )
    {
        targetContext = context;
        return true;
    }

    public override ValueTask<CommandArg?> Execute(
        IRoutable context,
        CommandArg newValue,
        CancellationToken cancel = default
    )
    {
        return InternalExecute(newValue, cancel);
    }

    protected abstract ValueTask<CommandArg?> InternalExecute(
        CommandArg newValue,
        CancellationToken cancel
    );
}

public abstract class StatelessCommand<TArg> : StatelessCommand
    where TArg : CommandArg
{
    public override bool CanExecute(
        IRoutable context,
        CommandArg parameter,
        out IRoutable targetContext
    )
    {
        targetContext = context;

        if (parameter is TArg arg)
        {
            return InternalCanExecute(arg);
        }

        return false;
    }

    protected abstract bool InternalCanExecute(TArg arg);

    protected override async ValueTask<CommandArg?> InternalExecute(
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not TArg arg)
        {
            throw new ArgumentException(
                $"Invalid command argument type. Expected {typeof(TArg).Name}, but got {newValue.GetType().Name}"
            );
        }

        return await InternalExecute(arg, cancel);
    }

    protected abstract ValueTask<TArg?> InternalExecute(TArg arg, CancellationToken cancel);
}

public abstract class StatelessCommand<TArg1, TArg2> : StatelessCommand
    where TArg1 : CommandArg
    where TArg2 : CommandArg
{
    public override bool CanExecute(
        IRoutable context,
        CommandArg parameter,
        out IRoutable targetContext
    )
    {
        targetContext = context;

        return parameter switch
        {
            TArg1 arg1 => InternalCanExecute(arg1),
            TArg2 arg2 => InternalCanExecute(arg2),
            _ => false,
        };
    }

    protected abstract bool InternalCanExecute(TArg1 arg);
    protected abstract bool InternalCanExecute(TArg2 arg);

    protected override async ValueTask<CommandArg?> InternalExecute(
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        return newValue switch
        {
            TArg1 arg1 => await InternalExecute(arg1, cancel),
            TArg2 arg2 => await InternalExecute(arg2, cancel),
            _ => throw new ArgumentException(
                $"Invalid command argument type. Expected {typeof(TArg1).Name} or {typeof(TArg2).Name} , but got {newValue.GetType().Name}"
            ),
        };
    }

    protected abstract ValueTask<TArg1?> InternalExecute(TArg1 arg, CancellationToken cancel);
    protected abstract ValueTask<TArg2?> InternalExecute(TArg2 arg, CancellationToken cancel);
}

public abstract class StatelessCommand<TArg1, TArg2, TArg3> : StatelessCommand
    where TArg1 : CommandArg
    where TArg2 : CommandArg
    where TArg3 : CommandArg
{
    public override bool CanExecute(
        IRoutable context,
        CommandArg parameter,
        out IRoutable targetContext
    )
    {
        targetContext = context;

        return parameter switch
        {
            TArg1 arg1 => InternalCanExecute(arg1),
            TArg2 arg2 => InternalCanExecute(arg2),
            TArg3 arg3 => InternalCanExecute(arg3),
            _ => false,
        };
    }

    protected abstract bool InternalCanExecute(TArg1 arg);
    protected abstract bool InternalCanExecute(TArg2 arg);
    protected abstract bool InternalCanExecute(TArg3 arg);

    protected override async ValueTask<CommandArg?> InternalExecute(
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        return newValue switch
        {
            TArg1 arg1 => await InternalExecute(arg1, cancel),
            TArg2 arg2 => await InternalExecute(arg2, cancel),
            TArg3 arg3 => await InternalExecute(arg3, cancel),
            _ => throw new ArgumentException(
                $"Invalid command argument type. Expected {typeof(TArg1).Name} or {typeof(TArg2).Name} , but got {newValue.GetType().Name}"
            ),
        };
    }

    protected abstract ValueTask<TArg1?> InternalExecute(TArg1 arg, CancellationToken cancel);
    protected abstract ValueTask<TArg2?> InternalExecute(TArg2 arg, CancellationToken cancel);
    protected abstract ValueTask<TArg3?> InternalExecute(TArg3 arg, CancellationToken cancel);
}
