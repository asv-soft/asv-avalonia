using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

public class PluginsSourceViewModel : RoutableViewModel
{
    public const string ViewModelIdPart = "source";

    private readonly ILoggerFactory _loggerFactory;
    private readonly INavigationService _navigationService;
    private readonly YesOrNoDialogPrefab _yesOrNoDialogPrefab;

    public PluginsSourceViewModel()
        : this(
            NullPluginServerInfo.Instance,
            DesignTime.Navigation,
            NullDialogService.Instance,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PluginsSourceViewModel(
        IPluginServerInfo pluginServerInfo,
        INavigationService navigationService,
        IDialogService dialogService,
        ILoggerFactory loggerFactory
    )
        : base(new NavigationId(ViewModelIdPart, pluginServerInfo.SourceUri), loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(pluginServerInfo);
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(dialogService);
        _loggerFactory = loggerFactory;
        _navigationService = navigationService;
        _yesOrNoDialogPrefab = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();

        Name = pluginServerInfo.Name;
        SourceUri = pluginServerInfo.SourceUri;
        Model = pluginServerInfo;
        Edit = new ReactiveCommand(EditImpl).DisposeItWith(Disposable);
        Remove = new ReactiveCommand(RemoveImpl).DisposeItWith(Disposable);

        Edit.IgnoreOnErrorResume(ex => Logger.LogError(ex, "Error to update plugin server list"));
        Remove.IgnoreOnErrorResume(ex => Logger.LogError(ex, "Error to remove plugin server"));
    }

    public IPluginServerInfo Model { get; }
    public string Name { get; }
    public string SourceUri { get; }
    public ReactiveCommand Edit { get; }
    public ReactiveCommand Remove { get; }

    private async ValueTask EditImpl(Unit unit, CancellationToken token = default)
    {
        using var viewModel = new SourceDialogViewModel(_loggerFactory, this);
        var dialog = new ContentDialog(viewModel, _navigationService)
        {
            Title = RS.PluginsSourceViewModel_EditDialog_Title,
            PrimaryButtonText = RS.PluginsSourceViewModel_EditDialog_PrimaryButtonText,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = Avalonia.RS.DialogButton_Cancel,
        };

        viewModel.ApplyDialog(dialog);

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await Rise(
                new UpdatePluginsSourceEvent(
                    this,
                    Model,
                    new PluginServer(
                        viewModel.Name.Value,
                        viewModel.SourceUri.Value,
                        viewModel.Username.Value,
                        viewModel.Password.Value
                    )
                )
            );
        }
    }

    private async ValueTask RemoveImpl(Unit unit, CancellationToken cancel = default)
    {
        var payload = new YesOrNoDialogPayload
        {
            Title = RS.PluginsSourceViewModel_RemoveDialog_Title,
            Message = RS.PluginsSourceViewModel_RemoveDialog_Message,
        };

        var result = await _yesOrNoDialogPrefab.ShowDialogAsync(payload);

        if (!result)
        {
            return;
        }

        await Rise(new RemovePluginsSourceEvent(this, Model));
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
