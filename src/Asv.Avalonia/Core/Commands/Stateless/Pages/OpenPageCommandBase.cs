namespace Asv.Avalonia;

public abstract class OpenPageCommandBase(string pageId, INavigationService nav)
    : StatelessCommand<StringArg, EmptyArg>
{
    protected override async ValueTask<EmptyArg?> InternalExecute(
        EmptyArg arg,
        CancellationToken cancel
    )
    {
        await nav.GoTo(new NavigationPath(new NavigationId(pageId)));
        return null;
    }

    protected override async ValueTask<StringArg?> InternalExecute(
        StringArg newValue,
        CancellationToken cancel
    )
    {
        await nav.GoTo(new NavigationPath(new NavigationId(pageId, newValue.Value)));
        return null;
    }

    protected override bool InternalCanExecute(StringArg arg)
    {
        return true;
    }

    protected override bool InternalCanExecute(EmptyArg arg)
    {
        return true;
    }
}
