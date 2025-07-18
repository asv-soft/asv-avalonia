using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public sealed class HomePageLogViewerExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, R3.CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenLogViewerCommand
                .StaticInfo.CreateAction(loggerFactory)
                .DisposeItWith(contextDispose)
        );
    }
}
