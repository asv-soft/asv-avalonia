using Asv.Modeling;
using Avalonia;
using Avalonia.Controls;
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
            return view.RegisterLayout(
                layoutId,
                data => load(data.Value),
                () => save() is { } value ? new LayoutScalar<TValue> { Value = value } : null,
                trigger
            );
        }

        public IDisposable RegisterGridColumnPixelWidth(string layoutId, Grid grid, int columnIndex)
        {
            ArgumentNullException.ThrowIfNull(grid);

            if (columnIndex < 0 || columnIndex >= grid.ColumnDefinitions.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            var column = grid.ColumnDefinitions[columnIndex];
            return view.RegisterLayoutValue<double, GridLength>(
                layoutId,
                width => column.Width = new GridLength(width, GridUnitType.Pixel),
                () =>
                {
                    var width = column.Width;
                    var isValid =
                        width is { GridUnitType: GridUnitType.Pixel, Value: > 0 }
                        && double.IsFinite(width.Value);

                    return isValid ? width.Value : null;
                },
                column.GetObservable(ColumnDefinition.WidthProperty).ToObservable()
            );
        }
    }
}

public sealed class LayoutScalar<TValue>
    where TValue : struct
{
    public TValue Value { get; set; }
}
