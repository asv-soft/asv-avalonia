using System.Windows.Input;

namespace Asv.Avalonia;

public class BindableAsyncCommand(string commandId, IRoutable owner) : ICommand
{
    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        owner.ExecuteCommand(commandId, parameter as CommandArg ?? CommandArg.Null);
    }

    public event EventHandler? CanExecuteChanged;
}
