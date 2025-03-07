using R3;

namespace Asv.Avalonia;

public abstract class RoutableViewModel(NavigationId id) : DisposableViewModel(id), IRoutable
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

        switch (e.RoutingStrategy)
        {
            case RoutingStrategy.Bubble:
            {
                if (Parent is not null)
                {
                    await Parent.Rise(e);
                }

                break;
            }

            case RoutingStrategy.Tunnel:
            {
                foreach (var child in GetRoutableChildren())
                {
                    await child.Rise(e);
                    if (e.IsHandled)
                    {
                        return;
                    }
                }

                break;
            }

            case RoutingStrategy.Direct:
                // Do nothing here
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public virtual ValueTask<IRoutable> Navigate(NavigationId id)
    {
        return ValueTask.FromResult(GetRoutableChildren().FirstOrDefault(x => x.Id == id) ?? this);
    }

    public abstract IEnumerable<IRoutable> GetRoutableChildren();

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
