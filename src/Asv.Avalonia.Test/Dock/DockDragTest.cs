using Asv.Modeling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public class DockDragTest(WorkspaceUiTestFixture fixture)
{
    [Fact]
    public Task ClickAndSmallMove_DoNotCreatePreviewOrDetach() => Run(() =>
    {
        using var scene = new Scene();
        var point = scene.Press();
        scene.Window.MouseMove(point + new Point(3, 2), RawInputModifiers.LeftMouseButton);
        Assert.Empty(scene.PreviewWindows);
        scene.Window.MouseUp(point, MouseButton.Left);
        Assert.Empty(scene.DetachedWindows);
        Assert.Equal(1, scene.Header.Opacity);
    });

    [Fact]
    public Task Drag_ShowsNonActivatingPreviewAndTracksPointer() => Run(() =>
    {
        using var scene = new Scene();
        var point = scene.Press();
        scene.Window.MouseMove(point + new Point(40, 40), RawInputModifiers.LeftMouseButton);
        var preview = Assert.Single(scene.PreviewWindows);
        Assert.False(preview.ShowActivated);
        Assert.False(preview.ShowInTaskbar);
        Assert.InRange(scene.Header.Opacity, 0.1, 0.9);
        var old = preview.Position;
        scene.Window.MouseMove(point + new Point(80, 60), RawInputModifiers.LeftMouseButton);
        Assert.NotEqual(old, preview.Position);
        Assert.Empty(scene.DetachedWindows);
    });

    [Fact]
    public Task Escape_CancelsPreviewAndKeepsPageAndSelection() => Run(() =>
    {
        using var scene = new Scene();
        var point = scene.Press();
        scene.Window.MouseMove(point + new Point(40, 40), RawInputModifiers.LeftMouseButton);
        Assert.Single(scene.PreviewWindows);
        scene.Window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        scene.Window.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        scene.Window.MouseUp(new Point(680, 40), MouseButton.Left);
        Assert.Empty(scene.DetachedWindows);
        Assert.Same(scene.Page, scene.Dock.SelectedItem);
        Assert.Equal(1, scene.Header.Opacity);
    });

    [Fact]
    public Task ReleaseInsideOffsetHost_UsesWindowClientCoordinates() => Run(() =>
    {
        using var scene = new Scene(new Thickness(60, 60, 80, 80));
        var point = scene.Press();
        scene.Window.MouseMove(new Point(570, 350), RawInputModifiers.LeftMouseButton);
        scene.Window.MouseUp(new Point(570, 350), MouseButton.Left);
        Assert.Empty(scene.DetachedWindows);
        Assert.Same(scene.Page, scene.Dock.SelectedItem);
        Assert.Equal(1, scene.Header.Opacity);
    });

    [Fact]
    public Task ReleaseOutside_OpensPageNearDropAndClosingWindowReattachesIt() => Run(() =>
    {
        using var scene = new Scene();
        scene.Press();
        scene.Window.MouseMove(new Point(680, 80), RawInputModifiers.LeftMouseButton);
        scene.Window.MouseUp(new Point(680, 80), MouseButton.Left);
        var detached = Assert.Single(scene.DetachedWindows);
        Assert.Same(scene.Page, detached.Page);
        Assert.True(detached.Position.X > scene.Window.Position.X);
        detached.Close();
        Dispatcher.UIThread.RunJobs();
        Assert.Same(scene.Page, scene.Dock.SelectedItem);
        Assert.Single(scene.Dock.GetVisualDescendants().OfType<DockTabStripItem>());
    });

    [Fact]
    public Task RemovingDraggedPage_ClosesPreviewAndClearsCapture() => Run(() =>
    {
        using var scene = new Scene();
        var point = scene.Press();
        scene.Window.MouseMove(point + new Point(40, 40), RawInputModifiers.LeftMouseButton);
        scene.Dock.Items.Remove(scene.Page);
        Assert.Empty(scene.PreviewWindows);
        scene.Window.MouseUp(new Point(680, 40), MouseButton.Left);
        Assert.Empty(scene.DetachedWindows);
    });

    [Fact]
    public Task CaptureLoss_CancelsPreviewWithoutStealingNewCapture() => Run(() =>
    {
        using var scene = new Scene();
        var point = scene.Press();
        scene.Window.MouseMove(point + new Point(40, 40), RawInputModifiers.LeftMouseButton);
        scene.Pointer!.Capture(scene.Dock);
        Assert.Empty(scene.PreviewWindows);
        Assert.Same(scene.Dock, scene.Pointer.Captured);
        Assert.Equal(1, scene.Header.Opacity);
        scene.Window.MouseUp(new Point(680, 40), MouseButton.Left);
        Assert.Empty(scene.DetachedWindows);
        scene.Pointer.Capture(null);
    });

    [Fact]
    public Task ReopeningDetachedPage_RestoresAndActivatesExistingWindow() => Run(() =>
    {
        using var scene = new Scene();
        scene.Press();
        scene.Window.MouseMove(new Point(680, 80), RawInputModifiers.LeftMouseButton);
        scene.Window.MouseUp(new Point(680, 80), MouseButton.Left);
        var detached = Assert.Single(scene.DetachedWindows);
        detached.WindowState = WindowState.Minimized;
        scene.Window.Activate();
        scene.Dock.SelectedItem = null;
        scene.Dock.SelectedItem = scene.Page;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(WindowState.Normal, detached.WindowState);
        Assert.True(detached.IsActive);
        Assert.Same(detached, Assert.Single(scene.DetachedWindows));
        Assert.Same(scene.Page, detached.Page);
    });

    private Task Run(Action action) => fixture.Session.Dispatch(action, TestContext.Current.CancellationToken);

    private sealed class Scene : IDisposable
    {
        private readonly List<Window> _opened = [];
        private readonly IDisposable _openedSubscription;
        private readonly StyleInclude? _theme;
        public IPointer? Pointer { get; private set; }
        public Window Window { get; }
        public DockControl Dock { get; }
        public TestPage Page { get; } = new();
        public DockTabStripItem Header => Dock.GetVisualDescendants().OfType<DockTabStripItem>().Single();
        public IEnumerable<Window> PreviewWindows => _opened.Where(window => window.IsVisible && window.Classes.Contains("dock-drag-preview"));
        public IEnumerable<DockWindow> DetachedWindows => _opened.OfType<DockWindow>().Where(window => window.IsVisible);
        public Scene(Thickness margin = default)
        {
            if (!Application.Current!.Styles.OfType<StyleInclude>().Any(style => style.Source?.AbsoluteUri == "avares://Asv.Avalonia/Theme.axaml"))
            {
                _theme = new StyleInclude(new Uri("avares://Asv.Avalonia/")) { Source = new Uri("avares://Asv.Avalonia/Theme.axaml") };
                Application.Current.Styles.Add(_theme);
            }
            _openedSubscription = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) => _opened.Add(window));
            Dock = new DockControl { Margin = margin };
            Window = new Window { Width = 600, Height = 400, Content = Dock };
            Window.AddHandler(InputElement.PointerPressedEvent, (_, e) => Pointer = e.Pointer, global::Avalonia.Interactivity.RoutingStrategies.Tunnel, handledEventsToo: true);
            Window.Show();
            Dock.Items.Add(Page);
            Dock.SelectedItem = Page;
            Dispatcher.UIThread.RunJobs();
        }
        public Point Press()
        {
            var point = Header.TranslatePoint(new Point(20, 12), Window)!.Value;
            Window.MouseDown(point, MouseButton.Left);
            return point;
        }
        public void Dispose()
        {
            // Closing the main view first cancels any preview before releasing page resources.
            Window.Close();
            foreach (var window in _opened.Where(window => window.IsVisible).ToArray()) window.Close();
            _openedSubscription.Dispose();
            Page.Dispose();
            if (_theme != null) Application.Current!.Styles.Remove(_theme);
        }
    }

    private sealed class TestPage : PageViewModel<IPage>
    {
        public TestPage() : base("dock-test", NullPageContext.Instance, NullLoggerFactory.Instance, NullDialogService.Instance, NullExtensionService.Instance) { Header = "Test page"; }
        protected override void AfterLoadExtensions() { }
    }
}
