using System.Composition;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public sealed class HomePageLogViewerExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, DisposableBag contextDispose)
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
