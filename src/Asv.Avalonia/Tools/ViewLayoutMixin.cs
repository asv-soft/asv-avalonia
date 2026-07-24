using Asv.Common;
using Asv.Modeling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Provides layout persistence helpers for layout controllers and Avalonia controls.
/// </summary>
public static class ViewLayoutMixin
{
    extension(ILayoutController layout)
    {
        /// <summary>
        /// Registers a reference-type layout snapshot and saves it whenever the trigger emits.
        /// </summary>
        /// <typeparam name="TData">The reference-type snapshot to persist.</typeparam>
        /// <typeparam name="TTrigger">The type of value emitted by the save trigger.</typeparam>
        /// <param name="layoutId">The non-empty layout identifier within the controller.</param>
        /// <param name="load">The action that applies a loaded snapshot.</param>
        /// <param name="save">
        /// The function that captures the current snapshot, or returns <see langword="null"/> to skip
        /// saving.
        /// </param>
        /// <param name="trigger">The observable whose emissions request a save.</param>
        /// <returns>
        /// A disposable that owns the layout registration and the trigger subscription.
        /// </returns>
        /// <remarks>
        /// This method does not load the registered value. Call
        /// <see cref="ILayoutController.LoadAllAsync"/> after completing registration. Trigger
        /// emissions are dropped while a previous save is still running.
        /// </remarks>
        public IDisposable Register<TData, TTrigger>(
            string layoutId,
            Action<TData> load,
            Func<TData?> save,
            Observable<TTrigger> trigger
        )
            where TData : class, new()
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentException.ThrowIfNullOrWhiteSpace(layoutId);
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(save);
            ArgumentNullException.ThrowIfNull(trigger);

            var sink = layout.Register<TData>(
                layoutId,
                (data, _) =>
                {
                    load(data);
                    return ValueTask.CompletedTask;
                }
            );
            var sub = trigger.SubscribeAwait(
                (_, cancel) =>
                {
                    return save() is { } data
                        ? sink.SaveAsync(data, cancel)
                        : ValueTask.CompletedTask;
                },
                AwaitOperation.Drop
            );
            return Disposable.Combine(sink, sub);
        }

        /// <summary>
        /// Registers a value-type layout snapshot and saves it whenever the trigger emits.
        /// </summary>
        /// <typeparam name="TValue">The value-type snapshot to persist.</typeparam>
        /// <typeparam name="TTrigger">The type of value emitted by the save trigger.</typeparam>
        /// <param name="layoutId">The non-empty layout identifier within the controller.</param>
        /// <param name="load">The action that applies a loaded value.</param>
        /// <param name="save">
        /// The function that captures the current value, or returns <see langword="null"/> to skip
        /// saving.
        /// </param>
        /// <param name="trigger">The observable whose emissions request a save.</param>
        /// <returns>
        /// A disposable that owns the layout registration and the trigger subscription.
        /// </returns>
        /// <remarks>
        /// This method does not load the registered value. Call
        /// <see cref="ILayoutController.LoadAllAsync"/> after completing registration. Trigger
        /// emissions are dropped while a previous save is still running.
        /// </remarks>
        public IDisposable Register<TValue, TTrigger>(
            string layoutId,
            Action<TValue> load,
            Func<TValue?> save,
            Observable<TTrigger> trigger
        )
            where TValue : struct
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentException.ThrowIfNullOrWhiteSpace(layoutId);
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(save);
            ArgumentNullException.ThrowIfNull(trigger);

