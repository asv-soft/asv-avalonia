using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public interface IDependencyBuilder
{
    IHostApplicationBuilder AppBuilder { get; }
}
