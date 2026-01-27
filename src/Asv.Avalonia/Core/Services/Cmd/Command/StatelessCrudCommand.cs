namespace Asv.Avalonia;

public abstract class StatelessCrudCommand<TOptions> : StatelessCommand<ActionArg>
    where TOptions : CommandArg
{
    protected override bool InternalCanExecute(ActionArg arg)
    {
        switch (arg.Action)
        {
            case ActionArg.Kind.Add:
                if (arg.Value is not TOptions)
                {
                    return false;
                }

                break;
            case ActionArg.Kind.Remove:
                if (string.IsNullOrEmpty(arg.SubjectId))
                {
                    return false;
                }

                break;
            case ActionArg.Kind.Change:
                if (arg.Value is not TOptions || string.IsNullOrEmpty(arg.SubjectId))
                {
                    return false;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return true;
    }

    protected override async ValueTask<ActionArg?> InternalExecute(
        ActionArg arg,
        CancellationToken cancel
    )
    {
        switch (arg.Action)
        {
            case ActionArg.Kind.Add:
                if (arg.Value is not TOptions options)
                {
                    throw new CommandArgMismatchException(typeof(TOptions), nameof(arg.Value));
                }

                var id = await Create(options);
                return CommandArg.RemoveAction(id);
            case ActionArg.Kind.Remove:
                if (string.IsNullOrEmpty(arg.SubjectId))
                {
                    throw new CommandArgMismatchException(typeof(string), nameof(arg.SubjectId));
                }

                var oldOptions = await Read(arg.SubjectId);
                await Delete(arg.SubjectId);
                return CommandArg.AddAction(oldOptions);
            case ActionArg.Kind.Change:
                if (arg.Value is not TOptions changeOptions)
                {
                    throw new CommandArgMismatchException(typeof(TOptions), nameof(arg.Value));
                }

                if (string.IsNullOrEmpty(arg.SubjectId))
                {
                    throw new CommandArgMismatchException(typeof(string), nameof(arg.SubjectId));
                }

                var prevOptions = await Read(arg.SubjectId);
                var newId = await Update(arg.SubjectId, changeOptions);
                return CommandArg.ChangeAction(newId, prevOptions);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected abstract ValueTask<string> Update(string subjectId, TOptions options);
    protected abstract ValueTask Delete(string subjectId);
    protected abstract ValueTask<string> Create(TOptions options);
    protected abstract ValueTask<TOptions> Read(string subjectId);
}
