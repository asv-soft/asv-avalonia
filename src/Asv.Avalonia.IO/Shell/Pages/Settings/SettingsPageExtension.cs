using System.Composition;
using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<ISettingsPage>]
[method: ImportingConstructor]
public class SettingsPageExtension(ILoggerFactory loggerFactory) : IExtensionFor<ISettingsPage>
{
    public void Extend(ISettingsPage context, CompositeDisposable contextDispose)
    {
        context.Nodes.Add(
            new TreePage(
                SettingsConnectionViewModel.SubPageId,
                RS.SettingsPageExtension_TreePage_Title,
                SettingsConnectionViewModel.Icon,
                SettingsConnectionViewModel.SubPageId,
                NavigationId.Empty,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );
    }
}
