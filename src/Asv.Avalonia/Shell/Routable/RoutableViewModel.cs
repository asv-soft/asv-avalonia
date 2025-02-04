using R3;

namespace Asv.Avalonia;

public abstract class RoutableViewModel(string id) : DisposableViewModel(id), IRoutable
{
    public IRoutable? Parent { get; set; }

    public async ValueTask Rise(AsyncRoutedEvent e)
    {
        if (IsDisposed)
        {
            return;
        }

        await InternalCatchEvent(e);
        if (e.IsHandled)
        {
            return;
        }

        if (Parent is not null)
        {
            await Parent.Rise(e);
        }
    }

    public abstract ValueTask<IRoutable> Navigate(string id);

    protected virtual ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        return ValueTask.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Parent = null;
        }

        base.Dispose(disposing);
    }
}
