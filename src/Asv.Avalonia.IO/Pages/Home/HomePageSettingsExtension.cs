using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageSettingsExtension(ILoggerFactory loggerFactory) : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenSettingsCommand
                .StaticInfo.CreateAction(loggerFactory, "Settings", "Opens Settings page")
                .DisposeItWith(contextDispose)
        );
    }
}
