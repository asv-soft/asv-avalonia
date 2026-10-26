using Asv.Modeling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.VisualTree;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public class ViewLayoutWorkspaceTest(WorkspaceUiTestFixture fixture)
{
    [Fact]
    public Task Load_OldDimensionOnlySnapshot_RestoresSizesAndOpensBothPanes() =>
        fixture.Session.Dispatch(
            () =>
            {
                using var model = new LayoutViewModel();
                using var manager = new LayoutManager<IViewModel>(
                    model,
                    new JsonLayoutStore("""{"LeftWidth":180,"RightWidth":360,"BottomHeight":90}""")
                );
                var view = new Control { DataContext = model };
                var panel = new WorkspacePanel
                {
                    MinLeftWidth = 200,
                    MinBottomHeight = 100,
                    IsLeftPaneOpen = false,
                    IsRightPaneOpen = false,
                };

                using var registration = view.RegisterWorkspaceLayout("workspace", panel);

                Assert.Equal(new GridLength(200), panel.LeftWidth);
                Assert.Equal(new GridLength(360), panel.RightWidth);
                Assert.Equal(new GridLength(100), panel.BottomHeight);
                Assert.True(panel.IsLeftPaneOpen);
                Assert.True(panel.IsRightPaneOpen);
            },
            TestContext.Current.CancellationToken
        );

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public Task Save_CollapsedPaneWithoutDimensions_PersistsFlags(
        bool isLeftPaneOpen,
        bool isRightPaneOpen
    ) =>
        fixture.Session.Dispatch(
            () =>
            {
                using var model = new LayoutViewModel();
                var store = new JsonLayoutStore();
                using var manager = new LayoutManager<IViewModel>(model, store);
                var view = new Control { DataContext = model };
                var panel = new WorkspacePanel
                {
                    LeftWidth = GridLength.Star,
                    RightWidth = GridLength.Star,
                    BottomHeight = GridLength.Auto,
                };
                using var registration = view.RegisterWorkspaceLayout("workspace", panel);

                panel.IsLeftPaneOpen = isLeftPaneOpen;
                panel.IsRightPaneOpen = isRightPaneOpen;

                var snapshot = Assert.IsType<JObject>(store.Snapshot);
                Assert.Equal(isLeftPaneOpen, snapshot.Value<bool>("IsLeftPaneOpen"));
                Assert.Equal(isRightPaneOpen, snapshot.Value<bool>("IsRightPaneOpen"));
                Assert.Null(snapshot.Value<double?>("LeftWidth"));
                Assert.Null(snapshot.Value<double?>("RightWidth"));
                Assert.Null(snapshot.Value<double?>("BottomHeight"));
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public Task Save_ZeroDimensions_PersistsOnlyPanePreferences() =>
        fixture.Session.Dispatch(
            () =>
            {
                using var model = new LayoutViewModel();
                var store = new JsonLayoutStore();
                using var manager = new LayoutManager<IViewModel>(model, store);
                var view = new Control { DataContext = model };
                var panel = new WorkspacePanel
                {
                    LeftWidth = new GridLength(0),
                    RightWidth = new GridLength(0),
                    BottomHeight = new GridLength(0),
                };
                using var registration = view.RegisterWorkspaceLayout("workspace", panel);

                panel.IsLeftPaneOpen = false;

                var snapshot = Assert.IsType<JObject>(store.Snapshot);
                Assert.Null(snapshot.Value<double?>("LeftWidth"));
                Assert.Null(snapshot.Value<double?>("RightWidth"));
                Assert.Null(snapshot.Value<double?>("BottomHeight"));
                Assert.False(snapshot.Value<bool>("IsLeftPaneOpen"));
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public Task Save_ReopenedPaneWithoutDimensions_ReplacesCollapsedPreference() =>
        fixture.Session.Dispatch(
            () =>
            {
                using var model = new LayoutViewModel();
                var store = new JsonLayoutStore();
                using var manager = new LayoutManager<IViewModel>(model, store);
                var view = new Control { DataContext = model };
                var panel = new WorkspacePanel
                {
                    LeftWidth = GridLength.Star,
                    RightWidth = GridLength.Star,
                    BottomHeight = GridLength.Auto,
                };
                using var registration = view.RegisterWorkspaceLayout("workspace", panel);
                panel.IsLeftPaneOpen = false;

                panel.IsLeftPaneOpen = true;

                var snapshot = Assert.IsType<JObject>(store.Snapshot);
                Assert.Equal(2, store.SaveCount);
                Assert.True(snapshot.Value<bool>("IsLeftPaneOpen"));
                Assert.True(snapshot.Value<bool>("IsRightPaneOpen"));
                Assert.Null(snapshot.Value<double?>("LeftWidth"));
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public Task Save_CollapsedPane_KeepsLastPositiveWidthAndEmptySidePreference() =>
        fixture.Session.Dispatch(
            () =>
            {
                using var model = new LayoutViewModel();
                var store = new JsonLayoutStore();
                using var manager = new LayoutManager<IViewModel>(model, store);
                var view = new Control { DataContext = model };
                var panel = new WorkspacePanel
                {
                    LeftWidth = new GridLength(280),
                    RightWidth = new GridLength(320),
                };
                using var registration = view.RegisterWorkspaceLayout("workspace", panel);

                panel.IsLeftPaneOpen = false;

                var snapshot = Assert.IsType<JObject>(store.Snapshot);
                Assert.Equal(280, snapshot.Value<double>("LeftWidth"));
                Assert.Equal(320, snapshot.Value<double>("RightWidth"));
                Assert.False(snapshot.Value<bool>("IsLeftPaneOpen"));
                Assert.True(snapshot.Value<bool>("IsRightPaneOpen"));
                Assert.False(panel.HasRightPaneContent);
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public Task Load_RestoresFlagsAndDimensionsWithoutReplacingTwoWayBindings() =>
        fixture.Session.Dispatch(
            () =>
            {
                using var model = new LayoutViewModel();
                using var manager = new LayoutManager<IViewModel>(
                    model,
                    new JsonLayoutStore(
                        """{"LeftWidth":290,"RightWidth":330,"BottomHeight":110,"IsLeftPaneOpen":false,"IsRightPaneOpen":false}"""
                    )
                );
                var view = new Control { DataContext = model };
                var source = new PaneState();
                var panel = new WorkspacePanel();
                using var leftOpenBinding = panel.Bind(
                    WorkspacePanel.IsLeftPaneOpenProperty,
                    new Binding(nameof(PaneState.LeftOpen)) { Source = source, Mode = BindingMode.TwoWay }
                );
                using var rightOpenBinding = panel.Bind(
                    WorkspacePanel.IsRightPaneOpenProperty,
                    new Binding(nameof(PaneState.RightOpen)) { Source = source, Mode = BindingMode.TwoWay }
                );
                using var leftWidthBinding = panel.Bind(
                    WorkspacePanel.LeftWidthProperty,
                    new Binding(nameof(PaneState.LeftWidth)) { Source = source, Mode = BindingMode.TwoWay }
                );
                using var rightWidthBinding = panel.Bind(
                    WorkspacePanel.RightWidthProperty,
                    new Binding(nameof(PaneState.RightWidth)) { Source = source, Mode = BindingMode.TwoWay }
                );
                using var bottomHeightBinding = panel.Bind(
                    WorkspacePanel.BottomHeightProperty,
                    new Binding(nameof(PaneState.BottomHeight)) { Source = source, Mode = BindingMode.TwoWay }
                );

                using var registration = view.RegisterWorkspaceLayout("workspace", panel);

                Assert.False(source.LeftOpen);
                Assert.False(source.RightOpen);
                Assert.Equal(new GridLength(290), source.LeftWidth);
                Assert.Equal(new GridLength(330), source.RightWidth);
                Assert.Equal(new GridLength(110), source.BottomHeight);
                source.LeftOpen = true;
                source.RightOpen = true;
                source.LeftWidth = new GridLength(340);
                source.RightWidth = new GridLength(380);
                source.BottomHeight = new GridLength(150);
                Assert.True(panel.IsLeftPaneOpen);
                Assert.True(panel.IsRightPaneOpen);
                Assert.Equal(new GridLength(340), panel.LeftWidth);
                Assert.Equal(new GridLength(380), panel.RightWidth);
                Assert.Equal(new GridLength(150), panel.BottomHeight);
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public Task Load_ExplicitReload_DoesNotSavePartialSnapshots() =>
        fixture.Session.Dispatch(
            () =>
            {
                using var model = new LayoutViewModel();
                var store = new JsonLayoutStore();
                using var manager = new LayoutManager<IViewModel>(model, store);
                var view = new Control { DataContext = model };
                var panel = new WorkspacePanel();
                using var registration = view.RegisterWorkspaceLayout("workspace", panel);
                store.Snapshot = JObject.Parse(
                    """{"LeftWidth":290,"RightWidth":330,"BottomHeight":110,"IsLeftPaneOpen":false,"IsRightPaneOpen":false}"""
                );

                model.Layout.LoadAllAsync().GetAwaiter().GetResult();

                Assert.Equal(0, store.SaveCount);
                Assert.False(panel.IsLeftPaneOpen);
                Assert.False(panel.IsRightPaneOpen);
                Assert.Equal(new GridLength(290), panel.LeftWidth);
                Assert.Equal(new GridLength(330), panel.RightWidth);
                Assert.Equal(new GridLength(110), panel.BottomHeight);
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public Task Load_MeasuredPanel_RestoresNewWidthsWhenClosingPanes() =>
        fixture.Session.Dispatch(
            () =>
            {
                using var model = new LayoutViewModel();
                var store = new JsonLayoutStore();
                using var manager = new LayoutManager<IViewModel>(model, store);
                var view = new Control { DataContext = model };
                var panel = new WorkspacePanel
                {
                    LeftWidth = new GridLength(200),
                    RightWidth = new GridLength(240),
                };
                foreach (var dock in new[] { WorkspaceDock.Left, WorkspaceDock.Center, WorkspaceDock.Right })
                {
                    var widget = new WorkspaceItem();
                    WorkspacePanel.SetDock(widget, dock);
                    panel.Children.Add(widget);
                }

                Layout();
                using var registration = view.RegisterWorkspaceLayout("workspace", panel);
                store.Snapshot = JObject.Parse(
                    """{"LeftWidth":290,"RightWidth":330,"IsLeftPaneOpen":false,"IsRightPaneOpen":false}"""
                );

                model.Layout.LoadAllAsync().GetAwaiter().GetResult();
                Layout();

                Assert.Equal(0, store.SaveCount);
                Assert.False(panel.IsLeftPaneOpen);
                Assert.False(panel.IsRightPaneOpen);
                Assert.Equal(new GridLength(290), panel.LeftWidth);
                Assert.Equal(new GridLength(330), panel.RightWidth);

                panel.IsLeftPaneOpen = true;
                panel.IsRightPaneOpen = true;
                Layout();

                var grid = panel.GetVisualChildren().OfType<Grid>().Single();
                Assert.Equal(290, grid.ColumnDefinitions[0].ActualWidth);
                Assert.Equal(330, grid.ColumnDefinitions[4].ActualWidth);

                void Layout()
                {
                    panel.Measure(new Size(1000, 600));
                    panel.Arrange(new Rect(0, 0, 1000, 600));
                    panel.Measure(new Size(1000, 600));
                    panel.Arrange(new Rect(0, 0, 1000, 600));
                }
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public Task Load_InvalidDimensions_PreservesCurrentSizesAndRestoresFlags() =>
        fixture.Session.Dispatch(
            () =>
            {
                using var model = new LayoutViewModel();
                using var manager = new LayoutManager<IViewModel>(
                    model,
                    new JsonLayoutStore(
                        """{"LeftWidth":0,"RightWidth":-5,"BottomHeight":"NaN","IsLeftPaneOpen":false}"""
                    )
                );
                var view = new Control { DataContext = model };
                var panel = new WorkspacePanel
                {
                    LeftWidth = new GridLength(280),
                    RightWidth = new GridLength(320),
                    BottomHeight = new GridLength(120),
                };

                using var registration = view.RegisterWorkspaceLayout("workspace", panel);

                Assert.Equal(new GridLength(280), panel.LeftWidth);
                Assert.Equal(new GridLength(320), panel.RightWidth);
                Assert.Equal(new GridLength(120), panel.BottomHeight);
                Assert.False(panel.IsLeftPaneOpen);
                Assert.True(panel.IsRightPaneOpen);
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public Task DataContextChanged_RestoresAndSavesOnlyNewOwner() =>
        fixture.Session.Dispatch(
            () =>
            {
                using var first = new LayoutViewModel();
                using var second = new LayoutViewModel();
                var firstStore = new JsonLayoutStore("""{"IsLeftPaneOpen":false,"IsRightPaneOpen":false}""");
                var secondStore = new JsonLayoutStore("""{"IsLeftPaneOpen":true,"IsRightPaneOpen":true}""");
                using var firstManager = new LayoutManager<IViewModel>(first, firstStore);
                using var secondManager = new LayoutManager<IViewModel>(second, secondStore);
                var view = new Control { DataContext = first };
                var panel = new WorkspacePanel();
                using var registration = view.RegisterWorkspaceLayout("workspace", panel);
                Assert.False(panel.IsLeftPaneOpen);
                Assert.False(panel.IsRightPaneOpen);

                view.DataContext = second;
                Assert.True(panel.IsLeftPaneOpen);
                Assert.True(panel.IsRightPaneOpen);
                panel.IsRightPaneOpen = false;

                Assert.Equal(0, firstStore.SaveCount);
                Assert.Equal(1, secondStore.SaveCount);
                Assert.False(secondStore.Snapshot!.Value<bool>("IsRightPaneOpen"));
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public Task DataContextChanged_UnsavedOwner_KeepsNewTwoWaySourcePreference() =>
        fixture.Session.Dispatch(
            () =>
            {
                using var first = new LayoutViewModel();
                using var second = new LayoutViewModel();
                var firstStore = new JsonLayoutStore("""{"IsLeftPaneOpen":false,"IsRightPaneOpen":false}""");
                var secondStore = new JsonLayoutStore();
                using var firstManager = new LayoutManager<IViewModel>(first, firstStore);
                using var secondManager = new LayoutManager<IViewModel>(second, secondStore);
                var view = new Control { DataContext = first };
                var panel = new WorkspacePanel();
                using var leftBinding = panel.Bind(
                    WorkspacePanel.IsLeftPaneOpenProperty,
                    new Binding("DataContext.LeftOpen") { Source = view, Mode = BindingMode.TwoWay }
                );
                using var rightBinding = panel.Bind(
                    WorkspacePanel.IsRightPaneOpenProperty,
                    new Binding("DataContext.RightOpen") { Source = view, Mode = BindingMode.TwoWay }
                );
                using var registration = view.RegisterWorkspaceLayout("workspace", panel);
                Assert.False(first.LeftOpen);
                Assert.False(first.RightOpen);

                view.DataContext = second;

                Assert.True(panel.IsLeftPaneOpen);
                Assert.True(panel.IsRightPaneOpen);
                Assert.True(second.LeftOpen);
                Assert.True(second.RightOpen);
                Assert.False(first.LeftOpen);
                Assert.False(first.RightOpen);
                Assert.Equal(0, firstStore.SaveCount);
                Assert.Equal(0, secondStore.SaveCount);
                panel.IsLeftPaneOpen = false;
                Assert.False(second.LeftOpen);
                Assert.Equal(1, secondStore.SaveCount);
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public Task Dispose_UnregistersAndStopsSavingWorkspaceChanges() =>
        fixture.Session.Dispatch(
            () =>
            {
                using var model = new LayoutViewModel();
                var store = new JsonLayoutStore();
                using var manager = new LayoutManager<IViewModel>(model, store);
                var view = new Control { DataContext = model };
                var panel = new WorkspacePanel();
                var registration = view.RegisterWorkspaceLayout("workspace", panel);

                registration.Dispose();
                panel.IsLeftPaneOpen = false;
                model.Layout.LoadAllAsync().GetAwaiter().GetResult();

                Assert.Equal(0, store.SaveCount);
                using var replacement = view.RegisterWorkspaceLayout("workspace", panel);
            },
            TestContext.Current.CancellationToken
        );

    private sealed class LayoutViewModel() : ViewModel("workspace-layout-test")
    {
        private bool _leftOpen = true;
        private bool _rightOpen = true;

        public bool LeftOpen
        {
            get => _leftOpen;
            set => SetField(ref _leftOpen, value);
        }

        public bool RightOpen
        {
            get => _rightOpen;
            set => SetField(ref _rightOpen, value);
        }
    }

    private sealed class JsonLayoutStore(string? json = null) : ILayoutStore
    {
        public JObject? Snapshot { get; set; } = json is null ? null : JObject.Parse(json);
        public int SaveCount { get; private set; }

        public bool TryLoad<TData>(NavPath path, string layoutId, out TData layoutData)
        {
            layoutData = Snapshot is null ? default! : Snapshot.ToObject<TData>()!;
            return Snapshot is not null;
        }

        public void Save<TData>(NavPath path, string layoutId, TData layoutData)
        {
            Snapshot = JObject.FromObject(layoutData!);
            SaveCount++;
        }

        public void Flush() { }
        public void Dispose() { }
    }

    private sealed class PaneState : AvaloniaObject
    {
        public static readonly StyledProperty<bool> LeftOpenProperty =
            AvaloniaProperty.Register<PaneState, bool>(nameof(LeftOpen), true);
        public static readonly StyledProperty<bool> RightOpenProperty =
            AvaloniaProperty.Register<PaneState, bool>(nameof(RightOpen), true);
        public static readonly StyledProperty<GridLength> LeftWidthProperty =
            AvaloniaProperty.Register<PaneState, GridLength>(nameof(LeftWidth), new GridLength(250));
        public static readonly StyledProperty<GridLength> RightWidthProperty =
            AvaloniaProperty.Register<PaneState, GridLength>(nameof(RightWidth), new GridLength(250));
        public static readonly StyledProperty<GridLength> BottomHeightProperty =
            AvaloniaProperty.Register<PaneState, GridLength>(nameof(BottomHeight), GridLength.Auto);

        public bool LeftOpen
        {
            get => GetValue(LeftOpenProperty);
            set => SetValue(LeftOpenProperty, value);
        }

        public bool RightOpen
        {
            get => GetValue(RightOpenProperty);
            set => SetValue(RightOpenProperty, value);
        }

        public GridLength LeftWidth
        {
            get => GetValue(LeftWidthProperty);
            set => SetValue(LeftWidthProperty, value);
        }

        public GridLength RightWidth
        {
            get => GetValue(RightWidthProperty);
            set => SetValue(RightWidthProperty, value);
        }

        public GridLength BottomHeight
        {
            get => GetValue(BottomHeightProperty);
            set => SetValue(BottomHeightProperty, value);
        }
    }
}
