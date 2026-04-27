using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public abstract class CompositeBindablePropertyBase<T>(string typeId)
    : ViewModel(typeId)
{
    public abstract ReactiveProperty<T> ModelValue { get; }
    public abstract void ForceValidate();
}
