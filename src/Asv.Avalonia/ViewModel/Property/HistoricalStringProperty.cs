using R3;

namespace Asv.Avalonia;

public class HistoricalStringProperty : RoutableViewModel
{
    private readonly ReactiveProperty<string?> _modelValue;
    private readonly IList<Func<ValidationResult>> _validationRules = [];

    private bool _internalChange;
    private bool _userChange;

    public ReactiveProperty<string?> ModelValue => _modelValue;
    public BindableReactiveProperty<string?> ViewValue { get; } = new();
    public BindableReactiveProperty<bool> IsSelected { get; } = new();

    public HistoricalStringProperty(string id, ReactiveProperty<string?> modelValue)
        : base(id)
    {
        _modelValue = modelValue;
        _internalChange = true;
        _sub2 = ViewValue
            .EnableValidation(ValidateValue)
            .SubscribeAwait(OnChangedByUser, AwaitOperation.Drop);
        _internalChange = false;
        _sub3 = _modelValue.Subscribe(OnChangeByModel);
    }

    public void AddValidationRule(Func<ValidationResult> validationFunc)
    {
        _validationRules.Add(validationFunc);
    }

    private Exception? ValidateValue(string? userValue)
    {
        foreach (var rule in _validationRules)
        {
            var res = rule.Invoke();
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

        _userChange = true;
        var newValue = new Persistable<string?>(userValue);
        _modelValue.OnNext(userValue);
        await this.ExecuteCommand(ChangeStringPropertyCommand.Id, newValue);
        _userChange = false;
    }

    private void OnChangeByModel(string? modelValue)
    {
        if (_userChange)
        {
            return;
        }

        _internalChange = true;
        ViewValue.OnNext(modelValue);
        _internalChange = false;
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        return ValueTask.CompletedTask;
    }

    public IPersistable Save()
    {
        return new Persistable<string?>(ModelValue.Value);
    }

    public void Restore(IPersistable state)
    {
        if (state is Persistable<string?> value)
        {
            ModelValue.OnNext(value.Value);
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
    }

    #endregion
}
