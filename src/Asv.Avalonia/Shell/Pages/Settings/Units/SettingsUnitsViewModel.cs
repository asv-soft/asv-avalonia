using System.Composition;
using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportSettings(PageId)]
public class SettingsUnitsViewModel : RoutableViewModel, ISettingsSubPage
{
    public const string PageId = "units";

    private readonly ObservableList<IUnit> _observableList;
    private readonly IDisposable _sub1;
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
        _observableList = new ObservableList<IUnit>(unit.Units.Values);
        _view = _observableList.CreateView(x => new MeasureUnitViewModel(x) { Parent = this });
        Items = _view.ToNotifyCollectionChanged();
        SelectedItem = new BindableReactiveProperty<MeasureUnitViewModel>();
        SearchText = new BindableReactiveProperty<string>();
        _sub1 = SearchText.ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                if (x.IsNullOrWhiteSpace())
                {
                    _view.ResetFilter();
                }
                else
                {
                    _view.AttachFilter(
                        new SynchronizedViewFilter<IUnit, MeasureUnitViewModel>((_, model) => model.Fitler(x)));
                }
            });
        
    }

    public NotifyCollectionChangedSynchronizedViewList<MeasureUnitViewModel> Items { get; }


    public BindableReactiveProperty<MeasureUnitViewModel> SelectedItem { get; }
    public BindableReactiveProperty<string> SearchText { get; }

    public ValueTask Init(ISettingsPage context)
    {
        return ValueTask.CompletedTask;
    }

    public override ValueTask<IRoutable> NavigateTo(string id)
    {
        var item = (IRoutable?)_view.FirstOrDefault(x => x.Id == id) ?? this;
        return ValueTask.FromResult(item);
    }
}