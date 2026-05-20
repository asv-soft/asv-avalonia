using System.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public class NullSoloRunFeature : IHostedService
{
    public static NullSoloRunFeature Instance { get; } = new NullSoloRunFeature();

    public NullSoloRunFeature()
    {
        Mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName + ".Design", out _);
        IsFirstInstance = true;
    }

    public bool IsFirstInstance { get; }
    public Mutex Mutex { get; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Mutex.Dispose();
        GC.SuppressFinalize(this);
    }
}
