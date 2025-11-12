using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;
using Exception = System.Exception;

namespace Asv.Avalonia.Plugins;

public class SourceDialogViewModel : DialogViewModelBase
{
    public const string ViewModelId = $"{BaseId}.plugins.sources.source";

    public SourceDialogViewModel()
        : base(DesignTime.Id, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        Name = new BindableReactiveProperty<string>("Github").EnableValidation();
        SourceUri = new BindableReactiveProperty<string>("https://github.com").EnableValidation();
        _sub1 = Name.Subscribe(x =>
        {
            if (string.IsNullOrWhiteSpace(x))
            {
                Name.OnErrorResume(new Exception(RS.SourceViewModel_NameValidation_NameIsRequired));
            }
        });
        _sub2 = SourceUri.Subscribe(x =>
        {
            if (string.IsNullOrWhiteSpace(x))
            {
                SourceUri.OnErrorResume(
                    new Exception(RS.SourceViewModel_SourceUriValidation_SourceUriIsRequired)
                );
            }
        });
    }

    public SourceDialogViewModel(
        ILoggerFactory loggerFactory,
        in PluginSourceViewModel? viewModel = null
    )
        : base(ViewModelId, loggerFactory)
    {
        Name = new BindableReactiveProperty<string>(viewModel?.Name.Value ?? string.Empty);
        SourceUri = new BindableReactiveProperty<string>(
            viewModel?.SourceUri.Value ?? string.Empty
        );
        Username = new BindableReactiveProperty<string?>(viewModel?.Model.Username);
        Password = new BindableReactiveProperty<string>();

        _sub1 = Name.EnableValidationRoutable(
            value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return new ValidationResult
                    {
                        IsSuccess = false,
                        ValidationException = new ValidationException(
                            RS.SourceViewModel_NameValidation_NameIsRequired
                        ),
                    };
                }

                return ValidationResult.Success;
            },
            this,
            true
        );

        _sub2 = SourceUri.EnableValidationRoutable(
            x =>
            {
                if (string.IsNullOrWhiteSpace(x))
                {
                    return new ValidationResult
                    {
                        IsSuccess = false,
                        ValidationException = new ValidationException(
                            RS.SourceViewModel_SourceUriValidation_SourceUriIsRequired
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

        _sub3 = IsValid.Subscribe(b => dialog.IsPrimaryButtonEnabled = b);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private IDisposable _sub3;

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
