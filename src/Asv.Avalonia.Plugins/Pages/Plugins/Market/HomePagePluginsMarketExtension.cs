using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePagePluginsMarketExtension(ILoggerFactory loggerFactory) : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenPluginsMarketCommand
                .StaticInfo.CreateAction(loggerFactory, "Plugins Market", "Open plugins market")
                .DisposeItWith(contextDispose)
        );
    }
}
