using System.Windows.Input;

namespace Asv.Avalonia;

public class AsyncReactiveCommand(string commandId, IRoutable owner) : ICommand
{
    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        owner.ExecuteCommand(commandId, parameter as IPersistable);
    }

    public event EventHandler? CanExecuteChanged;
}
