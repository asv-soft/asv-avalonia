using Avalonia.Controls;

namespace Asv.Avalonia;

public class DialogItemTextViewModel : DialogViewModelBase
{
    public const string DialogId = "dialog.item.text";

    public DialogItemTextViewModel()
        : base(DialogId)
    {
        if (Design.IsDesignMode)
        {
            Message = "Example";
        }
    }

    public required string Message { get; set; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
