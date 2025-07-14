using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageTestHistoryPropertiesExtension(ILoggerFactory loggerFactory)
    : AsyncDisposableOnce,
        IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenTestHistoryPropertiesPageCommand
                .StaticInfo.CreateAction(
                    loggerFactory,
                    "History properties",
                    "Opens history properties page"
                )
                .DisposeItWith(contextDispose)
        );
    }
}
