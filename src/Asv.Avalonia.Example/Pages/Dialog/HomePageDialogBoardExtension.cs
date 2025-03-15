using Asv.Common;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePage>]
public class HomePageDialogBoardExtension : AsyncDisposableOnce, IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenDialogBoardCommand.StaticInfo.CreateAction().DisposeItWith(contextDispose)
        );
    }
}
