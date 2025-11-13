using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

public class PluginSourceViewModel : RoutableViewModel
{
    public const string ViewModelIdPart = "source";

    private readonly ILoggerFactory _loggerFactory;
    private readonly INavigationService _navigationService;

    public PluginSourceViewModel()
        : this(NullPluginServerInfo.Instance, DesignTime.Navigation, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PluginSourceViewModel(
        IPluginServerInfo pluginServerInfo,
        INavigationService navigationService,
        ILoggerFactory loggerFactory
    )
        : base(new NavigationId(ViewModelIdPart, pluginServerInfo.SourceUri), loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(pluginServerInfo);
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _loggerFactory = loggerFactory;
        _navigationService = navigationService;

        Name = pluginServerInfo.Name;
        SourceUri = pluginServerInfo.SourceUri;
        Model = pluginServerInfo;
        IsEnabled = Observable.Return(true);

        Edit = IsEnabled.ToReactiveCommand<Unit>(EditImpl).DisposeItWith(Disposable);
        Edit.IgnoreOnErrorResume(ex => Logger.LogError(ex, "Error to update plugin server list"));
        Remove = IsEnabled.ToReactiveCommand<Unit>(RemoveImpl).DisposeItWith(Disposable);
        Remove.IgnoreOnErrorResume(ex => Logger.LogError(ex, "Error to remove plugin server"));
    }

    public IPluginServerInfo Model { get; }
    public string Name { get; }
    public string SourceUri { get; }
    public ReactiveCommand<Unit> Edit { get; }
    public ReactiveCommand<Unit> Remove { get; }
    public Observable<bool> IsEnabled { get; set; }

    private async ValueTask EditImpl(Unit unit, CancellationToken token = default)
    {
        using var viewModel = new SourceDialogViewModel(_loggerFactory, this);
        var dialog = new ContentDialog(viewModel, _navigationService)
        {
            Title = RS.PluginsSourcesViewModel_EditImpl_Title,
            PrimaryButtonText = RS.PluginsSourcesViewModel_EditImpl_Save,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.PluginsSourcesViewModel_AddImpl_Cancel,
        };

        viewModel.ApplyDialog(dialog);

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await Rise(
                new UpdatePluginSourceEvent(
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
        await Rise(new RemovePluginSourceEvent(this, Model));
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
