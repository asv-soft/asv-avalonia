using Avalonia.Controls;
using R3;

namespace Asv.Avalonia;

public partial class TreePageView : UserControl
{
    private readonly SerialDisposable _layout = new();

    public TreePageView()
    {
        InitializeComponent();
        _layout.Disposable = this.RegisterGridColumnPixelWidth(
            "PART_MainGrid.LeftColumnWidth",
            PART_MainGrid,
            0
        );
        DetachedFromVisualTree += (_, _) => _layout.Dispose();
    }
}
