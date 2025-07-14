using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageTesInfoBoxExtension(ILoggerFactory loggerFactory)
    : AsyncDisposableOnce,
        IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenTestInfoBoxPageCommand
                .StaticInfo.CreateAction(loggerFactory, "Test Info", "Opens test info page")
                .DisposeItWith(contextDispose)
        );
    }
}
