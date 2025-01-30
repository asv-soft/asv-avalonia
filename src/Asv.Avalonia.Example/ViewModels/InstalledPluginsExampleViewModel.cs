using NuGet.Packaging;
using R3;

namespace Asv.Avalonia.Example.ViewModels;

public class InstalledPluginsExampleViewModel : InstalledPluginsViewModel
{
    public InstalledPluginsExampleViewModel()
        : base("installed_plugins", new NullPluginManager(), NullLogService.Instance) { }
}
