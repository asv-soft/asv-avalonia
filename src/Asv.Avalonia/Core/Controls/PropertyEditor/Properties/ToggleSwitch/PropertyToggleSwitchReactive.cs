using R3;

namespace Asv.Avalonia;

public class PropertyToggleSwitchReactive : PropertyToggleSwitchViewModel
{
    private readonly ReactiveProperty<bool> _model;

    public PropertyToggleSwitchReactive(
        string id,
        ReactiveProperty<bool> model,
        bool enableUndo = true
    )
        : base(id, enableUndo)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;

        _model
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

    protected override ValueTask ApplyFromUser(bool value, CancellationToken cancel)
    {
        _model.OnNext(value);
        return ValueTask.CompletedTask;
    }
}
