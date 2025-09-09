using System.Windows.Input;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public interface IPage : IRoutable, IExportable
{
    MaterialIconKind Icon { get; }
    string Title { get; }
    ICommandHistory History { get; }
    BindableReactiveProperty<bool> HasChanges { get; }
    ReactiveProperty<PageState> State { get; }
    ICommand TryClose { get; }
    ValueTask TryCloseAsync(bool force);
    ValueTask TrySaveStateAsync(bool force);
}
