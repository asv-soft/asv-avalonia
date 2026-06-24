using R3;

namespace Asv.Avalonia;

public class PropertyToggleButtonGroupCallback : PropertyToggleButtonGroupViewModel, ISupportRefresh
{
    private readonly Func<CancellationToken, ValueTask<IHeadlinedViewModel?>> _read;
    private readonly Func<IHeadlinedViewModel, CancellationToken, ValueTask> _write;

    public PropertyToggleButtonGroupCallback(
        string id,
        Func<CancellationToken, ValueTask<IHeadlinedViewModel?>> read,
        Func<IHeadlinedViewModel, CancellationToken, ValueTask> write,
        Observable<IHeadlinedViewModel?> update,
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

    protected override async ValueTask ApplyFromUser(
        IHeadlinedViewModel item,
        CancellationToken cancel
    )
    {
        await _write(item, cancel).ConfigureAwait(false);
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
