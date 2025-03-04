using R3;

namespace Asv.Avalonia;

public interface IHistoricalProperty<T> : IRoutable
{
    ReactiveProperty<T> ModelValue { get; }
}
