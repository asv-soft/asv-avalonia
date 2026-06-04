using Asv.Modeling;
using Avalonia.Controls;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class NavigationStatusItemViewModel : StatusItem
{
    private readonly ObservableList<NavigationStatusBreadcrumbItem> _source;
    public const string StaticId = "nav_crumbs";

    public NavigationStatusItemViewModel()
        : base(StaticId, default)
    {
        _source = new ObservableList<NavigationStatusBreadcrumbItem>();
        Items = _source.ToNotifyCollectionChangedSlim();

        _source.Add(
            new NavigationStatusBreadcrumbItem(
                "Shell",
                MaterialIconKind.Application,
                AsvColorKind.None
            )
        );
        _source.Add(
            new NavigationStatusBreadcrumbItem("Tab 1", MaterialIconKind.Tab, AsvColorKind.None)
        );
        _source.Add(new NavigationStatusBreadcrumbItem("Element 1", null, AsvColorKind.None));
    }

    public NavigationStatusItemViewModel(IShellHost nav)
        : base(StaticId, default)
    {
        _source = new ObservableList<NavigationStatusBreadcrumbItem>();
        Items = _source.ToNotifyCollectionChangedSlim();
        nav.ExecuteNowOrWhenShellLoaded(OnShellLoaded).AddTo(Disposable);
    }

    private void OnShellLoaded(IShell shell, TopLevel top)
    {
        shell.Navigation.SelectedControl.Subscribe(OnChanged).AddTo(Disposable);
    }

    private void OnChanged(IViewModel? routable)
    {
        _source.Clear();
        if (routable == null)
        {
            return;
        }

        foreach (var item in routable.GetHierarchyFromRoot().OfType<IViewModel>())
        {
            _source.Add(CreateItem(item));
        }
    }

    private static NavigationStatusBreadcrumbItem CreateItem(IViewModel item)
    {
        var title = item is IHasHeader { Header: { Length: > 0 } header } ? header : item.Id.TypeId;
        var icon = item is IHasIcon hasIcon ? hasIcon.Icon : null;
        var iconColor = item is IHasIcon hasIconColor ? hasIconColor.IconColor : AsvColorKind.None;

        return new NavigationStatusBreadcrumbItem(title, icon, iconColor);
    }

    public string? CommandHotKey
    {
        get;
        set => SetField(ref field, value);
    }

    public NotifyCollectionChangedSynchronizedViewList<NavigationStatusBreadcrumbItem> Items { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    public override int Order { get; } = int.MaxValue;
}

public sealed class NavigationStatusBreadcrumbItem(
    string title,
    MaterialIconKind? icon,
    AsvColorKind iconColor
)
{
    public string Title { get; } = title;
    public MaterialIconKind? Icon { get; } = icon;
    public AsvColorKind IconColor { get; } = iconColor;
    public bool HasIcon => Icon.HasValue;
}
