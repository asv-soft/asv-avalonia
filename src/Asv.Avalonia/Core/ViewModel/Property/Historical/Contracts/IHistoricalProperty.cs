using R3;

namespace Asv.Avalonia;

public interface IHistoricalProperty<TModel> : IViewModel
{
    ReactiveProperty<TModel> ModelValue { get; }
}
