using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class PropertyToggleSwitchDesign : PropertyToggleSwitchViewModel
{
    public PropertyToggleSwitchDesign()
        : base(NavId.GenerateRandomAsString())
    {
        DesignTime.ThrowIfNotDesignMode();
        Header = "Toggle";
        ShortHeader = "Tgl";
        Description = "Toggle switch property with boolean value, busy state, and error state.";
        Icon = MaterialIconKind.ToggleSwitch;
        IconColor = AsvColorKind.Info5;
        CheckedText = "ON";
        CheckedIcon = MaterialIconKind.CheckCircle;
        CheckedStatus = AsvColorKind.Success;
        UncheckedText = "OFF";
        UncheckedIcon = MaterialIconKind.CloseCircle;
        UncheckedStatus = AsvColorKind.Error;
        ApplyValueFromModel(true);

        Menu.Add(
            new MenuItem("enable", "Enable")
            {
                Icon = MaterialIconKind.ToggleSwitch,
                Command = DesignTime.EmptyCommand,
            }
        );
        Menu.Add(
            new MenuItem("disable", "Disable")
            {
                Icon = MaterialIconKind.ToggleSwitchOff,
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
                ApplyValueFromModel(!Value.Value);
            })
            .AddTo(ref DisposableBag);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                ErrorMessage = ErrorMessage == null ? "Toggle sync error" : null;
            })
            .AddTo(ref DisposableBag);
    }

    protected override ValueTask ApplyFromUser(bool value, CancellationToken cancel)
    {
        return ValueTask.CompletedTask;
    }
}
