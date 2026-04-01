using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

public class HomePageMapTestPageExtension(ILoggerFactory loggerFactory) : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenMapTestPageCommand
                .StaticInfo.CreateAction(
                    loggerFactory,
                    RS.OpenMapTestPageCommand_Action_Name,
                    RS.OpenMapTestPageCommand_Action_Description
                )
                .DisposeItWith(contextDispose)
        );
    }
}
