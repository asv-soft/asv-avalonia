using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

/// <summary>
/// Represents a base view model with a title (header) and an optional icon.
/// This can be used as a foundation for view models that require a title and icon representation.
/// </summary>
public class HeadlinedViewModel(NavigationId id, ILoggerFactory loggerFactory)
    : RoutableViewModel(id, loggerFactory),
        IHeadlinedViewModel
{
    /// <summary>
    /// Gets or sets the icon associated with the view model.
    /// </summary>
    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public IBrush? IconBrush
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

    /// <summary>
    /// Gets or sets the header (title) of the view model.
    /// </summary>
    public string? Header
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Description
    {
        get;
        set => SetField(ref field, value);
    }

    public int Order { get; set; }

    public override ValueTask<IRoutable> Navigate(NavigationId id) =>
        ValueTask.FromResult<IRoutable>(this);

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
