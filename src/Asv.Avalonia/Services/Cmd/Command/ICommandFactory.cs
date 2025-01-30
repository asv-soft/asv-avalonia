namespace Asv.Avalonia;

public interface ICommandFactory
{
    ICommandInfo Info { get; }
    IAsyncCommand Create();
}
