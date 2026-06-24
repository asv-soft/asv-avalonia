using R3;

namespace Asv.Avalonia;

public class PropertySliderCallback : PropertySliderViewModel, ISupportRefresh
{
    private readonly Func<CancellationToken, ValueTask<double>> _read;
    private readonly Func<double, CancellationToken, ValueTask> _write;

    public PropertySliderCallback(
        string id,
        Func<CancellationToken, ValueTask<double>> read,
        Func<double, CancellationToken, ValueTask> write,
        Observable<double> update,
        bool enableUndo = true
    )
        : this(id, read, write, update, 0, 100, enableUndo) { }

    public PropertySliderCallback(
        string id,
        Func<CancellationToken, ValueTask<double>> read,
        Func<double, CancellationToken, ValueTask> write,
        Observable<double> update,
        double minimum,
        double maximum,
        bool enableUndo = true
    )
        : base(id, minimum, maximum, enableUndo)
    {
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(write);
        ArgumentNullException.ThrowIfNull(update);
        _read = read;
        _write = write;

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

    protected override async ValueTask ApplyFromUser(double value, CancellationToken cancel)
    {
        await _write(value, cancel).ConfigureAwait(false);
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
