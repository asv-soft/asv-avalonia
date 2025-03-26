using System.Windows.Input;

namespace Asv.Avalonia;

public class InternalContextCommand(string commandId, IRoutable owner, ICommandArg newValue)
    : ICommand
{
    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        owner.ExecuteCommand(commandId, newValue);
    }

    public event EventHandler? CanExecuteChanged;
}
