using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class PluginSourceViewModel : RoutableViewModel
{
    public const string ViewModelIdPart = "plugins.sources.source";

    public PluginSourceViewModel()
        : base(ViewModelIdPart)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PluginSourceViewModel(
        IPluginServerInfo pluginServerInfo,
        ILoggerFactory loggerFactory,
        PluginsSourcesViewModel sourcesViewModel
    )
        : base($"{ViewModelIdPart}.{Guid.NewGuid().ToString()}")
    {
        ArgumentNullException.ThrowIfNull(pluginServerInfo);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        Parent = sourcesViewModel;
        var logger = loggerFactory.CreateLogger<PluginSourceViewModel>();
        SourceId = new BindableReactiveProperty<string>(pluginServerInfo.SourceUri);
        Name = new BindableReactiveProperty<string>(pluginServerInfo.Name);
        SourceUri = new BindableReactiveProperty<string>(pluginServerInfo.SourceUri);
        Model = pluginServerInfo;
        IsEnabled = Observable.Return(true);

        Edit = IsEnabled.ToReactiveCommand<PluginSourceViewModel>(sourcesViewModel.EditImpl);
        Edit.IgnoreOnErrorResume(ex =>
            logger.LogError(ex, RS.PluginsSourcesViewModel_PluginsSourcesViewModel_ErrorToUpdate)
        );
        Remove = IsEnabled.ToReactiveCommand<PluginSourceViewModel>(sourcesViewModel.RemoveImpl);
        Remove.IgnoreOnErrorResume(ex =>
            logger.LogError(ex, RS.PluginsSourcesViewModel_PluginsSourcesViewModel_ErrorToRemove)
        );
    }

    public IPluginServerInfo Model { get; }
    public BindableReactiveProperty<string> SourceId { get; }
    public BindableReactiveProperty<string> Name { get; }
    public BindableReactiveProperty<string> SourceUri { get; }
    public ReactiveCommand<PluginSourceViewModel> Edit { get; }
    public ReactiveCommand<PluginSourceViewModel> Remove { get; }
    public Observable<bool> IsEnabled { get; set; }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            SourceId.Dispose();
            Name.Dispose();
            SourceUri.Dispose();
            Edit.Dispose();
            Remove.Dispose();
        }

        base.Dispose(isDisposing);
    }
}
