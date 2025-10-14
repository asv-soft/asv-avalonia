using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageSettingsExtension(ILayoutService layoutService, ILoggerFactory loggerFactory)
    : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenSettingsCommand
                .StaticInfo.CreateAction(
                    layoutService,
                    loggerFactory,
                    RS.OpenSettingsCommand_Action_Title,
                    RS.OpenSettingsCommand_Action_Description
                )
                .DisposeItWith(contextDispose)
        );
    }
}
