using System.Composition.Hosting;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public static class ContainerExtensions
{
    public static ContainerConfiguration WithExportFromModule<TOptions>(
        this ContainerConfiguration containerConfiguration,
        IExportModule<TOptions> module,
        IOptions<TOptions> options
    )
        where TOptions : class
    {
        module.ExportTypes(containerConfiguration, options);
        return containerConfiguration;
    }
}
