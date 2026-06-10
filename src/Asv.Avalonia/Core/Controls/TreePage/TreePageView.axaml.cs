using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
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

    private void PART_TreeMenu_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(FocusSelectedPage, DispatcherPriority.Background);
    }

    private void FocusSelectedPage()
    {
        if (IsFocusWithinTreeMenu() && PART_SelectedPageContent.DataContext is ITreeSubpage)
        {
            PART_SelectedPageContent.Focus();
        }
    }

    private bool IsFocusWithinTreeMenu()
    {
        if (TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is not Control control)
        {
            return false;
        }

        Control? current = control;
        while (current is not null)
        {
            if (ReferenceEquals(current, PART_TreeMenu))
            {
                return true;
            }

            current = current.GetLogicalParent() as Control;
        }

        return false;
    }
}
