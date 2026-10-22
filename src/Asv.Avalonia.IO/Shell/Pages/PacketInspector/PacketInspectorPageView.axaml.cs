using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input.Platform;

namespace Asv.Avalonia.IO;

public partial class PacketInspectorPageView : Workspace
{
    private IDisposable? _workspaceLayout;

    public PacketInspectorPageView() => InitializeComponent();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _workspaceLayout ??= this.RegisterWorkspaceLayout("PacketInspector.Workspace", this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _workspaceLayout?.Dispose();
        _workspaceLayout = null;
        base.OnDetachedFromVisualTree(e);
    }

    private async void CopyDetailsOnClick(object? sender, RoutedEventArgs args)
    {
        if (sender is not Button { DataContext: PacketInspectorPageViewModel model }
            || string.IsNullOrEmpty(model.DetailsText))
        {
            return;
        }
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(model.DetailsText);
        }
    }

    protected override Type StyleKeyOverride => typeof(Workspace);
}
