using Avalonia.Controls;

namespace Asv.Avalonia;

public struct GridLengthCfg
{
    public required double Width { get; init; }
    public required GridUnitType GridUnitType { get; init; }
}

public interface IColumnWidthLayoutConfig
{
    public IList<GridLengthCfg> ColumnsWidth { get; set; }
}
