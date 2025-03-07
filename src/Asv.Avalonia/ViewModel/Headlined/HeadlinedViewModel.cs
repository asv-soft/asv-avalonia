using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia;

/// <summary>
/// Represents a base view model with a title (header) and an optional icon.
/// This can be used as a foundation for view models that require a title and icon representation.
/// </summary>
public class HeadlinedViewModel(NavigationId id) : RoutableViewModel(id), IHeadlinedViewModel
{
    private string? _title;
    private MaterialIconKind? _icon;
    private string? _description;
    private IBrush? _iconBrush;

    /// <summary>
    /// Gets or sets the icon associated with the view model.
    /// </summary>
    public MaterialIconKind? Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    public IBrush? IconBrush
    {
        get => _iconBrush;
        set => SetField(ref _iconBrush, value);
    }

    /// <summary>
    /// Gets or sets the header (title) of the view model.
    /// </summary>
    public string? Header
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public string? Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public int Order { get; set; }

    public override ValueTask<IRoutable> Navigate(NavigationId id) => new(this);

    public override IEnumerable<IRoutable> GetRoutableChildren() => [];
}
