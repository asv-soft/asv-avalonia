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
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        ModelValue = modelValue;
        ViewValue = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        ViewValue.EnableValidation(ValidateUserValue);

        _internalChange = true;
        ViewValue.SubscribeAwait(OnChangedByUser, AwaitOperation.Drop).DisposeItWith(Disposable);
        _internalChange = false;

        ModelValue.Subscribe(OnChangeByModel).DisposeItWith(Disposable);
    }

    public override ReactiveProperty<bool> ModelValue { get; }
    public override BindableReactiveProperty<bool> ViewValue { get; }

    protected override Exception? ValidateUserValue(bool userValue)
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
        await ApplyValueToModel(userValue, cancel);
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

    protected override async ValueTask ApplyValueToModel(bool value, CancellationToken cancel)
    {
        var newValue = new BoolArg(value);
        await this.ExecuteCommand(ChangeBoolPropertyCommand.Id, newValue, cancel);
    }
}
