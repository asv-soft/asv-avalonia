using System.Composition;
using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportSettings(SubPageId)]
public class SettingsKeymapViewModel : RoutableViewModel, ISettingsSubPage
{
    private readonly ObservableList<ICommandInfo> _observableList;
    private readonly ISynchronizedView<ICommandInfo, SettingsKeyMapItemViewModel> _view;
    private readonly IDisposable _sub1;
    public const string SubPageId = "settings.hotkeys";

    public SettingsKeymapViewModel()
        : this(DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SettingsKeymapViewModel(ICommandService svc)
        : base(SubPageId)
    {
        SearchText = new BindableReactiveProperty<string>();
        _observableList = new ObservableList<ICommandInfo>(svc.Commands);
        SelectedItem = new BindableReactiveProperty<SettingsKeyMapItemViewModel>();
        _view = _observableList.CreateView(x => new SettingsKeyMapItemViewModel(x, svc)
        {
            Parent = this,
        });
        Items = _view.ToNotifyCollectionChanged();
        _sub1 = SearchText
            .ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                if (x.IsNullOrWhiteSpace())
                {
                    _view.ResetFilter();
                }
                else
                {
                    _view.AttachFilter((_, model) => model.Fitler(x));
                }
            });
    }

    public NotifyCollectionChangedSynchronizedViewList<SettingsKeyMapItemViewModel> Items { get; }
    public BindableReactiveProperty<string> SearchText { get; }
    public IBindableReactiveProperty<SettingsKeyMapItemViewModel> SelectedItem { get; }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        var item = _view.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            SelectedItem.Value = item;
            return ValueTask.FromResult<IRoutable>(item);
        }

        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return _view;
    }

    public ValueTask Init(ISettingsPage context)
    {
        return ValueTask.CompletedTask;
    }
}
