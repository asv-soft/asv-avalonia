using System.Composition;
using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

// Plugin version: v1.0.3-example.1
[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePagePluginExtension(ILoggerFactory loggerFactory) : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            new ActionViewModel("plugin_action", loggerFactory)
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
