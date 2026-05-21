using System;
using Asv.Modeling;
using Avalonia;
using Avalonia.Controls;
using R3;

namespace Asv.Avalonia.Example;

public class DialogControlsPageViewConfig
{
    public Vector ScrollOffset { get; set; }
}

public partial class DialogControlsPageView : UserControl
{
    private readonly IDisposable? _layout;
    private readonly Subject<Unit>? _layoutChanged;

    public DialogControlsPageView()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            return;
        }

        _layoutChanged = new Subject<Unit>();
        _layout = this.RegisterLayout<DialogControlsPageViewConfig, Unit>(
            nameof(DialogControlsPageView),
            LoadLayout,
            SaveLayout,
            _layoutChanged
        );
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _layout?.Dispose();
        _layoutChanged?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    private void LoadLayout(DialogControlsPageViewConfig config)
    {
        MainScrollViewer.Offset = config.ScrollOffset;
    }

    private DialogControlsPageViewConfig? SaveLayout()
    {
        if (
            !double.IsFinite(MainScrollViewer.Offset.X)
            || !double.IsFinite(MainScrollViewer.Offset.Y)
        )
        {
            return null;
        }

        return new DialogControlsPageViewConfig { ScrollOffset = MainScrollViewer.Offset };
    }

    private void MainScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        _layoutChanged?.OnNext(Unit.Default);
    }
}
