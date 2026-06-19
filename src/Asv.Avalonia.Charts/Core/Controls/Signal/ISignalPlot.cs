using Avalonia.Media;

namespace Asv.Avalonia.Charts;

public interface ISignalPlot : IViewModel
{
    IBrush? LineColor { get; set; }
    int HistorySize { get; set; }
    void Refresh(double[] data);
}
