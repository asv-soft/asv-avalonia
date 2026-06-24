using R3;

namespace Asv.Avalonia;

public class PropertyToggleButtonGroupReactive : PropertyToggleButtonGroupViewModel
{
    private readonly ReactiveProperty<IHeadlinedViewModel?> _model;

    public PropertyToggleButtonGroupReactive(
        string id,
        ReactiveProperty<IHeadlinedViewModel?> model,
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

    protected override ValueTask ApplyFromUser(IHeadlinedViewModel item, CancellationToken cancel)
    {
        _model.OnNext(item);
        return ValueTask.CompletedTask;
    }
}
