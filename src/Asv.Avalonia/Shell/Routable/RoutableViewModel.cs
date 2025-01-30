using R3;

namespace Asv.Avalonia;

public abstract class RoutableViewModel : DisposableViewModel, IRoutable
{
    private readonly IDisposable _sub1;

    protected RoutableViewModel(string id)
        : base(id)
    {
        _sub1 = IsFocused.SubscribeAwait(
            (x, _) =>
            {
                if (x)
                {
                    return Rise(new FocusedEvent(this));
                }

                return ValueTask.CompletedTask;
            }
        );
    }

    public IRoutable? Parent { get; set; }
    public abstract IEnumerable<IRoutable> Children { get; }

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

        switch (e.RoutingEventStrategy)
        {
            case RoutingEventStrategy.Bubble when Parent is not null:
                await Parent.Rise(e);
                break;
            case RoutingEventStrategy.Tunnel:
            {
                foreach (var child in Children)
                {
                    await child.Rise(e);
                    if (e.IsHandled)
                    {
                        break;
                    }
                }

                return;
            }

            case RoutingEventStrategy.Direct:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public virtual ValueTask<IRoutable> NavigateTo(ArraySegment<string> path)
    {
        if (path.Count == 0)
        {
            IsFocused.Value = true;
            return ValueTask.FromResult<IRoutable>(this);
        }

        foreach (var child in Children)
        {
            if (child.Id == path[0])
            {
                return child.NavigateTo(path[1..]);
            }
        }

        return ValueTask.FromResult<IRoutable>(this);
    }

    public ReactiveProperty<bool> IsFocused { get; } = new(false);

    protected abstract ValueTask InternalCatchEvent(AsyncRoutedEvent e);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            IsFocused.Dispose();
        }

        base.Dispose(disposing);
    }
}
