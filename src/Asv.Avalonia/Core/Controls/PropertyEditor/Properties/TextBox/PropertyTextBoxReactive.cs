using R3;

namespace Asv.Avalonia;

public class PropertyTextBoxReactive : PropertyTextBoxViewModel
{
    private readonly ReactiveProperty<string?> _model;
    private readonly Func<string?, Exception?>? _validator;

    public PropertyTextBoxReactive(
        string id,
        ReactiveProperty<string?> model,
        Func<string?, Exception?>? validator = null,
        bool enableUndo = true
    )
        : base(id, enableUndo)
    {
        _model = model;
        _validator = validator;

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

        Text.EnableValidation(ValidateText).AddTo(ref DisposableBag);
        Text.ForceValidate();
    }

    private Exception? ValidateText(string? value)
    {
        return _validator?.Invoke(value);
    }

    protected override ValueTask ApplyFromUserCore(CancellationToken cancel)
    {
        _model.OnNext(Text.Value);
        return ValueTask.CompletedTask;
    }
}
