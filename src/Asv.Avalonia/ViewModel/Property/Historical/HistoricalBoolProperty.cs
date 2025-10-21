using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalBoolProperty
    : BindablePropertyBase<bool, bool>,
        IHistoricalProperty<bool>
{
    private bool _externalChange;
    private bool _internalChange;

    public HistoricalBoolProperty(
        NavigationId id,
        ReactiveProperty<bool> modelValue,
        ILoggerFactory loggerFactory,
        IRoutable parent
    )
        : base(id, loggerFactory, parent)
    {
        ModelValue = modelValue;
        ViewValue = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        IsSelected = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        ViewValue.EnableValidation(ValidateValue);

        _internalChange = true;
        ViewValue.SubscribeAwait(OnChangedByUser, AwaitOperation.Drop).DisposeItWith(Disposable);
        _internalChange = false;

        ModelValue.Subscribe(OnChangeByModel).DisposeItWith(Disposable);
    }

    public override ReactiveProperty<bool> ModelValue { get; }
    public override BindableReactiveProperty<bool> ViewValue { get; }
    public override BindableReactiveProperty<bool> IsSelected { get; }

    protected override Exception? ValidateValue(bool userValue)
    {
        return null;
    }

    protected override async ValueTask OnChangedByUser(bool userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        _externalChange = true;
        await ChangeModelValue(userValue, cancel);
        _externalChange = false;
    }

    protected override void OnChangeByModel(bool modelValue)
    {
        if (_externalChange)
        {
            return;
        }

        _internalChange = true;
        ViewValue.OnNext(modelValue);
        _internalChange = false;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override async ValueTask ChangeModelValue(bool value, CancellationToken cancel)
    {
        var newValue = new BoolArg(value);
        await this.ExecuteCommand(ChangeBoolPropertyCommand.Id, newValue, cancel);
    }
}
