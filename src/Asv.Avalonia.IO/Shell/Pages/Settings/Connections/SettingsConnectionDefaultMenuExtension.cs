using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class SettingsConnectionDefaultMenuExtension(
    ILoggerFactory loggerFactory,
    [FromKeyedServices(SettingsConnectionDefaultMenuExtension.Contract)]
        IEnumerable<IMenuItem> menuItems
) : IExtensionFor<ISettingsConnectionSubPage>
{
    public const string StaticId = "ext.settings.connection.menu";

    public const string Contract = "settings.connection.menu";

    string Asv.Modeling.ISupportId<string>.Id => StaticId;

    public void Extend(ISettingsConnectionSubPage context, CompositeDisposable contextDispose)
    {
        foreach (var menuItem in menuItems)
        {
            context.Menu.Add(menuItem);
            menuItem.DisposeItWith(contextDispose);
        }
    }
}
