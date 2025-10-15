using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public abstract class RoutableViewModel(
    NavigationId id,
    ILayoutService layoutService,
    ILoggerFactory loggerFactory
) : DisposableViewModel(id, layoutService, loggerFactory), IRoutable
{
    private RoutedEventHandler? _routedEventHandler;

    public IRoutable? Parent
    {
        get;
        set => SetField(ref field, value);
    }

    public async ValueTask Rise(AsyncRoutedEvent e)
    {
        if (IsDisposed)
        {
            // If the view model is disposed, we should not process any events
            Logger.ZLogWarning($"{this} is disposed, but try to rise event {e}");
            return;
        }

        await InternalCatchEvent(e);
        if (e.IsHandled)
        {
            return;
        }

        // If the event is handled in the current view model, try to invoke external handlers
        if (_routedEventHandler != null)
        {
            await _routedEventHandler.Invoke(this, e);
            if (e.IsHandled)
            {
                return;
            }
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

    public IDisposable AddEventHandler(RoutedEventHandler handler)
    {
        _routedEventHandler += handler;
        return R3.Disposable.Create(handler, RemoveEventHandler);
    }

    public void RemoveEventHandler(RoutedEventHandler handler)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        _routedEventHandler -= handler;
#pragma warning restore CS8601 // Possible null reference assignment.
    }

    public virtual ValueTask<IRoutable> Navigate(NavigationId id)
    {
        return ValueTask.FromResult(GetRoutableChildren().FirstOrDefault(x => x.Id == id) ?? this);
    }

    public abstract IEnumerable<IRoutable> GetRoutableChildren();

    protected virtual async ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        switch (e)
        {
            case TreeVisitorEvent treeVisitorEvent:
                treeVisitorEvent.Visit(this);
                break;

            case SaveLayoutEvent saveLayoutEvent:
            {
                if (saveLayoutEvent.IsHandled)
                {
                    break;
                }

                await HandleSaveLayout();
                break;
            }

            case LoadLayoutEvent loadLayoutEvent:
            {
                if (loadLayoutEvent.IsHandled)
                {
                    break;
                }

                await HandleLoadLayout();
                break;
            }

            case SaveLayoutToFileEvent saveLayoutToFileEvent:
            {
                if (saveLayoutToFileEvent.IsHandled)
                {
                    break;
                }

                await HandleSaveLayout();
                LayoutService.FlushFromMemory(this);
                break;
            }

            default:
                break;
        }
    }

    protected virtual ValueTask HandleSaveLayout(CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask HandleLoadLayout(CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Parent = null;
            _routedEventHandler = null;
        }

        base.Dispose(disposing);
    }
}
