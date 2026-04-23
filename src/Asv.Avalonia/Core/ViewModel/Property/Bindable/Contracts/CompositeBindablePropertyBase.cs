using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public abstract class CompositeBindablePropertyBase<T>(
    NavId id,
    ILoggerFactory loggerFactory
) : ViewModelBase(id, loggerFactory)
{
    public abstract ReactiveProperty<T> ModelValue { get; }
    public abstract void ForceValidate();
}
