using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.LogViewer;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public sealed class HomePageLogViewerExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, R3.CompositeDisposable contextDispose)
    {
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
