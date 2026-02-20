using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

public class HomePagePluginExtension(ILoggerFactory loggerFactory) : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
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
