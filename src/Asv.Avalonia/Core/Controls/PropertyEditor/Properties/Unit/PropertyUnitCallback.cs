using R3;

namespace Asv.Avalonia;

public class PropertyUnitCallback : PropertyUnitViewModel, ISupportRefresh
{
    private readonly Func<CancellationToken, ValueTask<double>> _read;
    private readonly Func<double, CancellationToken, ValueTask> _write;
    private readonly Func<double, Exception?>? _validator;

    public PropertyUnitCallback(
        string id,
        IUnit unit,
        Func<CancellationToken, ValueTask<double>> read,
        Func<double, CancellationToken, ValueTask> write,
        Observable<double> update,
        Func<double, Exception?>? validator = null,
        string? format = null
    )
        : base(id, unit, format)
    {
        _read = read;
        _write = write;
        _validator = validator;
        update
            .Subscribe(
                ApplyValueFromModel,
                x =>
                {
                    if (x.Exception == null)
                    {
                        return;
                    }
                    ApplyErrorFromModel(x.Exception);
                }
            )
            .AddTo(ref DisposableBag);
    }

    protected override Exception? ValidateSiValue(double siValue)
    {
        if (_validator != null)
        {
            var result = _validator(siValue);
            if (result != null)
            {
                return result;
            }
        }
        return base.ValidateSiValue(siValue);
    }

    protected override async ValueTask ApplyFromUser(double siValue, CancellationToken cancel)
    {
        await _write(siValue, cancel).ConfigureAwait(false);
    }

    public async ValueTask Refresh(CancellationToken cancel = default)
    {
        try
        {
            var value = await _read(cancel).ConfigureAwait(false);
            ApplyValueFromModel(value);
        }
        catch (Exception e)
        {
            ApplyErrorFromModel(e);
        }
    }
}
