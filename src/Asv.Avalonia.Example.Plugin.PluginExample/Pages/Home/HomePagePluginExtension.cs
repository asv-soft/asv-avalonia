using Avalonia.Media;
using Material.Icons;
using R3;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

[ExportExtensionFor<IHomePage>]
public class HomePagePluginExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            new ActionViewModel("plugin_action")
            {
                Parent = null,
                Icon = MaterialIconKind.Plugin,
                IconBrush = Brushes.Red,
                Header = "Plugin example action",
                Description = "This is example action from plugin (do nothing)",
                Order = 0,
                Command = null,
                CommandParameter = null,
                IsVisible = true,
            }
        );
    }
}
