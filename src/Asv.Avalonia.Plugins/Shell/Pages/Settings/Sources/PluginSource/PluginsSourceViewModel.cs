using System.Windows.Input;
using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.Plugins;

public class PluginsSourceViewModel : ViewModel
{
    public const string ViewModelIdPart = "source";

    public PluginsSourceViewModel()
        : this(
            NullPluginServerInfo.Instance,
            _ => ValueTask.CompletedTask,
            _ => ValueTask.CompletedTask
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PluginsSourceViewModel(
        IPluginServerInfo pluginServerInfo,
        Func<PluginsSourceViewModel, ValueTask> edit,
        Func<PluginsSourceViewModel, ValueTask> remove
    )
        : base(
            ViewModelIdPart,
            new NavArgs(new KeyValuePair<string, string>("source", pluginServerInfo.SourceUri))
        )
    {
        ArgumentNullException.ThrowIfNull(pluginServerInfo);
        ArgumentNullException.ThrowIfNull(edit);
        ArgumentNullException.ThrowIfNull(remove);

        Name = pluginServerInfo.Name;
        SourceUri = pluginServerInfo.SourceUri;
        Model = pluginServerInfo;
        Edit = new ReactiveCommand((_, _) => edit(this)).DisposeItWith(Disposable);
        Remove = new ReactiveCommand((_, _) => remove(this)).DisposeItWith(Disposable);
    }

    public IPluginServerInfo Model { get; }
    public string Name { get; }
    public string SourceUri { get; }
    public ICommand Edit { get; }
    public ICommand Remove { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
