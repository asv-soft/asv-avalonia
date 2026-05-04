using System.Windows.Input;
using Asv.Modeling;
using Material.Icons;

namespace Asv.Avalonia;

public interface IPage : IHasUndoHistory<IViewModel>, IHasIcon, IHasStatusIcon, IHasHeader
{
    ICommand TryClose { get; }
    ValueTask TryCloseAsync(bool force);
}