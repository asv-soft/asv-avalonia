using Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public class AvaloniaLifetime(Func<AppBuilder> configureApp) : IHostLifetime
{
    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public AppBuilder AppBuilder => configureApp();
}
