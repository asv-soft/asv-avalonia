using Asv.Common;
using Asv.Modeling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using R3;

namespace Asv.Avalonia;

public static class ViewLayoutMixin
{
    extension(ILayoutController layout)
    {
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

        private static bool IsValidPixelWidth(double value)
        {
            return value is > 0 && double.IsFinite(value);
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
