using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalStringProperty
    : BindablePropertyBase<string?, string?>,
        IHistoricalProperty<string?>
{
    private readonly IList<Func<string?, ValidationResult>> _validationRules = [];
    private bool _internalChange;
    private readonly IUndoChangeSink<ValueUndoChange<string?>> _undoSink;

    public HistoricalStringProperty(
        string id,
        ReactiveProperty<string?> modelValue,
        ILoggerFactory loggerFactory,
        IList<Func<string?, ValidationResult>>? validationRules = null
    )
        : base(id)
    {
        InternalInitValidationRules(validationRules);

        ModelValue = modelValue;
        ViewValue = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        ViewValue.EnableValidation(ValidateUserValue);

        _internalChange = true;
        ViewValue.SubscribeAwait(OnChangedByUser, AwaitOperation.Drop).DisposeItWith(Disposable);
        _internalChange = false;

        ModelValue.Subscribe(OnChangeByModel).DisposeItWith(Disposable);
        _undoSink = Undo.RegisterValue<string?>("default", ApplyStringValue, ApplyStringValue)
            .DisposeItWith(Disposable);
    }

    private void ApplyStringValue(string? value)
    {
        ModelValue.Value = value;
    }

    public override ReactiveProperty<string?> ModelValue { get; }
    public override BindableReactiveProperty<string?> ViewValue { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    public void AddValidationRule(Func<string?, ValidationResult> validationFunc)
    {
        _validationRules.Add(validationFunc);
    }

    protected override Exception? ValidateUserValue(string? userValue)
    {
        foreach (var rule in _validationRules)
        {
            var res = rule(userValue);
            if (!res.IsSuccess)
            {
                return res.ValidationException?.GetExceptionWithLocalizationOrSelf();
            }
        }

        return null;
    }

    protected override ValueTask OnChangedByUser(string? userValue, CancellationToken cancel)
    {
        if (ViewValue.HasErrors)
        {
            return ValueTask.CompletedTask;
        }

        if (_internalChange)
        {
            return ValueTask.CompletedTask;
        }

        var oldValue = ModelValue.Value;
        if (oldValue == userValue)
        {
            return ValueTask.CompletedTask;
        }

        try
        {
            _internalChange = true;
            ApplyStringValue(userValue);
            _undoSink.PublishUpdate(oldValue, userValue);
            return ValueTask.CompletedTask;
        }
        catch (Exception exception)
        {
            return ValueTask.FromException(exception);
        }
        finally
        {
            _internalChange = false;
        }
    }

    protected override void OnChangeByModel(string? modelValue)
    {
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

    protected override ValueTask ApplyValueToModel(string? value, CancellationToken cancel)
    {
        ApplyStringValue(value);
        return ValueTask.CompletedTask;
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
