using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public abstract class CompositeBindablePropertyBase<T> : RoutableViewModel
{
    protected CompositeBindablePropertyBase(
        NavigationId id,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IRoutable parent
    )
        : base(id, layoutService, loggerFactory)
    {
        Parent = parent;
    }

    public abstract ReactiveProperty<T> ModelValue { get; }
    public abstract void ForceValidate();
}
