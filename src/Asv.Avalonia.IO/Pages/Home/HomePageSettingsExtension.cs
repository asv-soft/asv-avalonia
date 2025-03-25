using Asv.Common;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<IHomePage>]
public class HomePageSettingsExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenSettingsCommand.StaticInfo.CreateAction().DisposeItWith(contextDispose)
        );
    }
}
