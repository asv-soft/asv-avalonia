using Asv.Avalonia.GeoMap;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public sealed class MapPreloadDialogTest(WorkspaceUiTestFixture fixture)
{
    [Fact]
    public Task Defaults_ExcludeEmptyProviderAndPreserveGlobalMapSettings() =>
        Run(async () =>
        {
            using var map = new DialogMapService();
            using var viewModel = new MapPreloadDialogViewModel(
                map,
                new DialogPreloadService(),
                new GeoPoint(55, 60, 100)
            );
            Assert.Single(viewModel.Providers);
            Assert.True(viewModel.Providers[0].IsSelected.Value);
            Assert.Same(viewModel, viewModel.Providers[0].Parent);
            Assert.Equal(viewModel.Providers, viewModel.GetChildren());
            Assert.Equal(5, viewModel.RadiusKm.Value);
            Assert.Equal(10, viewModel.MinZoom.Value);
            Assert.Equal(15, viewModel.MaxZoom.Value);
            Assert.Equal(73, viewModel.RadiusPolygon.Value.Count);
            var changedCenter = new GeoPoint(54, 59, 200);
            viewModel.SelectCenter(changedCenter);
            viewModel.PreviewZoom.Value = 14;
            Assert.Equal(changedCenter, viewModel.Center.Value);
            Assert.Equal(2, map.MinZoom.Value);
            Assert.Equal(18, map.MaxZoom.Value);
            Assert.Same(map.Provider, map.CurrentProvider.Value);
            await viewModel.CancelAndWaitAsync();
        });

    [Fact]
    public Task InvalidSelection_DisablesDownloadAndDoesNotUsePreviousEstimate() =>
        Run(async () =>
        {
            using var map = new DialogMapService();
            var preload = new DialogPreloadService();
            using var viewModel = new MapPreloadDialogViewModel(map, preload, GeoPoint.Zero);
            await WaitUntilAsync(() => viewModel.CanDownload.Value);
            Assert.Equal(4, viewModel.EstimatedTiles.Value);
            viewModel.MinZoom.Value = 16;
            Assert.False(viewModel.CanDownload.Value);
            Assert.False(viewModel.IsEstimating.Value);
            await viewModel.DownloadAsync();
            Assert.Equal(0, preload.DownloadCalls);
            viewModel.MinZoom.Value = 10;
            viewModel.Providers[0].IsSelected.Value = false;
            Assert.False(viewModel.CanDownload.Value);
            await viewModel.CancelAndWaitAsync();
        });

    [Fact]
    public Task Closing_CancelsAndWaitsForPersistenceBeforeDisposal_AndIgnoresLateProgress() =>
        Run(async () =>
        {
            using var map = new DialogMapService();
            var preload = new DialogPreloadService { WaitForPersistence = true };
            var viewModel = new MapPreloadDialogViewModel(map, preload, GeoPoint.Zero);
            try
            {
                await WaitUntilAsync(() => viewModel.CanDownload.Value);
                var download = viewModel.DownloadAsync();
                await preload.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.True(viewModel.IsRunning.Value);
                Assert.False(viewModel.ParametersEnabled.Value);
                viewModel.SelectCenter(new GeoPoint(10, 20, 0));
                Assert.Equal(GeoPoint.Zero, viewModel.Center.Value);
                Assert.Equal(5, preload.Request!.RadiusKm);

                var closing = viewModel.CancelAndWaitAsync();
                await preload.CancellationObserved.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.False(closing.IsCompleted);
                Assert.False(download.IsCompleted);
                preload.PersistenceCompleted.TrySetResult();
                await closing.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.True(download.IsCompletedSuccessfully);
                Assert.Equal(1, viewModel.LastResult!.DownloadedTiles);
                Assert.Equal(1000, viewModel.LastResult.DownloadedBytes);

                var notifications = 0;
                using var subscription = viewModel
                    .ProgressPercent.Skip(1)
                    .Subscribe(_ => notifications++);
                viewModel.Dispose();
                preload.Progress!.Report(new MapPreloadProgress(4, 4, 0, 4, 0, 4000));
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(0, notifications);
            }
            finally
            {
                preload.PersistenceCompleted.TrySetResult();
                await viewModel.CancelAndWaitAsync();
                viewModel.Dispose();
            }
        });

    [Fact]
    public Task DownloadFailure_IsVisibleAndAllowsAnotherRun() =>
        Run(async () =>
        {
            using var map = new DialogMapService();
            var preload = new DialogPreloadService { Failure = new IOException("Offline") };
            using var viewModel = new MapPreloadDialogViewModel(map, preload, GeoPoint.Zero);
            await WaitUntilAsync(() => viewModel.CanDownload.Value);
            await viewModel.DownloadAsync();
            Assert.Equal("Offline", viewModel.ErrorText.Value);
            Assert.False(viewModel.IsRunning.Value);
            Assert.True(viewModel.ParametersEnabled.Value);
            Assert.True(viewModel.CanDownload.Value);
            preload.Failure = null;
            await viewModel.DownloadAsync();
            Assert.Null(viewModel.ErrorText.Value);
            Assert.Equal(2, preload.DownloadCalls);
            Assert.Equal(100, viewModel.ProgressPercent.Value);
            await viewModel.CancelAndWaitAsync();
        });

    [Theory]
    [InlineData("HeaderCloseButton")]
    [InlineData("CloseButton")]
    [InlineData("Escape")]
    public Task Prefab_ExpandedViewSupportsPointerSelectionAndWaitsForWorkOnEveryCloseRoute(
        string closeRoute
    ) =>
        Run(async () =>
        {
            using var scene = new DialogScene();
            var showing = new MapPreloadDialogPrefab(
                scene.MapService,
                scene.PreloadService
            ).ShowDialogAsync(
                new MapPreloadDialogPayload { InitialCenter = GeoPoint.Zero },
                scene.Window
            );
            ContentDialog? dialog = null;
            try
            {
                await WaitUntilAsync(() => scene.View is not null);
                scene.Window.UpdateLayout();
                dialog = Assert.Single(scene.Window.GetVisualDescendants().OfType<ContentDialog>());
                Assert.Equal(DialogSizeMode.Expanded, dialog.SizeMode);
                var contentScroller = Assert.Single(
                    dialog.GetVisualDescendants().OfType<ScrollViewer>(),
                    scroll => scroll.Name == "ContentScrollViewer"
                );
                Assert.Equal(
                    global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                    contentScroller.VerticalScrollBarVisibility
                );
                var view = Assert.IsType<MapPreloadDialogView>(scene.View);
                var map = view.FindControl<MapItemsControl>("AreaMap")!;
                var viewModel = Assert.IsType<MapPreloadDialogViewModel>(view.DataContext);
                var marker = Assert.IsType<MapItem>(map.Items[0]);
                Assert.InRange(map.Bounds.Width, 400, 1000);
                Assert.InRange(map.Bounds.Height, 250, 650);

                var clickPoint = map.TranslatePoint(new Point(90, 70), scene.Window)!.Value;
                scene.Window.MouseDown(clickPoint, MouseButton.Left);
                scene.Window.MouseUp(clickPoint, MouseButton.Left);
                Assert.NotEqual(GeoPoint.Zero, viewModel.Center.Value);
                Assert.Equal(viewModel.Center.Value, marker.Location);
                var clickedCenter = viewModel.Center.Value;
                scene.Window.UpdateLayout();
                var markerPoint = marker
                    .TranslatePoint(
                        new Point(marker.Bounds.Width / 2, marker.Bounds.Height / 2),
                        scene.Window
                    )!
                    .Value;
                scene.Window.MouseDown(markerPoint, MouseButton.Left);
                scene.Window.MouseMove(
                    markerPoint + new Vector(40, 20),
                    RawInputModifiers.LeftMouseButton
                );
                scene.Window.MouseUp(markerPoint + new Vector(40, 20), MouseButton.Left);
                Assert.NotEqual(clickedCenter, viewModel.Center.Value);
                Assert.Equal(viewModel.Center.Value, marker.Location);
                var oldPolygon = marker.Polygon;
                viewModel.RadiusKm.Value = 7;
                Assert.NotSame(oldPolygon, marker.Polygon);
                Assert.Same(viewModel.RadiusPolygon.Value, marker.Polygon);
                Assert.Equal(73, marker.Polygon!.Count);

                await WaitUntilAsync(() => viewModel.CanDownload.Value);
                _ = viewModel.DownloadAsync();
                await scene.PreloadService.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.False(map.IsEnabled);
                if (closeRoute == "Escape")
                {
                    dialog
                        .GetVisualDescendants()
                        .OfType<Button>()
                        .Single(button => button.Name == "CloseButton")
                        .Focus();
                    scene.Window.KeyPress(
                        Key.Escape,
                        RawInputModifiers.None,
                        PhysicalKey.Escape,
                        null
                    );
                    scene.Window.KeyRelease(
                        Key.Escape,
                        RawInputModifiers.None,
                        PhysicalKey.Escape,
                        null
                    );
                }
                else
                {
                    dialog
                        .GetVisualDescendants()
                        .OfType<Button>()
                        .Single(button => button.Name == closeRoute)
                        .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }

                await scene.PreloadService.CancellationObserved.Task.WaitAsync(
                    TimeSpan.FromSeconds(5)
                );
                Assert.False(showing.IsCompleted);
                Assert.False(viewModel.IsDisposed);
                scene.PreloadService.PersistenceCompleted.TrySetResult();
                var result = await showing.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.Equal(1, result!.DownloadedTiles);
                Assert.True(viewModel.IsDisposed);
            }
            finally
            {
                scene.PreloadService.PersistenceCompleted.TrySetResult();
                if (!showing.IsCompleted)
                {
                    dialog?.Hide();
                }

                await showing.WaitAsync(TimeSpan.FromSeconds(5));
            }
        });

    private Task Run(Func<Task> action) =>
        fixture.Session.Dispatch(
            async () =>
            {
                await action();
                return true;
            },
            TestContext.Current.CancellationToken
        );

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!condition())
        {
            await Task.Delay(10, timeout.Token);
        }
    }

    private sealed class DialogPreloadService : IMapPreloadService
    {
        public TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource CancellationObserved { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource PersistenceCompleted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool WaitForPersistence { get; init; }
        public Exception? Failure { get; set; }
        public int DownloadCalls { get; private set; }
        public MapPreloadRequest? Request { get; private set; }
        public IProgress<MapPreloadProgress>? Progress { get; private set; }

        public long CountTiles(
            MapPreloadRequest request,
            CancellationToken cancellationToken = default
        ) => 4;

        public async Task<MapPreloadProgress> DownloadAsync(
            MapPreloadRequest request,
            IProgress<MapPreloadProgress>? progress = null,
            CancellationToken cancellationToken = default
        )
        {
            DownloadCalls++;
            Request = request;
            Progress = progress;
            using var registration = cancellationToken.Register(() =>
                CancellationObserved.TrySetResult()
            );
            Started.TrySetResult();
            if (WaitForPersistence)
            {
                await PersistenceCompleted.Task;
                progress?.Report(new MapPreloadProgress(4, 1, 0, 1, 0, 1000));
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (Failure is not null)
            {
                throw Failure;
            }

            return new MapPreloadProgress(4, 4, 0, 4, 0, 4000);
        }
    }

    private sealed class DialogMapService : IMapService, IDisposable
    {
        public ITileProvider Provider { get; } = new PreloadTestProvider("dialog");
        public SynchronizedReactiveProperty<MapModeType> Mode { get; } = new(MapModeType.Mixed);
        public SynchronizedReactiveProperty<IBrush> EmptyTileBrush { get; } =
            new(Brushes.Transparent);
        public SynchronizedReactiveProperty<int> MinZoom { get; } = new(2);
        public SynchronizedReactiveProperty<int> MaxZoom { get; } = new(18);
        public SynchronizedReactiveProperty<ITileProvider> CurrentProvider { get; }
        public IReadOnlyList<ITileProvider> AvailableProviders { get; }

        public DialogMapService()
        {
            CurrentProvider = new(Provider);
            AvailableProviders = [EmptyTileProvider.Instance, Provider];
        }

        public void SetProviderApiKey(string providerId, string? apiKey) { }

        public string? GetProviderApiKey(string providerId) => null;

        public void Dispose()
        {
            Mode.Dispose();
            EmptyTileBrush.Dispose();
            MinZoom.Dispose();
            MaxZoom.Dispose();
            CurrentProvider.Dispose();
        }
    }

    private sealed class DialogScene : IDisposable
    {
        private readonly StyleInclude[] _themes;
        private readonly FuncDataTemplate<MapPreloadDialogViewModel> _template;

        public DialogScene()
        {
            _themes =
            [
                new StyleInclude(new Uri("avares://Asv.Avalonia/"))
                {
                    Source = new Uri("avares://Asv.Avalonia/Theme.axaml"),
                },
                new StyleInclude(new Uri("avares://Asv.Avalonia.GeoMap/"))
                {
                    Source = new Uri("avares://Asv.Avalonia.GeoMap/Theme.axaml"),
                },
            ];
            foreach (var theme in _themes)
            {
                Application.Current!.Styles.Add(theme);
            }

            _template = new FuncDataTemplate<MapPreloadDialogViewModel>(
                (viewModel, _) =>
                {
                    View = new MapPreloadDialogView { DataContext = viewModel };
                    var map = View.FindControl<MapItemsControl>("AreaMap")!;
                    // Keep the production view, marker, and coordinate canvas without requiring AppHost's tile loader.
                    map.ItemsPanel = new FuncTemplate<Panel?>(() =>
                    {
                        var canvas = new MapCanvas();
                        canvas.Bind(
                            MapCanvas.CenterMapProperty,
                            new Binding(nameof(map.CenterMap)) { Source = map }
                        );
                        canvas.Bind(
                            MapCanvas.ProviderProperty,
                            new Binding(nameof(map.Provider)) { Source = map }
                        );
                        canvas.Bind(
                            MapCanvas.ZoomProperty,
                            new Binding(nameof(map.Zoom)) { Source = map }
                        );
                        return canvas;
                    });
                    map.Template = new FuncControlTemplate<MapItemsControl>(
                        (_, scope) =>
                        {
                            var presenter = new ItemsPresenter
                            {
                                Name = "PART_ItemsPresenter",
                                ItemsPanel = map.ItemsPanel,
                            };
                            scope.Register(presenter.Name, presenter);
                            return new Border
                            {
                                Background = Brushes.Transparent,
                                Child = presenter,
                            };
                        }
                    );
                    return View;
                }
            );
            Application.Current!.DataTemplates.Insert(0, _template);
            var mainView = new Border();
            Window = new Window
            {
                Width = 1200,
                Height = 800,
                Content = mainView,
            };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        public DialogMapService MapService { get; } = new();
        public DialogPreloadService PreloadService { get; } = new() { WaitForPersistence = true };
        public Window Window { get; }
        public MapPreloadDialogView? View { get; private set; }

        public void Dispose()
        {
            Window.Close();
            Application.Current!.DataTemplates.Remove(_template);
            foreach (var theme in _themes)
            {
                Application.Current.Styles.Remove(theme);
            }

            MapService.Dispose();
        }
    }
}
