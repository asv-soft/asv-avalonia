using System.Composition;
using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportSettings(PageId)]
public class SettingsUnitsViewModel : SettingsSubPage
{
    public const string PageId = "units";
    private readonly ISynchronizedView<IUnit, MeasureUnitViewModel> _view;

    public SettingsUnitsViewModel()
        : this(DesignTime.UnitService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SettingsUnitsViewModel(IUnitService unit)
        : base(PageId)
    {
        var observableList = new ObservableList<IUnit>(unit.Units.Values);
        _view = observableList.CreateView(x => new MeasureUnitViewModel(x) { Parent = this });
        Items = _view.ToNotifyCollectionChanged().DisposeItWith(Disposable);
        SelectedItem = new BindableReactiveProperty<MeasureUnitViewModel>().DisposeItWith(
            Disposable
        );
        SearchText = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        SearchText
            .ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                if (x.IsNullOrWhiteSpace())
                {
                    _view.ResetFilter();
                }
                else
                {
                    _view.AttachFilter(
                        new SynchronizedViewFilter<IUnit, MeasureUnitViewModel>(
                            (_, model) => model.Filter(x)
                        )
                    );
                }
            })
            .DisposeItWith(Disposable);
    }

    public NotifyCollectionChangedSynchronizedViewList<MeasureUnitViewModel> Items { get; }

    public BindableReactiveProperty<MeasureUnitViewModel> SelectedItem { get; }
    public BindableReactiveProperty<string> SearchText { get; }

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        var item = _view.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            SelectedItem.Value = item;
            return ValueTask.FromResult<IRoutable>(item);
        }

        return base.Navigate(id);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var child in base.GetRoutableChildren())
        {
            yield return child;
        }

        foreach (var model in _view)
        {
            yield return model;
        }
    }

    public override IExportInfo Source => SystemModule.Instance;
}
