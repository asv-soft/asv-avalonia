using System.Composition;
using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.Example;

public class DialogControlsPageViewConfig
{
    public Vector ScrollOffset { get; set; }
}

[ExportViewFor(typeof(DialogControlsPageViewModel))]
public partial class DialogControlsPageView : UserControl
{
    private readonly ILayoutService _layoutService;

    private DialogControlsPageViewConfig? _config;

    public DialogControlsPageView()
        : this(NullLayoutService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public DialogControlsPageView(ILayoutService layoutService)
    {
        _layoutService = layoutService;
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        LoadLayout();
        base.OnAttachedToVisualTree(e);
    }

    private void LoadLayout()
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        _config = _layoutService.Get<DialogControlsPageViewConfig>(this);

        MainScrollViewer.Offset = _config.ScrollOffset;
    }

    private void SaveLayout()
    {
        if (Design.IsDesignMode || _config is null || DataContext is null)
        {
            return;
        }

        _config.ScrollOffset = MainScrollViewer.Offset;

        _layoutService.SetInMemory(this, _config);
    }

    private void MainScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        SaveLayout();
    }
}
