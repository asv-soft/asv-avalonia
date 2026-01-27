using Avalonia.Controls;

namespace Asv.Avalonia;

public struct ColumnConfig
{
    public int Order { get; init; }
    public GridLengthConfig Width { get; init; }
}

public struct GridLengthConfig
{
    public required double Value { get; init; }
    public required GridUnitType GridUnitType { get; init; }
}

public interface IGridColumnLayoutConfig
{
    public IDictionary<string, ColumnConfig> Columns { get; set; }
}
