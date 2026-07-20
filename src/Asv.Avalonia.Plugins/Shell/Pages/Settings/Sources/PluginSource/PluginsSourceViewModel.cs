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
            (_, _) => ValueTask.CompletedTask,
            (_, _) => ValueTask.CompletedTask
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PluginsSourceViewModel(
        IPluginServerInfo pluginServerInfo,
        Func<PluginsSourceViewModel, CancellationToken, ValueTask> edit,
        Func<PluginsSourceViewModel, CancellationToken, ValueTask> remove
    )
        : base(
            ViewModelIdPart,
            new NavArgs(new KeyValuePair<string, string?>("source", pluginServerInfo.SourceUri))
        )
    {
        ArgumentNullException.ThrowIfNull(pluginServerInfo);
        ArgumentNullException.ThrowIfNull(edit);
        ArgumentNullException.ThrowIfNull(remove);

        Name = pluginServerInfo.Name;
        SourceUri = pluginServerInfo.SourceUri;
        Model = pluginServerInfo;
        Edit = new ReactiveCommand((_, cancel) => edit(this, cancel)).DisposeItWith(Disposable);
        Remove = new ReactiveCommand((_, cancel) => remove(this, cancel)).DisposeItWith(Disposable);
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
