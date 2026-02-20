using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

public class HomePageInstalledPluginsExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenInstalledPluginsCommand
                .StaticInfo.CreateAction(
                    loggerFactory,
                    RS.OpenInstalledPluginsCommand_Action_Title,
                    RS.OpenInstalledPluginsCommand_Action_Description
                )
                .DisposeItWith(contextDispose)
        );
    }
}
