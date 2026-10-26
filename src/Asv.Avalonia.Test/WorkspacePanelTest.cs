using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.VisualTree;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public class WorkspacePanelTest(WorkspaceUiTestFixture fixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task PaneLayout_DoesNotAddControlsOrReserveHeaderSpace(bool initiallyOpen) => Run(() =>
    {
        var panel = CreatePanel();
        foreach (var open in new[] { initiallyOpen, !initiallyOpen })
        {
            SetPaneOpen(panel, true, open);
            SetPaneOpen(panel, false, open);
            Layout(panel);

            Assert.Empty(panel.GetVisualDescendants().OfType<Button>());
            var grid = panel.GetVisualChildren().OfType<Grid>().Single();
            var center = grid.Children.OfType<Grid>().Single();
            Assert.Equal(0, center.Bounds.Y);
            Assert.Equal(panel.Bounds.Height, center.Bounds.Height);
            if (open)
            {
                Assert.All(grid.Children.OfType<ScrollViewer>(), side =>
                {
                    Assert.Equal(0, side.Bounds.Y);
                    Assert.Equal(panel.Bounds.Height, side.Bounds.Height);
                });
            }
            else
            {
                Assert.Equal(0, GetPaneWidth(panel, true));
                Assert.Equal(0, GetPaneWidth(panel, false));
                Assert.True(GetWidget(panel, null).Bounds.Width > 900);
            }
        }
    });

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task ClosingPane_ReclaimsWidthDespiteMinimumAndPreservesWidgets(bool left) => Run(() =>
    {
        var panel = CreatePanel();
        var hidden = GetWidget(panel, left);
        hidden.IsExpanded = false;
        Layout(panel);
        var oppositeWidth = GetWidget(panel, !left).Bounds.Width;
        var centerWidth = GetWidget(panel, null).Bounds.Width;

        SetPaneOpen(panel, left, false);
        Layout(panel);

        Assert.Equal(0, GetPaneWidth(panel, left));
        Assert.Equal(oppositeWidth, GetWidget(panel, !left).Bounds.Width, 3);
        Assert.True(GetWidget(panel, null).Bounds.Width > centerWidth);
        Assert.Contains(hidden, panel.Children);
        Assert.True(hidden.IsVisible);
        Assert.False(hidden.IsExpanded);
        Assert.False(GetPaneOpen(panel, left));
        Assert.True(GetPaneOpen(panel, !left));
    });

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task ClosingThenOpeningPane_RestoresItsNonzeroWidth(bool left) => Run(() =>
    {
        var panel = CreatePanel();
        Layout(panel);
        var width = GetWidget(panel, left).Bounds.Width;

        SetPaneOpen(panel, left, false);
        Layout(panel);
        SetPaneOpen(panel, left, true);
        Layout(panel);

        Assert.Equal(width, GetWidget(panel, left).Bounds.Width, 3);
        Assert.True(GetPaneOpen(panel, left));
        Assert.True(GetPaneOpen(panel, !left));
    });

    [Fact]
    public Task ClosingBothPanes_ReclaimsBothColumns() => Run(() =>
    {
        var panel = CreatePanel();
        Layout(panel);

        SetPaneOpen(panel, true, false);
        SetPaneOpen(panel, false, false);
        Layout(panel);

        Assert.Equal(0, GetPaneWidth(panel, true));
        Assert.Equal(0, GetPaneWidth(panel, false));
        Assert.False(GetPaneOpen(panel, true));
        Assert.False(GetPaneOpen(panel, false));
        Assert.True(GetWidget(panel, null).Bounds.Width > 900);
    });

    [Fact]
    public Task MovingLastWidgetAwayAndBack_PreservesClosedPreference() => Run(() =>
    {
        var panel = CreatePanel();
        Layout(panel);
        var left = GetWidget(panel, true);
        SetPaneOpen(panel, true, false);

        WorkspacePanel.SetDock(left, WorkspaceDock.Right);
        Layout(panel);

        Assert.False(GetPaneContent(panel, true));
        Assert.False(GetPaneOpen(panel, true));

        WorkspacePanel.SetDock(left, WorkspaceDock.Left);
        Layout(panel);

        Assert.True(GetPaneContent(panel, true));
        Assert.Equal(0, GetPaneWidth(panel, true));
        Assert.False(GetPaneOpen(panel, true));
    });

    [Fact]
    public Task NoVisibleSideWidgets_ReclaimsSpaceAndKeepsPreferences() => Run(() =>
    {
        var panel = CreatePanel();
        GetWidget(panel, true).IsVisible = false;
        GetWidget(panel, false).IsVisible = false;

        Layout(panel);

        Assert.False(GetPaneContent(panel, true));
        Assert.False(GetPaneContent(panel, false));
        Assert.True(GetPaneOpen(panel, true));
        Assert.True(GetPaneOpen(panel, false));
        Assert.Equal(0, GetPaneWidth(panel, true));
        Assert.Equal(0, GetPaneWidth(panel, false));
        Assert.True(GetWidget(panel, null).Bounds.Width > 900);
    });

    [Fact]
    public Task PaneToggles_RaiseOneLayoutChangeForEachStateChange() => Run(() =>
    {
        var panel = CreatePanel();
        Layout(panel);
        var changes = 0;
        panel.WorkspaceChanged += (_, _) => changes++;

        panel.IsLeftPaneOpen = false;
        panel.IsLeftPaneOpen = true;
        panel.IsLeftPaneOpen = true;

        Assert.Equal(2, changes);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task ThemedWorkspace_ExternalTogglesRemainBoundAfterReattach(bool customPanel) => Run(() =>
    {
        var theme = new StyleInclude(new Uri("avares://Asv.Avalonia/"))
        {
            Source = new Uri("avares://Asv.Avalonia/Theme.axaml"),
        };
        Application.Current!.Styles.Add(theme);
        var widgets = new[]
        {
            new WorkspaceWidget("left-1") { Position = WorkspaceDock.Left },
            new WorkspaceWidget("left-2") { Position = WorkspaceDock.Left },
            new WorkspaceWidget("center") { Position = WorkspaceDock.Center },
            new WorkspaceWidget("right-1") { Position = WorkspaceDock.Right },
            new WorkspaceWidget("right-2") { Position = WorkspaceDock.Right },
            new WorkspaceWidget("bottom") { Position = WorkspaceDock.Bottom },
        };
        var workspace = new Workspace { ItemsSource = widgets };
        if (customPanel)
        {
            workspace.ItemsPanel = new FuncTemplate<Panel?>(() => new WorkspacePanel
            {
                LeftWidth = new GridLength(240),
                MinLeftWidth = 180,
                RightWidth = new GridLength(260),
                MinRightWidth = 200,
            });
        }

        var leftToggle = CreatePaneToggle(workspace, true);
        var rightToggle = CreatePaneToggle(workspace, false);
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { leftToggle, rightToggle },
        };
        DockPanel.SetDock(toolbar, Dock.Top);
        var page = new DockPanel { Children = { toolbar, workspace } };
        var window = new Window { Width = 1024, Height = 640, Content = page };
        try
        {
            window.Show();
            window.UpdateLayout();
            var panel = Assert.IsType<WorkspacePanel>(workspace.ItemsPanelRoot);
            Assert.True(leftToggle.IsChecked);
            Assert.True(rightToggle.IsChecked);
            foreach (var (left, open) in new[] { (true, false), (false, false), (true, true), (false, true), (true, false) })
            {
                var toggle = left ? leftToggle : rightToggle;
                toggle.SetCurrentValue(ToggleButton.IsCheckedProperty, open);
                window.UpdateLayout();
                Assert.Equal(open, left ? workspace.IsLeftPaneOpen : workspace.IsRightPaneOpen);
                Assert.Equal(open, GetPaneOpen(panel, left));
                Assert.Equal(open, GetPaneWidth(panel, left) > 0);
            }

            window.Content = null;
            window.Content = page;
            window.UpdateLayout();
            panel = Assert.IsType<WorkspacePanel>(workspace.ItemsPanelRoot);

            Assert.False(workspace.IsLeftPaneOpen);
            Assert.False(panel.IsLeftPaneOpen);
            Assert.False(leftToggle.IsChecked);
            Assert.True(rightToggle.IsChecked);
            Assert.True(panel.IsRightPaneOpen);
            Assert.Equal(0, GetPaneWidth(panel, true));
            Assert.Equal(2, panel.Children.Count(x => WorkspacePanel.GetDock(x) == WorkspaceDock.Right));
            Assert.All(widgets, widget => Assert.True(widget.IsVisible));

            panel.SetCurrentValue(WorkspacePanel.IsLeftPaneOpenProperty, true);
            panel.SetCurrentValue(WorkspacePanel.IsRightPaneOpenProperty, false);
            window.UpdateLayout();

            Assert.True(workspace.IsLeftPaneOpen);
            Assert.False(workspace.IsRightPaneOpen);
            Assert.True(leftToggle.IsChecked);
            Assert.False(rightToggle.IsChecked);
            Assert.True(GetPaneWidth(panel, true) > 0);
            Assert.Equal(0, GetPaneWidth(panel, false));

            leftToggle.SetCurrentValue(ToggleButton.IsCheckedProperty, false);
            rightToggle.SetCurrentValue(ToggleButton.IsCheckedProperty, true);
            window.UpdateLayout();

            Assert.False(panel.IsLeftPaneOpen);
            Assert.True(panel.IsRightPaneOpen);
            Assert.Equal(0, GetPaneWidth(panel, true));
            Assert.True(GetPaneWidth(panel, false) > 0);
        }
        finally
        {
            window.Close();
            Application.Current.Styles.Remove(theme);
            foreach (var widget in widgets)
            {
                widget.Dispose();
            }
        }
    });

    private Task Run(Action action) =>
        fixture.Session.Dispatch(action, TestContext.Current.CancellationToken);

    private static WorkspacePanel CreatePanel()
    {
        var panel = new WorkspacePanel
        {
            LeftWidth = new GridLength(1, GridUnitType.Star),
            CentralWidth = new GridLength(4, GridUnitType.Star),
            RightWidth = new GridLength(2, GridUnitType.Star),
            MinLeftWidth = 120,
            MinRightWidth = 160,
        };
        foreach (var position in new[] { WorkspaceDock.Left, WorkspaceDock.Center, WorkspaceDock.Right })
        {
            var item = new WorkspaceItem { Header = position.ToString() };
            WorkspacePanel.SetDock(item, position);
            panel.Children.Add(item);
        }

        return panel;
    }

    private static WorkspaceItem GetWidget(WorkspacePanel panel, bool? left)
    {
        var dock = left switch
        {
            true => WorkspaceDock.Left,
            false => WorkspaceDock.Right,
            null => WorkspaceDock.Center,
        };
        return panel.Children.OfType<WorkspaceItem>().Single(x => WorkspacePanel.GetDock(x) == dock);
    }

    private static void Layout(WorkspacePanel panel)
    {
        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));
        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));
    }

    private static void SetPaneOpen(WorkspacePanel panel, bool left, bool value)
    {
        if (left)
        {
            panel.IsLeftPaneOpen = value;
        }
        else
        {
            panel.IsRightPaneOpen = value;
        }
    }

    private static bool GetPaneOpen(WorkspacePanel panel, bool left) =>
        left ? panel.IsLeftPaneOpen : panel.IsRightPaneOpen;

    private static bool GetPaneContent(WorkspacePanel panel, bool left) =>
        left ? panel.HasLeftPaneContent : panel.HasRightPaneContent;

    private static double GetPaneWidth(WorkspacePanel panel, bool left) =>
        panel.GetVisualChildren().OfType<Grid>().Single().ColumnDefinitions[left ? 0 : 4].ActualWidth;

    private static ToggleButton CreatePaneToggle(Workspace workspace, bool left)
    {
        var toggle = new ToggleButton { Content = left ? "Filters" : "Details" };
        toggle.Bind(ToggleButton.IsCheckedProperty, new Binding(
            left ? nameof(Workspace.IsLeftPaneOpen) : nameof(Workspace.IsRightPaneOpen)
        )
        {
            Source = workspace,
            Mode = BindingMode.TwoWay,
        });
        return toggle;
    }
}
