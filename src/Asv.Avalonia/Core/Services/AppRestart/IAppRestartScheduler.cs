namespace Asv.Avalonia;

public interface IAppRestartScheduler
{
    void Schedule();
    void Cancel();
    bool IsScheduled { get; }
}
