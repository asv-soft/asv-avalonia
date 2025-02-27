using R3;
using Exception = System.Exception;

namespace Asv.Avalonia;

public class SourceViewModel : RoutableViewModelWithValidation
{
    private readonly IPluginManager _mng;
    private readonly PluginSourceViewModel? _viewModel;

    public SourceViewModel()
        : base(string.Empty)
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

        SubscribeToErrorsChanged();
    }

    public SourceViewModel(
        string id,
        IPluginManager mng,
        ILogService log,
        PluginSourceViewModel? viewModel
    )
        : base(id)
    {
        _mng = mng;
        _viewModel = viewModel;

        Name = new BindableReactiveProperty<string>(
            _viewModel?.Name.Value ?? string.Empty
        ).EnableValidation();
        SourceUri = new BindableReactiveProperty<string>(
            _viewModel?.SourceUri.Value ?? string.Empty
        ).EnableValidation();
        Username = new BindableReactiveProperty<string?>(_viewModel?.Model.Username);
        Password = new BindableReactiveProperty<string>();

        ApplyCommand = new ReactiveCommand((_, _) => Update(), configureAwait: false);

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

        if (_viewModel is not null)
        {
            Name = _viewModel.Name;
            SourceUri = _viewModel.SourceUri;
            Username.OnNext(_viewModel.Model.Username);
        }

        SubscribeToErrorsChanged();
    }

    public BindableReactiveProperty<string> Name { get; set; }
    public BindableReactiveProperty<string> SourceUri { get; set; }
    public BindableReactiveProperty<string?> Username { get; set; }
    public BindableReactiveProperty<string> Password { get; set; }
    private ReactiveCommand ApplyCommand { get; }

    public void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);
        IsValid.Subscribe(x =>
        {
            dialog.IsPrimaryButtonEnabled = x.IsSuccess;
        });

        dialog.PrimaryButtonCommand = ApplyCommand;
    }

    private ValueTask Update()
    {
        if (_viewModel == null)
        {
            _mng.AddServer(
                new PluginServer(Name.Value, SourceUri.Value, Username.Value, Password.Value)
            );
        }
        else
        {
            _mng.RemoveServer(_viewModel.Model);
            _mng.AddServer(
                new PluginServer(Name.Value, SourceUri.Value, Username.Value, Password.Value)
            );
        }

        return ValueTask.CompletedTask;
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            _viewModel?.Dispose();
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
