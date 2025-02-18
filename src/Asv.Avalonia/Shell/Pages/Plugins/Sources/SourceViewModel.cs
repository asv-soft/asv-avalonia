using FluentAvalonia.UI.Controls;
using R3;

namespace Asv.Avalonia;

public class SourceViewModel : DisposableViewModel
{
    private readonly IPluginManager _mng;
    private readonly ILogService _log;
    private readonly PluginSourceViewModel? _viewModel;

    public SourceViewModel()
        : base(string.Empty)
    {
        DesignTime.ThrowIfNotDesignMode();
        Name = new BindableReactiveProperty<string>("Github").EnableValidation(x =>
            !string.IsNullOrWhiteSpace(x)
                ? new Exception(RS.SourceViewModel_SourceViewModel_NameIsRequired)
                : null
        );
        SourceUri = new BindableReactiveProperty<string>("https://github.com").EnableValidation(x =>
            !string.IsNullOrWhiteSpace(x)
                ? new Exception(RS.SourceViewModel_SourceViewModel_SourceUriIsRequired)
                : null
        );
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
        _log = log;
        _viewModel = viewModel;
        if (_viewModel == null)
        {
            return;
        }

        Name = new BindableReactiveProperty<string>(_viewModel.Name.Value).EnableValidation(x =>
            !string.IsNullOrWhiteSpace(x)
                ? new Exception(RS.SourceViewModel_SourceViewModel_NameIsRequired)
                : null
        );
        SourceUri = new BindableReactiveProperty<string>(
            _viewModel.SourceUri.Value
        ).EnableValidation(x =>
            !string.IsNullOrWhiteSpace(x)
                ? new Exception(RS.SourceViewModel_SourceViewModel_SourceUriIsRequired)
                : null
        );
        Username = new BindableReactiveProperty<string?>(_viewModel.Model.Username);
        Password = new BindableReactiveProperty<string>();

        var isValid = Observable.Create<bool>(x =>
        {
            x.OnNext(!Name.HasErrors || !SourceUri.HasErrors);
            return x;
        });

        ApplyCommand = new ReactiveCommand(isValid, false);
        ApplyCommand.Subscribe(_ => Update());
    }

    public BindableReactiveProperty<string> Name { get; set; }
    public BindableReactiveProperty<string> SourceUri { get; set; }
    public BindableReactiveProperty<string?> Username { get; set; }
    public BindableReactiveProperty<string> Password { get; set; }
    private ReactiveCommand ApplyCommand { get; }

    public void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);
        dialog.PrimaryButtonCommand = ApplyCommand;
    }

    private void Update()
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
    }
}
