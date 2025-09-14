using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalStringProperty
    : BindablePropertyBase<string?, string?>,
        IHistoricalProperty<string?>
{
    private readonly IList<Func<string?, ValidationResult>> _validationRules = [];
    private bool _externalChange;
    private bool _internalChange;

    public HistoricalStringProperty(
        string id,
        ReactiveProperty<string?> modelValue,
        ILoggerFactory loggerFactory,
        IRoutable parent,
        IList<Func<string?, ValidationResult>>? validationRules = null
    )
        : base(id, loggerFactory, parent)
    {
        InternalInitValidationRules(validationRules);

        ModelValue = modelValue;
        ViewValue = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        IsSelected = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        ViewValue.EnableValidation(ValidateValue);

        _internalChange = true;
        ViewValue.SubscribeAwait(OnChangedByUser, AwaitOperation.Drop).DisposeItWith(Disposable);
        _internalChange = false;

        ModelValue.Subscribe(OnChangeByModel).DisposeItWith(Disposable);
    }

    public override ReactiveProperty<string?> ModelValue { get; }
    public override BindableReactiveProperty<string?> ViewValue { get; }
    public override BindableReactiveProperty<bool> IsSelected { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public void AddValidationRule(Func<string?, ValidationResult> validationFunc)
    {
        _validationRules.Add(validationFunc);
    }

    protected override Exception? ValidateValue(string? userValue)
    {
        foreach (var rule in _validationRules)
        {
            var res = rule(userValue);
            if (!res.IsSuccess)
            {
                return res.ValidationException;
            }
        }

        return null;
    }

    protected override async ValueTask OnChangedByUser(string? userValue, CancellationToken cancel)
    {
        if (ViewValue.HasErrors)
        {
            return;
        }

        if (_internalChange)
        {
            return;
        }

        _externalChange = true;
        await ChangeModelValue(userValue ?? string.Empty, cancel);
        _externalChange = false;
    }

    protected override void OnChangeByModel(string? modelValue)
    {
        if (_externalChange)
        {
            return;
        }

        _internalChange = true;
        ViewValue.OnNext(modelValue);
        _internalChange = false;
    }

    private void InternalInitValidationRules(
        IList<Func<string?, ValidationResult>>? validationRules
    )
    {
        if (validationRules is null)
        {
            return;
        }

        foreach (var rules in validationRules)
        {
            AddValidationRule(rules);
        }
    }

    protected override async ValueTask ChangeModelValue(string? value, CancellationToken cancel)
    {
        var newValue = new StringArg(value ?? string.Empty);
        await this.ExecuteCommand(ChangeStringPropertyCommand.Id, newValue, cancel);
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _validationRules.Clear();
    }
}
