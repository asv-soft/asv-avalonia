using System.Composition;
using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public sealed class HomePageLogViewerExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, R3.CompositeDisposable contextDispose)
    {
        var logReader = AppHost.Instance.Services.GetService<ILogReaderService>();
        if (logReader is null)
        {
            return;
        }

        context.Tools.Add(
            OpenLogViewerCommand
                .StaticInfo.CreateAction(
                    loggerFactory,
                    RS.OpenLogViewerCommand_Action_Title,
                    RS.OpenLogViewerCommand_Action_Description
                )
                .DisposeItWith(contextDispose)
        );
    }
}
