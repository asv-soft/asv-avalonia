using R3;

namespace Asv.Avalonia;

public interface IValidationProperty<TModel> : IRoutable
{
    ReactiveProperty<TModel> ModelValue { get; }
    void ForceValidate();
}
