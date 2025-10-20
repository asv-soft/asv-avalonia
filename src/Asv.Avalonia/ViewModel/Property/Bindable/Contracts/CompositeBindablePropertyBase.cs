using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public abstract class CompositeBindablePropertyBase<T> : RoutableViewModel
{
    protected CompositeBindablePropertyBase(
        NavigationId id,
        ILoggerFactory loggerFactory,
        IRoutable parent
    )
        : base(id, loggerFactory)
    {
        Parent = parent;
    }

    public abstract ReactiveProperty<T> ModelValue { get; }
    public abstract void ForceValidate();
}
