using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.Example;

/// <summary>
/// The "Details" tab model at depth 1. It owns a required field and a nested
/// field model; both validation events bubble to the containing dialog.
/// </summary>
public sealed class ValidationTabViewModel : ViewModel
{
    public ValidationTabViewModel(IViewModel parent)
        : base("nested-validation-tab")
    {
        Code = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        SetParent(parent);
        Field = new ValidationFieldViewModel(this).DisposeItWith(Disposable);
        Code.EnableValidationRoutable(Validate, this, isForceValidation: true)
            .DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string> Code { get; }

    public ValidationFieldViewModel Field { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Field;
    }

    private static ValidationResult Validate(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? ValidationResult.FailAsNullOrWhiteSpace
            : ValidationResult.Success;
}
