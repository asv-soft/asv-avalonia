namespace Asv.Avalonia;

public abstract class OpenPageCommandBase(string pageId, INavigationService nav)
    : ContextCommand<IShell>
{
    protected override async ValueTask<ICommandParameter?> InternalExecute(
        IShell context,
        ICommandParameter newValue,
        CancellationToken cancel
    )
    {
        if (newValue is StringCommandParameter args)
        {
            await nav.GoTo(new NavigationPath(new NavigationId(pageId, args.Value)));
        }
        else
        {
            await nav.GoTo(new NavigationPath(new NavigationId(pageId)));
        }

        return null;
    }
}
