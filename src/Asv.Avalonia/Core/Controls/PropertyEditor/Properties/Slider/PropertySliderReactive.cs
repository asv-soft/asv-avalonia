using R3;

namespace Asv.Avalonia;

public class PropertySliderReactive : PropertySliderViewModel
{
    private readonly ReactiveProperty<double> _model;

    public PropertySliderReactive(string id, ReactiveProperty<double> model, bool enableUndo = true)
        : this(id, model, 0, 100, enableUndo) { }

    public PropertySliderReactive(
        string id,
        ReactiveProperty<double> model,
        double minimum,
        double maximum,
        bool enableUndo = true
    )
        : base(id, minimum, maximum, enableUndo)
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

    protected override ValueTask ApplyFromUser(double value, CancellationToken cancel)
    {
        _model.OnNext(value);
        return ValueTask.CompletedTask;
    }
}
