namespace Asv.Avalonia;

public abstract class OpenPageCommandBase(string pageId, INavigationService nav)
    : ContextCommand<IShell>
{
    protected override async ValueTask<IPersistable?> InternalExecute(
        IShell context,
        IPersistable newValue,
        CancellationToken cancel
    )
    {
        if (newValue is Persistable<string> args)
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
