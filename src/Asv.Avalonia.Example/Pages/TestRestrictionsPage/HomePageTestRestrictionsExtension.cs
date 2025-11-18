using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public sealed class HomePageTestRestrictionsExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, R3.CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenTestRestrictionsPageCommand
                .StaticInfo.CreateAction(loggerFactory, "Test restrictions", string.Empty)
                .DisposeItWith(contextDispose)
        );
    }
}
