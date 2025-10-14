using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class ActionViewModel(string id, ILayoutService layoutService, ILoggerFactory loggerFactory)
    : HeadlinedViewModel(id, layoutService, loggerFactory),
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
