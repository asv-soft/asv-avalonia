using Material.Icons;

namespace Asv.Avalonia;

public interface ITitledViewModel : IViewModel
{
    /// <summary>
    /// Gets or sets the icon associated with the view model.
    /// </summary>
    MaterialIconKind? Icon { get; set; }

    /// <summary>
    /// Gets or sets the header (title) of the view model.
    /// </summary>
    string? Title { get; set; }
}

/// <summary>
/// Represents a base view model with a title (header) and an optional icon.
/// This can be used as a foundation for view models that require a title and icon representation.
/// </summary>
public abstract class TitledViewModel(string id) : RoutableViewModel(id), ITitledViewModel
{
    private string? _title;
    private MaterialIconKind? _icon;

    /// <summary>
    /// Gets or sets the icon associated with the view model.
    /// </summary>
    public MaterialIconKind? Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    /// <summary>
    /// Gets or sets the header (title) of the view model.
    /// </summary>
    public string? Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }
}
