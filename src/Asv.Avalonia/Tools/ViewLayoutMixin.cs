using Asv.Modeling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public static class ViewLayoutMixin
{
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
            registration.Disposable = Register();

            var dataContextSubscription = view.GetObservable(StyledElement.DataContextProperty)
                .ToObservable()
                .Subscribe(_ => registration.Disposable = Register());

            return Disposable.Combine(registration, dataContextSubscription);

            IDisposable Register()
            {
                if (view.DataContext is not IViewModel viewModel)
                {
                    return Disposable.Empty;
                }

                var sink = viewModel.Layout.Register(layoutId, load);
                sink.Load();
                var sub = trigger.Subscribe(_ =>
                {
                    if (save() is { } data)
                    {
                        sink.Save(data);
                    }
                });
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
            registration.Disposable = Register();

            var dataContextSubscription = view.GetObservable(StyledElement.DataContextProperty)
                .ToObservable()
                .Subscribe(_ => registration.Disposable = Register());

            return Disposable.Combine(registration, dataContextSubscription);

            IDisposable Register()
            {
                if (view.DataContext is not IViewModel viewModel)
                {
                    return Disposable.Empty;
                }

                var sink = viewModel.Layout.Register(layoutId, load);
                sink.Load();
                var sub = trigger.Subscribe(_ =>
                {
                    if (save() is { } data)
                    {
                        sink.Save(data);
                    }
                });
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
    }
}
