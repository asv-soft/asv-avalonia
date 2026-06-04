using R3;

namespace Asv.Avalonia;

public class PropertyComboBoxReactive : PropertyComboBoxViewModel
{
    private readonly ReactiveProperty<IHeadlinedViewModel?> _model;

    public PropertyComboBoxReactive(
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
