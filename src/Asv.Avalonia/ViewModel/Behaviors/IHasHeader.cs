namespace Asv.Avalonia;

public interface IHasHeader : IViewModel
{
    /// <summary>
    /// Gets or sets the header (title) of the view model.
    /// </summary>
    string? Header { get; set; }
}