using Avalonia.Input;

namespace Asv.Avalonia;

public class NullCommandService : ICommandService
{
    public static ICommandService Instance { get; } = new NullCommandService();
    public IEnumerable<ICommandInfo> Commands => [];

    public IAsyncCommand? CreateCommand(string commandId)
    {
        return null;
    }

    public ICommandHistory CreateHistory(IRoutable owner)
    {
        return NullCommandHistory.Instance;
    }

    public bool CanExecuteCommand(string commandId, IRoutable context, out IRoutable? target)
    {
        target = null;
        return false;
    }
}