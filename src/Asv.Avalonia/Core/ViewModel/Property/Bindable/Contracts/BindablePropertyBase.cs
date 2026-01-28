using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public abstract class BindablePropertyBase<TModel, TView>(
    NavigationId id,
    ILoggerFactory loggerFactory
) : RoutableViewModel(id, loggerFactory), ISupportFocus
{
    public abstract BindableReactiveProperty<TView> ViewValue { get; }

    public abstract ReactiveProperty<TModel> ModelValue { get; }

    public void ForceValidate()
    {
        ViewValue.ForceValidate();
    }

    public bool IsFocused
    {
        get;
        set => SetField(ref field, value);
    }

    public void Focus()
    {
        // this is for force focus on view
        IsFocused = false;
        IsFocused = true;
    }

    protected abstract Exception? ValidateUserValue(TView userValue);

    protected abstract ValueTask OnChangedByUser(TView userValue, CancellationToken cancel);

    protected abstract void OnChangeByModel(TModel modelValue);

    protected virtual ValueTask ApplyValueToModel(TModel value, CancellationToken cancel)
    {
        ModelValue.OnNext(value);
        return ValueTask.CompletedTask;
    }
}