            var sink = layout.Register<TValue>(
                layoutId,
                (data, _) =>
                {
                    load(data);
                    return ValueTask.CompletedTask;
                }
            );
            var sub = trigger.SubscribeAwait(
                (_, cancel) =>
                {
                    return save() is { } data
                        ? sink.SaveAsync(data, cancel)
                        : ValueTask.CompletedTask;
                },
                AwaitOperation.Drop
            );
            return Disposable.Combine(sink, sub);
        }
    }

    extension(Control view)
    {
        /// <summary>
        /// Persists a reference-type layout snapshot against the current view model data context.
        /// </summary>
        /// <typeparam name="TData">The reference-type snapshot to persist.</typeparam>
        /// <typeparam name="TTrigger">The type of value emitted by the save trigger.</typeparam>
        /// <param name="layoutId">The non-empty layout identifier within each data context.</param>
        /// <param name="load">The action that applies a loaded snapshot on the UI thread.</param>
        /// <param name="save">
        /// The function that captures a snapshot on the UI thread, or returns
        /// <see langword="null"/> to skip saving.
        /// </param>
        /// <param name="trigger">The observable whose emissions request a save.</param>
        /// <returns>
        /// A disposable that owns data context tracking, the current layout registration, and its
        /// trigger subscription.
        /// </returns>
        /// <remarks>
        /// The helper registers whenever the data context implements <see cref="IViewModel"/>, loads
        /// the value immediately, and re-registers after data context changes. Trigger emissions are
        /// dropped while a previous save is still running.
        /// </remarks>
        public IDisposable RegisterLayout<TData, TTrigger>(
            string layoutId,
            Action<TData> load,
            Func<TData?> save,
            Observable<TTrigger> trigger
        )
            where TData : class, new()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(layoutId);
            ArgumentNullException.ThrowIfNull(view);
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(save);
            ArgumentNullException.ThrowIfNull(trigger);

            var registration = new SerialDisposable();

            var dataContextSubscription = view.GetObservable(StyledElement.DataContextProperty)
                .ToObservable()
                .Subscribe(_ => ReRegister());

            return Disposable.Combine(registration, dataContextSubscription);

            void ReRegister()
            {
                registration.Disposable = Disposable.Empty;
                registration.Disposable = Register();
            }

            IDisposable Register()
            {
                if (view.DataContext is not IViewModel viewModel)
                {
                    return Disposable.Empty;
                }

                var sink = viewModel.Layout.Register<TData>(
                    layoutId,
                    (data, cancel) => InvokeOnUiThreadAsync(() => load(data), cancel)
                );
                sink.LoadAsync(CancellationToken.None).AsTask().SafeFireAndForget();
                var sub = trigger.SubscribeAwait(
                    async (_, cancel) =>
                    {
                        var data = await InvokeOnUiThreadAsync(save, cancel);
                        if (data is not null)
                        {
                            await sink.SaveAsync(data, cancel);
                        }
                    },
                    AwaitOperation.Drop
                );
                return Disposable.Combine(sink, sub);
            }
        }

        /// <summary>
        /// Persists a value-type layout snapshot against the current view model data context.
        /// </summary>
        /// <typeparam name="TValue">The value-type snapshot to persist.</typeparam>
        /// <typeparam name="TTrigger">The type of value emitted by the save trigger.</typeparam>
        /// <param name="layoutId">The non-empty layout identifier within each data context.</param>
        /// <param name="load">The action that applies a loaded value on the UI thread.</param>
        /// <param name="save">
        /// The function that captures a value on the UI thread, or returns
        /// <see langword="null"/> to skip saving.
        /// </param>
        /// <param name="trigger">The observable whose emissions request a save.</param>
        /// <returns>
        /// A disposable that owns data context tracking, the current layout registration, and its
        /// trigger subscription.
        /// </returns>
        /// <remarks>
        /// The helper registers whenever the data context implements <see cref="IViewModel"/>, loads
        /// the value immediately, and re-registers after data context changes. Trigger emissions are
        /// dropped while a previous save is still running.
        /// </remarks>
        public IDisposable RegisterLayoutValue<TValue, TTrigger>(
            string layoutId,
            Action<TValue> load,
            Func<TValue?> save,
            Observable<TTrigger> trigger
        )
            where TValue : struct
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(layoutId);
            ArgumentNullException.ThrowIfNull(view);
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(save);
            ArgumentNullException.ThrowIfNull(trigger);

            var registration = new SerialDisposable();

            var dataContextSubscription = view.GetObservable(StyledElement.DataContextProperty)
                .ToObservable()
                .Subscribe(_ => ReRegister());

            return Disposable.Combine(registration, dataContextSubscription);

            void ReRegister()
            {
                registration.Disposable = Disposable.Empty;
                registration.Disposable = Register();
            }

            IDisposable Register()
            {
                if (view.DataContext is not IViewModel viewModel)
                {
                    return Disposable.Empty;
                }

                var sink = viewModel.Layout.Register<TValue>(
                    layoutId,
                    (data, cancel) => InvokeOnUiThreadAsync(() => load(data), cancel)
                );
                sink.LoadAsync(CancellationToken.None).AsTask().SafeFireAndForget();
                var sub = trigger.SubscribeAwait(
                    async (_, cancel) =>
                    {
                        var data = await InvokeOnUiThreadAsync(save, cancel);
                        if (data is { } value)
                        {
                            await sink.SaveAsync(value, cancel);
                        }
                    },
                    AwaitOperation.Drop
                );
                return Disposable.Combine(sink, sub);
            }
        }

        /// <summary>
        /// Persists one grid column's finite, positive pixel width.
        /// </summary>
        /// <param name="layoutId">The layout identifier associated with the column.</param>
        /// <param name="grid">The grid that owns the column and relevant splitters.</param>
        /// <param name="columnIndex">The zero-based column definition index.</param>
        /// <returns>
        /// A disposable that owns the layout registration, width observation, and splitter event
        /// handlers.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="columnIndex"/> does not identify a column in <paramref name="grid"/>.
        /// </exception>
        public IDisposable RegisterGridColumnPixelWidth(string layoutId, Grid grid, int columnIndex)
        {
            ArgumentNullException.ThrowIfNull(grid);

            if (columnIndex < 0 || columnIndex >= grid.ColumnDefinitions.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            var column = grid.ColumnDefinitions[columnIndex];
            var widthChanged = column
                .GetObservable(ColumnDefinition.WidthProperty)
                .ToObservable()
                .Skip(1)
                .Select(_ => Unit.Default);

            var splitterDragCompleted = new Subject<Unit>();
            var splitterSubscriptions = new CompositeDisposable();
            foreach (var splitter in grid.Children.OfType<GridSplitter>())
            {
                splitter.DragCompleted += OnSplitterDragCompleted;
                splitterSubscriptions.Add(
                    Disposable.Create(() => splitter.DragCompleted -= OnSplitterDragCompleted)
                );
            }

            var layout = view.RegisterLayoutValue<double, Unit>(
                layoutId,
                width => column.Width = new GridLength(width, GridUnitType.Pixel),
                () =>
                {
                    var width = column.ActualWidth;
                    if (!IsValidPixelWidth(width))
                    {
                        width = column.Width.Value;
                    }

                    return IsValidPixelWidth(width) ? width : null;
                },
                widthChanged.Merge(splitterDragCompleted)
            );
            return Disposable.Combine(layout, splitterSubscriptions, splitterDragCompleted);

            void OnSplitterDragCompleted(object? sender, VectorEventArgs e)
            {
                splitterDragCompleted.OnNext(Unit.Default);
            }
        }

        /// <summary>
        /// Persists the generated panel sizes of a workspace.
        /// </summary>
        /// <param name="layoutId">The layout identifier associated with the workspace.</param>
        /// <param name="workspace">The workspace whose generated panel should be tracked.</param>
        /// <returns>
        /// A disposable that owns the pending loaded handler or the active panel registration.
        /// </returns>
        /// <remarks>
        /// Registration occurs immediately when the workspace panel is available; otherwise the
        /// helper waits until the workspace is loaded.
        /// </remarks>
        public IDisposable RegisterWorkspaceLayout(string layoutId, Workspace workspace)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(layoutId);
            ArgumentNullException.ThrowIfNull(workspace);

            var registration = new SerialDisposable();

            workspace.Loaded += OnLoaded;
            TryRegister();

            return Disposable.Combine(
                registration,
                Disposable.Create(() => workspace.Loaded -= OnLoaded)
            );

            void OnLoaded(object? sender, RoutedEventArgs e) => TryRegister();

            void TryRegister()
            {
                if (workspace.ItemsPanelRoot is WorkspacePanel panel)
                {
                    workspace.Loaded -= OnLoaded;
                    registration.Disposable = view.RegisterWorkspaceLayout(layoutId, panel);
                }
            }
        }

        /// <summary>
        /// Persists the left, right, and bottom dimensions of a workspace panel.
        /// </summary>
        /// <param name="layoutId">The layout identifier associated with the panel.</param>
        /// <param name="panel">The workspace panel to track.</param>
        /// <returns>
        /// A disposable that owns the layout registration, workspace event handler, and internal
        /// change observable.
        /// </returns>
        /// <remarks>
        /// Restored values are limited to finite, positive pixel sizes and clamped to the panel's
        /// minimum dimensions. No snapshot is saved when all three dimensions are unavailable.
        /// </remarks>
        public IDisposable RegisterWorkspaceLayout(string layoutId, WorkspacePanel panel)
        {
            ArgumentNullException.ThrowIfNull(panel);

            var changed = new Subject<Unit>();

            panel.WorkspaceChanged += OnWorkspaceChanged;
            var unsubscribe = Disposable.Create(() => panel.WorkspaceChanged -= OnWorkspaceChanged);

            var layout = view.RegisterLayout(
                layoutId,
                config =>
                {
                    ApplyPixel(config.LeftWidth, panel.MinLeftWidth, x => panel.LeftWidth = x);
                    ApplyPixel(config.RightWidth, panel.MinRightWidth, x => panel.RightWidth = x);
                    ApplyPixel(
                        config.BottomHeight,
                        panel.MinBottomHeight,
                        x => panel.BottomHeight = x
                    );
                },
                () =>
                {
                    var config = new WorkspacePanelConfig
                    {
                        LeftWidth = panel.LeftColumnPixelWidth,
                        RightWidth = panel.RightColumnPixelWidth,
                        BottomHeight = panel.BottomRowPixelHeight,
                    };

                    return
                        config.LeftWidth is null
                        && config.RightWidth is null
                        && config.BottomHeight is null
                        ? null
                        : config;
                },
                changed
            );

            return Disposable.Combine(layout, unsubscribe, changed);

            void OnWorkspaceChanged(object? sender, WorkspaceEventArgs e)
            {
                changed.OnNext(Unit.Default);
            }
        }

        private static void ApplyPixel(double? stored, double min, Action<GridLength> apply)
        {
            if (stored is not { } value || !IsValidPixelWidth(value))
            {
                return;
            }

            if (double.IsFinite(min) && min > 0 && value < min)
            {
                value = min;
            }

            apply(new GridLength(value, GridUnitType.Pixel));
        }

        private static bool IsValidPixelWidth(double value)
        {
            return value > 0 && double.IsFinite(value);
        }

        private static async ValueTask InvokeOnUiThreadAsync(
            Action action,
            CancellationToken cancel
        )
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                action();
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Normal, cancel);
        }

        private static async ValueTask<TValue?> InvokeOnUiThreadAsync<TValue>(
            Func<TValue?> action,
            CancellationToken cancel
        )
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                return action();
            }

            return await Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Normal, cancel);
        }
    }
}
