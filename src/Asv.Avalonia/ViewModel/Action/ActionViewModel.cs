using System.Windows.Input;

namespace Asv.Avalonia;

public class ActionViewModel(string typeId)
    : HeadlinedViewModel(typeId),
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
