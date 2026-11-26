using Asv.Common;
using Asv.Modeling;
using Avalonia.Controls;
using R3;

namespace Asv.Avalonia.Example;

/// <summary>
/// A dialog demonstrating validation at three depths: a required field on the
/// dialog itself, a required field on a nested tab model, and a required field
/// on a nested field model below the tab. Validation events from the nested
/// models bubble up to this dialog.
/// </summary>
public sealed class NestedValidationDialogViewModel : DialogViewModelBase
{
    public const string ViewModelId = $"{BaseId}-nested-validation";

    private readonly SerialDisposable _applyDialog = new();

    public NestedValidationDialogViewModel()
        : base(ViewModelId)
    {
        Name = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        Details = new ValidationTabViewModel(this).DisposeItWith(Disposable);
        Name.EnableValidationRoutable(Validate, this, isForceValidation: true)
            .DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string> Name { get; }

    public ValidationTabViewModel Details { get; }

    public override void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);
        _applyDialog.Disposable = IsValid.Subscribe(b => dialog.IsPrimaryButtonEnabled = b);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Details;
    }

    private static ValidationResult Validate(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? ValidationResult.FailAsNullOrWhiteSpace
            : ValidationResult.Success;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _applyDialog.Dispose();
        }

        base.Dispose(disposing);
    }
}
