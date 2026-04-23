using System.Windows.Input;
using Asv.Modeling;
using Material.Icons;

namespace Asv.Avalonia;

public interface IPage : IViewModel, IHasUndoHistory<IViewModel>
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