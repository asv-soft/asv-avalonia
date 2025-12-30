using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

public sealed class SourceDialogViewModel : DialogViewModelBase
{
    public const string ViewModelId = $"{BaseId}.plugins.sources.source";

    public SourceDialogViewModel()
        : this(DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        Name.Value = "Github";
        SourceUri.Value = "https://github.com";
    }

    public SourceDialogViewModel(
        ILoggerFactory loggerFactory,
        in PluginsSourceViewModel? viewModel = null
    )
        : base(ViewModelId, loggerFactory)
    {
        Name = new BindableReactiveProperty<string>(viewModel?.Name ?? string.Empty);
        SourceUri = new BindableReactiveProperty<string>(viewModel?.SourceUri ?? string.Empty);
        Username = new BindableReactiveProperty<string?>(viewModel?.Model.Username);
        Password = new BindableReactiveProperty<string>();

        _sub1 = Name.EnableValidationRoutable(
            name =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new ValidationResult
                    {
                        IsSuccess = false,
                        ValidationException = new ValidationException(
                            "Name should not be null or empty",
                            RS.SourceDialogViewModel_NameValidation_NameIsRequired
                        ),
                    };
                }

                return ValidationResult.Success;
            },
            this,
            true
        );

        _sub2 = SourceUri.EnableValidationRoutable(
            uri =>
            {
                if (string.IsNullOrWhiteSpace(uri))
                {
                    return new ValidationResult
                    {
                        IsSuccess = false,
                        ValidationException = new ValidationException(
                            "SourceUri should not be null or empty",
                            RS.SourceDialogViewModel_SourceUriValidation_SourceUriIsRequired
                        ),
                    };
                }

                return ValidationResult.Success;
            },
            this,
            true
        );
    }

    public BindableReactiveProperty<string> Name { get; }
    public BindableReactiveProperty<string> SourceUri { get; }
    public BindableReactiveProperty<string?> Username { get; }
    public BindableReactiveProperty<string> Password { get; }

    public override void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        _sub3.Disposable = IsValid.Subscribe(b => dialog.IsPrimaryButtonEnabled = b);
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private readonly SerialDisposable _sub3 = new();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
            Name.Dispose();
            SourceUri.Dispose();
            Username.Dispose();
            Password.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
