using System.Composition;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePagePluginExtension(ILoggerFactory loggerFactory) : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, DisposableBag contextDispose)
    {
        context.Tools.Add(
            OpenExamplePageCommand.StaticInfo.CreateAction(
                loggerFactory,
                "Plugin example action",
                "This is example action from plugin"
            )
        );
    }
}
