using Material.Icons;

namespace Asv.Avalonia;

public interface IHasStatusIcon : IViewModel
{
    MaterialIconKind? Status { get; }
    AsvColorKind StatusColor { get; }
}