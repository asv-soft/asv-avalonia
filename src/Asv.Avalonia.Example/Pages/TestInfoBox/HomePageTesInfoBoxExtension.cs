using Asv.Common;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePage>]
public class HomePageTesInfoBoxExtension : AsyncDisposableOnce, IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenTestInfoBoxPageCommand.StaticInfo.CreateAction().DisposeItWith(contextDispose)
        );
    }
}
