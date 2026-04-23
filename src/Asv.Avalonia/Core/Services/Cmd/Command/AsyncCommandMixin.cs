using System.Windows.Input;

namespace Asv.Avalonia;

public static class AsyncCommandMixin
{
    public static ICommand CreateSystemCommand(this ICommandInfo cmdInfo, IViewModel parent)
    {
        return new BindableAsyncCommand(cmdInfo.Id, parent);
    }
}
