using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class HomePageSettingsExtension(ILoggerFactory loggerFactory) : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
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
