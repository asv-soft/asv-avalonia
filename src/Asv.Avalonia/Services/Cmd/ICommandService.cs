namespace Asv.Avalonia;

public interface ICommandService
{
    IEnumerable<ICommandInfo> Commands { get; }
    IAsyncCommand? CreateCommand(string commandId);
    ICommandHistory CreateHistory(IRoutable owner);
}
