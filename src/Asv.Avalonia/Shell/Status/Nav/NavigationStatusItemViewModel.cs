using System.Composition;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportStatusItem]
public class NavigationStatusItemViewModel : StatusItem
{
    private readonly ObservableList<string> _source;
    public const string StaticId = "nav-crumbs";

    public NavigationStatusItemViewModel()
        : base(StaticId, DesignTime.LoggerFactory)
    {
        _source = new ObservableList<string>();
        Items = _source.ToNotifyCollectionChangedSlim();
        _source.Add("shell");
        _source.Add("tab1");
        _source.Add("element1");
    }

    [ImportingConstructor]
    public NavigationStatusItemViewModel(ILoggerFactory loggerFactory, INavigationService nav)
        : base(StaticId, loggerFactory)
    {
        _source = new ObservableList<string>();
        Items = _source.ToNotifyCollectionChangedSlim();
        nav.SelectedControl.Subscribe(OnChanged).AddTo(Disposable);
    }

    private void OnChanged(IRoutable? routable)
    {
        _source.Clear();
        if (routable == null)
        {
            return;
        }

        foreach (var item in routable.GetHierarchyFromRoot())
        {
            _source.Add(item.Id.Id);
        }
    }

    public NotifyCollectionChangedSynchronizedViewList<string> Items { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override int Order { get; } = 500;
}
