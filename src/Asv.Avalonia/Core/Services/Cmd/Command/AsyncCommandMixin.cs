using System.Windows.Input;

namespace Asv.Avalonia;

public static class AsyncCommandMixin
{
    public static ICommand CreateSystemCommand(this ICommandInfo cmdInfo, IRoutable parent)
    {
        return new BindableAsyncCommand(cmdInfo.Id, parent);
    }
}
