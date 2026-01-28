using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class ActionViewModel(string id, ILoggerFactory loggerFactory)
    : HeadlinedViewModel(id, loggerFactory),
        IActionViewModel
{
    public ICommand? Command
    {
        get;
        set => SetField(ref field, value);
    }

    public object? CommandParameter
    {
        get;
        set => SetField(ref field, value);
    }
}
