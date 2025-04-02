using Asv.Common;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePage>]
public class HomePageFlightExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenFlightModeCommand.StaticInfo.CreateAction().DisposeItWith(contextDispose)
        );
    }
}
