namespace Asv.Avalonia;

public interface IHasDescription : IViewModel
{
    string? Description { get; set; }
}