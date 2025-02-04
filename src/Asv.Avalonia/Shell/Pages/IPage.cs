using Material.Icons;
using R3;

namespace Asv.Avalonia;

public interface IPage : IRoutable
{
    BindableReactiveProperty<MaterialIconKind> Icon { get; }
    BindableReactiveProperty<string> Title { get; }
    ICommandHistory History { get; }
}
