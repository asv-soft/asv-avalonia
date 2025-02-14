namespace Asv.Avalonia;

public abstract class OpenPageCommandBase(string pageId) : ContextCommand<IShell>
{
    protected override async ValueTask<IPersistable?> InternalExecute(
        IShell context,
        IPersistable newValue,
        CancellationToken cancel
    )
    {
        await context.Navigate(pageId);
        return null;
    }
}
