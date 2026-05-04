namespace Asv.Avalonia;

public interface IHasVisibility : IViewModel
{
    bool IsVisible { get; set; }
}