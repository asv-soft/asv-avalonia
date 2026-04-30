using System.Windows.Input;
using Asv.Modeling;
using Material.Icons;

namespace Asv.Avalonia;

public interface IPage : IViewModel, IHasUndoHistory<IViewModel>, IHasIcon, IHasStatusIcon
{
    
    string Title { get; }
    ICommandHistory History { get; }
    ICommand TryClose { get; }
    ValueTask TryCloseAsync(bool force);
}