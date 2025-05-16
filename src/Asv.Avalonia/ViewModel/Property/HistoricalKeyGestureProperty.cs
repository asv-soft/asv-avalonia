using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalKeyGestureProperty : HistoricalPropertyBase<KeyGesture?, string?>
{
    private readonly ReactiveProperty<KeyGesture?> _modelValue;

    private bool _internalChange;

    public HistoricalKeyGestureProperty(string id, ReactiveProperty<KeyGesture?> modelValue)
        : base(id)
    {
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

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override ReactiveProperty<KeyGesture?> ModelValue => _modelValue;
    public override BindableReactiveProperty<string?> ViewValue { get; } = new();
    public override BindableReactiveProperty<bool> IsSelected { get; } = new();

    protected override Exception? ValidateValue(string? userValue)
    {
        if (userValue is null)
        {
            return null;
        }

        try
        {
            _ = KeyGesture.Parse(userValue);
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    protected override async ValueTask OnChangedByUser(string? userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        KeyGesture? keyGesture = null;
        if (userValue is not null)
        {
            keyGesture = KeyGesture.Parse(userValue);
        }

        var newValue = new KeyGestureCommandArg(keyGesture);
        await this.ExecuteCommand(ChangeKeyGesturePropertyCommand.Id, newValue);
    }

    protected override void OnChangeByModel(KeyGesture? modelValue)
    {
        _internalChange = true;
        ViewValue.OnNext(modelValue?.ToString() ?? null);
        _internalChange = false;
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
