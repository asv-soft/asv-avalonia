using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageSettingsExtension(ILoggerFactory loggerFactory) : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, DisposableBag contextDispose)
    {
        context.Tools.Add(
            OpenSettingsCommand
                .StaticInfo.CreateAction(
                    loggerFactory,
                    RS.OpenSettingsCommand_Action_Title,
                    RS.OpenSettingsCommand_Action_Description
                )
                .DisposeItWith(contextDispose)
        );
    }
}
