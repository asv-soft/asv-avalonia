using System.Composition;
using Asv.Cfg;
using Asv.IO;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportSettings(SubPageId)]
public class SettingsConnectionViewModel : RoutableViewModel, ISettingsSubPage
{
    private readonly ISynchronizedView<IProtocolPort, SettingsConnectionItemViewModel> _view;
    private readonly ObservableList<IProtocolPort> _observableList;
    private IConfiguration _cfg;
    public BindableReactiveProperty<SettingsConnectionItemViewModel> SelectedItem { get; set; }
    public const string SubPageId = "settings.connection";
    public NotifyCollectionChangedSynchronizedViewList<SettingsConnectionItemViewModel> Items { get; }

    [ImportingConstructor]
    public SettingsConnectionViewModel(IConfiguration cfg,
        IMavlinkConnectionService connectionService)
        : base(SubPageId)
    {
        _cfg = cfg;
        _observableList = new ObservableList<IProtocolPort>(connectionService.Router.Ports);
        _view = _observableList.CreateView(x => new SettingsConnectionItemViewModel(x, connectionService));
        Items = _view.ToNotifyCollectionChanged();
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        // var item = _view.FirstOrDefault(x => x.Id == id);
        // if (item != null)
        // {
        //     SelectedItem.Value = item;
        //     return ValueTask.FromResult<IRoutable>(item);
        // }
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public IExportInfo Source => SystemModule.Instance;

    public ValueTask Init(ISettingsPage context)
    {
        throw new NotImplementedException();
    }
}