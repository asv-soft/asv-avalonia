using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalHotKeyProperty : HistoricalPropertyBase<HotKeyInfo?, string?>
{
    private readonly ICommandService _svc;
    private readonly string _commandId;

    private readonly IList<Func<string?, ValidationResult>> _validationRules = [];

    private bool _internalChange;

    public override ReactiveProperty<HotKeyInfo?> ModelValue { get; }
    public override BindableReactiveProperty<string?> ViewValue { get; } = new();
    public override BindableReactiveProperty<bool> IsSelected { get; } = new();

    public HistoricalHotKeyProperty(
        string id,
        ICommandService svc,
        string commandId,
        ILoggerFactory loggerFactory,
        IList<Func<string?, ValidationResult>>? validationRules = null
    )
        : base(id, loggerFactory)
    {
        _svc = svc;
        _commandId = commandId;

        if (validationRules is not null)
        {
            foreach (var v in validationRules)
            {
                _validationRules.Add(v);
            }
        }

        ModelValue = new ReactiveProperty<HotKeyInfo?>(svc.GetHotKey(commandId));

        _internalChange = true;
        ViewValue.EnableValidation().ForceValidate();

        _sub1 = ViewValue.SubscribeAwait(
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

        _sub2 = ModelValue.Subscribe(OnChangeByModel);
    }

    #region Validation & sync

    protected override Exception? ValidateValue(string? userValue)
    {
        if (string.IsNullOrWhiteSpace(userValue))
        {
            return null;
        }

        try
        {
            _ = HotKeyInfo.Parse(userValue);
        }
        catch (Exception e)
        {
            return e;
        }

        return (
            from rule in _validationRules
            select rule(userValue) into res
            where res.IsFailed
            select res.ValidationException
        ).FirstOrDefault();
    }

    protected override async ValueTask OnChangedByUser(string? userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        var newValue = string.IsNullOrWhiteSpace(userValue) ? null : HotKeyInfo.Parse(userValue);

        if (newValue is not null)
        {
            _svc.SetHotKey(_commandId, newValue);
        }

        ModelValue.Value = newValue;
        await ValueTask.CompletedTask;
    }

    protected override void OnChangeByModel(HotKeyInfo? modelValue)
    {
        _internalChange = true;
        ViewValue.OnNext(modelValue?.ToString());
        _internalChange = false;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren() => [];

    #endregion

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _sub1.Dispose();
        _sub2.Dispose();
        ViewValue.Dispose();
        IsSelected.Dispose();
    }

    #endregion
}
