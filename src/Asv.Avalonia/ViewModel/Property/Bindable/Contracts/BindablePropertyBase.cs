using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public abstract class BindablePropertyBase<TModel, TView> : RoutableViewModel
{
    protected BindablePropertyBase(NavigationId id, ILoggerFactory loggerFactory, IRoutable parent)
        : base(id, loggerFactory)
    {
        Parent = parent;
    }

    public abstract BindableReactiveProperty<TView> ViewValue { get; }
    public abstract BindableReactiveProperty<bool> IsSelected { get; }
    public abstract ReactiveProperty<TModel> ModelValue { get; }

    public void ForceValidate()
    {
        ViewValue.ForceValidate();
    }

    protected abstract Exception? ValidateValue(TView userValue);

    protected abstract ValueTask OnChangedByUser(TView userValue, CancellationToken cancel);

    protected abstract void OnChangeByModel(TModel modelValue);

    protected virtual ValueTask ChangeModelValue(TModel value, CancellationToken cancel)
    {
        ModelValue.OnNext(value);
        return ValueTask.CompletedTask;
    }
}
