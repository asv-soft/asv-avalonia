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

    public abstract ValueTask<IRoutable> NavigateTo(string id);

    public BindableReactiveProperty<bool> IsFocused { get; } = new(false);
    protected virtual ValueTask InternalCatchEvent(AsyncRoutedEvent e) => ValueTask.CompletedTask;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            IsFocused.Dispose();
        }

        base.Dispose(disposing);
    }
}