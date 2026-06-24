using R3;

namespace Asv.Avalonia;

public class PropertyToggleSwitchCallback : PropertyToggleSwitchViewModel, ISupportRefresh
{
    private readonly Func<CancellationToken, ValueTask<bool>> _read;
    private readonly Func<bool, CancellationToken, ValueTask> _write;

    public PropertyToggleSwitchCallback(
        string id,
        Func<CancellationToken, ValueTask<bool>> read,
        Func<bool, CancellationToken, ValueTask> write,
        Observable<bool> update,
        bool enableUndo = true
    )
        : base(id, enableUndo)
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

    protected override async ValueTask ApplyFromUser(bool value, CancellationToken cancel)
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
