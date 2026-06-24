using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class PropertySliderDesign : PropertySliderViewModel
{
    public PropertySliderDesign()
        : base(NavId.GenerateRandomAsString(), 0, 100)
    {
        DesignTime.ThrowIfNotDesignMode();
        ShortHeader = "Thr";
        Header = "Throttle";
        Description = "Slider property with numeric value, units, busy state, and error state.";
        Icon = MaterialIconKind.Signal;
        IconColor = AsvColorKind.Success;
        TickFrequency = 5;
        SmallChange = 1;
        LargeChange = 10;
        IsSnapToTickEnabled = true;
        Units = "%";
        ValueFormat = "0";
        ApplyValueFromModel(65);

        Menu.Add(
            new MenuItem("copy_value", "Copy value")
            {
                Icon = MaterialIconKind.ContentCopy,
                Command = DesignTime.EmptyCommand,
            }
        );
        Menu.Add(
            new MenuItem("reset_value", "Reset value")
            {
                Icon = MaterialIconKind.Restore,
                Command = DesignTime.EmptyCommand,
            }
        );
        Menu.Add(
            new MenuItem("more_actions", "More actions") { Icon = MaterialIconKind.DotsHorizontal }
        );
        Menu.Add(
            new MenuItem("validate_value", "Validate", "more_actions")
            {
                Icon = MaterialIconKind.CheckCircle,
                Command = DesignTime.EmptyCommand,
            }
        );

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                MarkUpdated();
            })
            .AddTo(ref DisposableBag);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                IsBusy = !IsBusy;
            })
            .AddTo(ref DisposableBag);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                var nextValue = Value.Value >= Maximum ? Minimum : Value.Value + TickFrequency;
                ApplyValueFromModel(nextValue);
            })
            .AddTo(ref DisposableBag);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                ErrorMessage = ErrorMessage == null ? "Slider sync error" : null;
            })
            .AddTo(ref DisposableBag);
    }

    protected override ValueTask ApplyFromUser(double value, CancellationToken cancel)
    {
        return ValueTask.CompletedTask;
    }
}
