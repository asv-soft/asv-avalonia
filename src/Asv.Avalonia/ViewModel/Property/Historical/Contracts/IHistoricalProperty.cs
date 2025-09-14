using R3;

namespace Asv.Avalonia;

public interface IHistoricalProperty<TModel> : IRoutable
{
    ReactiveProperty<TModel> ModelValue { get; }
    void ForceValidate();
}
