using R3;

namespace Asv.Avalonia;

public class HistoricalStringProperty : RoutableViewModel, IHistoricalProperty<string?>
{
    private readonly ReactiveProperty<string?> _modelValue;
    private readonly IList<Func<string?, ValidationResult>> _validationRules = [];

    private bool _internalChange;

    public ReactiveProperty<string?> ModelValue => _modelValue;
    public BindableReactiveProperty<string?> ViewValue { get; } = new();
    public BindableReactiveProperty<bool> IsSelected { get; } = new();

    public HistoricalStringProperty(
        string id,
        ReactiveProperty<string?> modelValue,
        IList<Func<string?, ValidationResult>>? validationRules = null
    )
        : base(id)
    {
        InternalInitValidationRules(validationRules);
        _modelValue = modelValue;

        _internalChange = true;
        ViewValue.EnableValidation().ForceValidate();
        _sub2 = ViewValue.SubscribeAwait(
            async (value, cancel) =>
            {
                var error = ValidateValue(value);
                if (error is null)
                {
                    await OnChangedByUser(value, cancel);
                    return;
                }

                ViewValue.OnErrorResume(error);
            },
            AwaitOperation.Drop
        );
        _internalChange = false;

        _sub3 = _modelValue.Subscribe(OnChangeByModel);
    }

    public void AddValidationRule(Func<string?, ValidationResult> validationFunc)
    {
        _validationRules.Add(validationFunc);
    }

    private Exception? ValidateValue(string? userValue)
    {
        foreach (var rule in _validationRules)
        {
            var res = rule(userValue);
            if (res.IsFailed)
            {
                return res.ValidationException;
            }
        }

        return null;
    }

    private async ValueTask OnChangedByUser(string? userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        var newValue = new StringCommandParameter(userValue ?? string.Empty);
        await this.ExecuteCommand(ChangeStringPropertyCommand.Id, newValue);
    }

    private void OnChangeByModel(string? modelValue)
    {
        _internalChange = true;
        ViewValue.OnNext(modelValue);
        _internalChange = false;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        return ValueTask.CompletedTask;
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

    #region Dispose

    private readonly IDisposable _sub2;
    private readonly IDisposable _sub3;

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _sub2.Dispose();
        _sub3.Dispose();
        ViewValue.Dispose();
        IsSelected.Dispose();
        _validationRules.Clear();
    }

    #endregion
}
