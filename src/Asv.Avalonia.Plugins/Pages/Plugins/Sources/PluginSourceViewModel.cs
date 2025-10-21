using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

public class PluginSourceViewModel : RoutableViewModel
{
    public const string ViewModelIdPart = "plugins.sources.source";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public PluginSourceViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        : base(DesignTime.Id, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PluginSourceViewModel(
        IPluginServerInfo pluginServerInfo,
        ILoggerFactory loggerFactory,
        PluginsSourcesViewModel sourcesViewModel
    )
        : base($"{ViewModelIdPart}.{NavigationId.GenerateRandomAsString()}", loggerFactory)
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

    public override ValueTask<IRoutable> Navigate(NavigationId id)
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
