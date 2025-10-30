using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public abstract class CompositeBindablePropertyBase<T>(
    NavigationId id,
    ILoggerFactory loggerFactory) : RoutableViewModel(id, loggerFactory)
{
    public abstract ReactiveProperty<T> ModelValue { get; }
    public abstract void ForceValidate();
}
