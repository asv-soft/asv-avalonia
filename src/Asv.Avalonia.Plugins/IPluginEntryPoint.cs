using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Plugins;

public interface IPluginAppBuilder
{
    void Register(IHostApplicationBuilder builder);
}
