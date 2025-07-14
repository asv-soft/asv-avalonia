using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageDialogBoardExtension(ILoggerFactory loggerFactory)
    : AsyncDisposableOnce,
        IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenDialogBoardCommand
                .StaticInfo.CreateAction(
                    loggerFactory,
                    RS.OpenDialogBoard_CommandInfo_ButtonName,
                    RS.OpenDialogBoard_CommandInfo_ButtonDescription
                )
                .DisposeItWith(contextDispose)
        );
    }
}
