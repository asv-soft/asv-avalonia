using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageInstalledPluginsExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenInstalledPluginsCommand
                .StaticInfo.CreateAction(
                    loggerFactory,
                    "Installed plugins",
                    "Opens installed plugins page"
                )
                .DisposeItWith(contextDispose)
        );
    }
}
