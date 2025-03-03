using R3;

namespace Asv.Avalonia;

public interface IModelProperty<T> : IRoutable
{
    ReactiveProperty<T> ModelValue { get; }
}
