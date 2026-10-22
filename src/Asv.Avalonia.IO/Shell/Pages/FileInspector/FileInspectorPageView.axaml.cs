using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input.Platform;

namespace Asv.Avalonia.IO;

public partial class FileInspectorPageView : Workspace
{
    private IDisposable? _workspaceLayout;

    public FileInspectorPageView() => InitializeComponent();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _workspaceLayout ??= this.RegisterWorkspaceLayout("FileInspector.Workspace", this);
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
