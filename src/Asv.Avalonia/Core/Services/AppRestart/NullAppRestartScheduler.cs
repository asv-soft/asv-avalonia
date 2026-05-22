namespace Asv.Avalonia;

public sealed class NullAppRestartScheduler : IAppRestartScheduler
{
    public static NullAppRestartScheduler Instance { get; } = new();
    public bool IsScheduled => false;

    public void Schedule() { }

    public void Cancel() { }
}
