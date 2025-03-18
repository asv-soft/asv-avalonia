using Microsoft.Extensions.Logging;
using R3;
using Exception = System.Exception;

namespace Asv.Avalonia.Plugins;

// TODO: add validation
public class SourceViewModel : DialogViewModelBase
{
    public const string ViewModelId = "plugins.sources.source.dialog";

    private readonly IPluginManager _mng;
    private readonly PluginSourceViewModel? _viewModel;

    public SourceViewModel()
        : base(ViewModelId)
    {
        DesignTime.ThrowIfNotDesignMode();
        Name = new BindableReactiveProperty<string>("Github").EnableValidation();
        SourceUri = new BindableReactiveProperty<string>("https://github.com").EnableValidation();
        _sub1 = Name.Subscribe(x =>
        {
            if (string.IsNullOrWhiteSpace(x))
            {
                Name.OnErrorResume(
                    new Exception(RS.SourceViewModel_SourceViewModel_NameIsRequired)
                );
            }
        });
        _sub2 = SourceUri.Subscribe(x =>
        {
            if (string.IsNullOrWhiteSpace(x))
            {
                SourceUri.OnErrorResume(
                    new Exception(RS.SourceViewModel_SourceViewModel_SourceUriIsRequired)
                );
            }
        });
    }

    public SourceViewModel(
        IPluginManager mng,
        ILoggerFactory loggerFactory,
        PluginSourceViewModel? viewModel
    )
        : base(ViewModelId)
    {
        _mng = mng;
        _viewModel = viewModel;

        Name = new BindableReactiveProperty<string>(_viewModel?.Name.Value ?? string.Empty);
        SourceUri = new BindableReactiveProperty<string>(
            _viewModel?.SourceUri.Value ?? string.Empty
        );
        Username = new BindableReactiveProperty<string?>(_viewModel?.Model.Username);
        Password = new BindableReactiveProperty<string>();

        ApplyCommand = new ReactiveCommand((_, _) => Update(), configureAwait: false);

        _sub1 = Name.EnableValidation(
            value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return ValueTask.FromResult<ValidationResult>(
                        new Exception(RS.SourceViewModel_SourceViewModel_NameIsRequired)
                    );
                }

                return ValidationResult.Success;
            },
            this,
            true
        );

        _sub2 = SourceUri.EnableValidation(
            x =>
            {
                if (string.IsNullOrWhiteSpace(x))
                {
                    return ValueTask.FromResult<ValidationResult>(
                        new Exception(RS.SourceViewModel_SourceViewModel_SourceUriIsRequired)
                    );
                }

                return ValidationResult.Success;
            },
            this,
            true
        );
    }

    public BindableReactiveProperty<string> Name { get; set; }
    public BindableReactiveProperty<string> SourceUri { get; set; }
    public BindableReactiveProperty<string?> Username { get; set; }
    public BindableReactiveProperty<string> Password { get; set; }
    private ReactiveCommand ApplyCommand { get; }

    public void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        _sub3 = IsValid.Subscribe(b =>
        {
            dialog.IsPrimaryButtonEnabled = b;
        });
        dialog.PrimaryButtonCommand = ApplyCommand;
    }

    private ValueTask Update()
    {
        if (_viewModel != null)
        {
            _mng.RemoveServer(_viewModel.Model);
        }

        _mng.AddServer(
            new PluginServer(Name.Value, SourceUri.Value, Username.Value, Password.Value)
        );

        return ValueTask.CompletedTask;
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
            ApplyCommand.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
