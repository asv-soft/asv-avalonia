using R3;

namespace Asv.Avalonia;

public class PropertyTextBoxCallback : PropertyTextBoxViewModel, ISupportRefresh
{
    private readonly Func<CancellationToken, ValueTask<string?>> _read;
    private readonly Func<string?, CancellationToken, ValueTask> _write;
    private readonly Func<string?, Exception?>? _validator;

    public PropertyTextBoxCallback(
        string id,
        Func<CancellationToken, ValueTask<string?>> read,
        Func<string?, CancellationToken, ValueTask> write,
        Observable<string?> update,
        Func<string?, Exception?>? validator = null
    )
        : base(id)
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

        Text.EnableValidation(ValidateText).AddTo(ref DisposableBag);
        Text.ForceValidate();
    }

    private Exception? ValidateText(string? value)
    {
        return _validator?.Invoke(value);
    }

    protected override async ValueTask ApplyFromUser(CancellationToken cancel)
    {
        await _write(Text.Value, cancel).ConfigureAwait(false);
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
