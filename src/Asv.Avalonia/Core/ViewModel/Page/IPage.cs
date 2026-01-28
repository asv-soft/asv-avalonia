using System.Windows.Input;
using Material.Icons;

namespace Asv.Avalonia;

public interface IPage : IRoutable, IExportable
{
    MaterialIconKind Icon { get; }
    AsvColorKind IconColor { get; }

    MaterialIconKind? Status { get; }
    AsvColorKind StatusColor { get; }
    string Title { get; }
    ICommandHistory History { get; }
    ICommand TryClose { get; }
    ValueTask TryCloseAsync(bool force);
}
