namespace Asv.Avalonia;

internal sealed class WorkspacePanelConfig
{
    public double? LeftWidth { get; set; }
    public double? RightWidth { get; set; }
    public double? BottomHeight { get; set; }
    public bool IsLeftPaneOpen { get; set; } = true;
    public bool IsRightPaneOpen { get; set; } = true;
}
