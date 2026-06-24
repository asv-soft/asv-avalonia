using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class PropertyToggleButtonGroupDesign : PropertyToggleButtonGroupViewModel
{
    public PropertyToggleButtonGroupDesign()
        : base(NavId.GenerateRandomAsString())
    {
        DesignTime.ThrowIfNotDesignMode();
        ShortHeader = "Mode";
        Header = "Optimization mode";
        Description = "Select one option from a compact button group.";
        Icon = MaterialIconKind.Tune;
        IconColor = AsvColorKind.Success;

        var firstItem = AddItem(
            "speed",
            "Speed",
            "Prefer faster processing.",
            MaterialIconKind.Signal,
            AsvColorKind.Success
        );
        AddItem(
            "quality",
            "Quality",
            "Prefer higher quality.",
            MaterialIconKind.CheckCircle,
            AsvColorKind.Success
        );
        AddItem(
            "balanced",
            "Balanced",
            "Use a balanced preset.",
            MaterialIconKind.ScaleBalance,
            AsvColorKind.Success
        );
        SelectedItem.Value = firstItem;

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => MarkUpdated())
            .AddTo(ref DisposableBag);
    }

    private IHeadlinedViewModel AddItem(
        string id,
        string header,
        string? description,
        MaterialIconKind? icon,
        AsvColorKind iconColor
    )
    {
        var item = new HeadlinedViewModel(id)
        {
            Header = header,
            Description = description,
            Icon = icon,
            IconColor = iconColor,
        };
        ItemsSource.Add(item);
        return item;
    }

    protected override ValueTask ApplyFromUser(IHeadlinedViewModel item, CancellationToken cancel)
    {
        return ValueTask.CompletedTask;
    }
}
