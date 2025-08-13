using Avalonia.Controls;

namespace Asv.Avalonia;

public class DockItem
{
    public required string Id { get; init; }
    public required TabItem TabControl { get; init; }
}
