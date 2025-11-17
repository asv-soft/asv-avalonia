using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Plugins;

public class PluginsSourceViewModel : RoutableViewModel
{
    public const string ViewModelIdPart = "source";

    public PluginsSourceViewModel()
        : this(NullPluginServerInfo.Instance, DesignTime.Navigation, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PluginsSourceViewModel(
        IPluginServerInfo pluginServerInfo,
        INavigationService navigationService,
        ILoggerFactory loggerFactory
    )
        : base(new NavigationId(ViewModelIdPart, pluginServerInfo.SourceUri), loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(pluginServerInfo);
        ArgumentNullException.ThrowIfNull(navigationService);

        Name = pluginServerInfo.Name;
        SourceUri = pluginServerInfo.SourceUri;
        Model = pluginServerInfo;
        Edit = new BindableAsyncCommand(EditPluginsSourceCommand.Id, this);
        Remove = new BindableAsyncCommand(DeletePluginsSourceCommand.Id, this);
    }

    public IPluginServerInfo Model { get; }
    public string Name { get; }
    public string SourceUri { get; }
    public ICommand Edit { get; }
    public ICommand Remove { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
