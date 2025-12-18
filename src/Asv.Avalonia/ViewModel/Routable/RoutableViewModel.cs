using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public abstract class RoutableViewModel : DisposableViewModel, IRoutable
{
    protected RoutableViewModel(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        Events = new RoutedEventController<IRoutable>(this).DisposeItWith(Disposable);
        Events
            .Subscribe(
                (_, e) =>
                {
                    switch (e)
                    {
                        case TreeVisitorEvent treeVisitorEvent:
                            treeVisitorEvent.Visit(this);
                            break;
                        default:
                            break;
                    }

                    return ValueTask.CompletedTask;
                }
            )
            .DisposeItWith(Disposable);
    }

    public IRoutedEventController<IRoutable> Events { get; }

    public IRoutable? Parent
    {
        get;
        set => SetField(ref field, value);
    }

    public virtual ValueTask<IRoutable> Navigate(NavigationId id)
    {
        return ValueTask.FromResult(GetChildren().FirstOrDefault(x => x.Id == id) ?? this);
    }

    public abstract IEnumerable<IRoutable> GetChildren();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Parent = null;
        }

        base.Dispose(disposing);
    }
}
