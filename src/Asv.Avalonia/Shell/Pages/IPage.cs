using System.Windows.Input;
using Asv.Avalonia.Routable;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public interface IPage : IRoutable, IExportable
{
    MaterialIconKind Icon { get; }
    string Title { get; }
    ICommandHistory History { get; }
    BindableReactiveProperty<bool> HasChanges { get; }
    ICommand TryClose { get; }
    ValueTask TryCloseAsync();
}
