using Material.Icons;

namespace Asv.Avalonia;

public interface IHasIcon : IViewModel
{
    MaterialIconKind? Icon { get; set; }
    AsvColorKind IconColor { get; set; }
}