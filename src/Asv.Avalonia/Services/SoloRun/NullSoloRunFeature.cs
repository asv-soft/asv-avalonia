using System.Diagnostics;
using R3;

namespace Asv.Avalonia;

public class NullSoloRunFeature : ISoloRunFeature
{
    public static ISoloRunFeature Instance { get; } = new NullSoloRunFeature();

    public NullSoloRunFeature()
    {
        Args = new ReactiveProperty<IAppArgs>(NullAppArgs.Instance);
        Mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName + ".Design", out _);
        IsFirstInstance = true;
    }

    public bool IsFirstInstance { get; }
    public Mutex Mutex { get; }
    public ReadOnlyReactiveProperty<IAppArgs> Args { get; }

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
        Args.Dispose();
        GC.SuppressFinalize(this);
    }
}
