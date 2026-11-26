using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.Example;

/// <summary>
/// A validation field at depth 2. Its validation event must bubble through the
/// tab model to the containing dialog.
/// </summary>
public sealed class ValidationFieldViewModel : ViewModel
{
    public ValidationFieldViewModel(IViewModel parent)
        : base("nested-validation-field")
    {
        Value = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        SetParent(parent);
        Value
            .EnableValidationRoutable(Validate, this, isForceValidation: true)
            .DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string> Value { get; }

    private static ValidationResult Validate(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? ValidationResult.FailAsNullOrWhiteSpace
            : ValidationResult.Success;
}
