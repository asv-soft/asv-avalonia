using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class PropertyComboBoxViewModel : HeadlinedViewModel, IPropertyViewModel
{
    public PropertyComboBoxViewModel()
        : this(NavId.GenerateRandomAsString())
    {
        DesignTime.ThrowIfNotDesignMode();
        ShortName = "Mode";
        Icon = MaterialIconKind.Tune;
        Header = "Work mode";
        Description = "Select the desired work mode from the list.";
        var firstItem = AddItem(
            "manual_mode",
            "Manual",
            "Operator controls every step directly.",
            MaterialIconKind.Hand,
            AsvColorKind.Info1
        );
        AddItem(
            "guided_mode",
            "Guided",
            "Assisted flow with validation after each step.",
            MaterialIconKind.Compass,
            AsvColorKind.Info5
        );
        AddItem(
            "survey_mode",
            "Survey",
            "Collects structured measurements for later analysis.",
            MaterialIconKind.MapMarkerRadius,
            AsvColorKind.Success
        );
        AddItem(
            "silent_mode",
            "Silent",
            "Runs without icon or accent decoration.",
            null,
            AsvColorKind.None
        );
        AddItem(
            "inspection_mode",
            "Inspection",
            "Highlights issues and requires operator confirmation.",
            MaterialIconKind.MagnifyScan,
            AsvColorKind.Warning
        );
        AddItem(
            "minimal_mode",
            "Minimal",
            "Text-only item for compact layouts.",
            null,
            AsvColorKind.None
        );
        AddItem(
            "diagnostic_mode",
            "Diagnostics",
            "Shows service information and hardware status.",
            MaterialIconKind.Stethoscope,
            AsvColorKind.Info12
        );
        AddItem(
            "offline_mode",
            "Offline cache",
            "No image, useful when external data is unavailable.",
            null,
            AsvColorKind.None
        );
        AddItem(
            "emergency_mode",
            "Emergency",
            null,
            MaterialIconKind.AlertOctagon,
            AsvColorKind.Error
        );
        AddItem("custom_mode", "Custom profile", null, null, AsvColorKind.None);
        SelectedItem.Value = firstItem;

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                UpdatedFlag = !UpdatedFlag;
            })
            .AddTo(ref DisposableBag);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                IsBusy = !IsBusy;
            })
            .AddTo(ref DisposableBag);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                ErrorMessage = ErrorMessage == null ? "Network error" : null;
            })
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

    public PropertyComboBoxViewModel(string typeId)
        : base(typeId)
    {
        SelectedItem = new BindableReactiveProperty<IHeadlinedViewModel?>().AddTo(
            ref DisposableBag
        );
        ItemsView = ItemsSource.ToNotifyCollectionChangedSlim().AddTo(ref DisposableBag);
        SelectItemCommand = new ReactiveCommand<IHeadlinedViewModel>(item =>
        {
            SelectedItem.Value = item;
        }).AddTo(ref DisposableBag);
    }

    public NotifyCollectionChangedSynchronizedViewList<IHeadlinedViewModel> ItemsView { get; }

    public ObservableList<IHeadlinedViewModel> ItemsSource { get; } = [];

    public BindableReactiveProperty<IHeadlinedViewModel?> SelectedItem { get; }

    public ReactiveCommand<IHeadlinedViewModel> SelectItemCommand { get; }

    public bool UpdatedFlag
    {
        get;
        private set => SetField(ref field, value);
    }

    public string? ShortName
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsBusy
    {
        get;
        set => SetField(ref field, value);
    }

    public MaterialIconKind ErrorIcon
    {
        get;
        set => SetField(ref field, value);
    } = MaterialIconKind.CloseNetwork;

    public string? ErrorMessage
    {
        get;
        set => SetField(ref field, value);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return ItemsView;
    }
}
