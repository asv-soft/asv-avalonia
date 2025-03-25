using System.Composition;
using Asv.Common;
using Asv.IO;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

[ExportSettings(SubPageId)]
public class SettingsConnectionViewModel : SettingsSubPage
{
    private readonly ObservableList<IProtocolPort> _source;
    private readonly ISynchronizedView<IProtocolPort, PortViewModel> _sourceSyncView;

    public const string SubPageId = "settings.connection1";

    public SettingsConnectionViewModel()
        : this(NullDeviceManager.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        var source = new ObservableList<PortViewModel>
        {
            new PortViewModel(),
            new PortViewModel(),
            new PortViewModel(),
        };
        View = source.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    [ImportingConstructor]
    public SettingsConnectionViewModel(IDeviceManager deviceManager)
        : base(SubPageId)
    {
        _source = [];
        _sourceSyncView = _source
            .CreateView(x => new PortViewModel(x, deviceManager))
            .DisposeItWith(Disposable);
        _sourceSyncView.DisposeRemovedViewItems().DisposeItWith(Disposable);
        View = _sourceSyncView.ToNotifyCollectionChanged();

        foreach (var port in deviceManager.Router.Ports)
        {
            _source.Add(port);
        }

        deviceManager.Router.PortAdded.Subscribe(x => _source.Add(x)).DisposeItWith(Disposable);

        deviceManager
            .Router.PortRemoved.Subscribe(x => _source.Remove(x))
            .DisposeItWith(Disposable);

        Menu.Add(new MenuItem("add.serial", "Add Serial"));
        Menu.Add(new MenuItem("add.tcp", "Add TCP"));
        Menu.Add(new MenuItem("add.udp", "Add UDP"));
    }

    public NotifyCollectionChangedSynchronizedViewList<PortViewModel> View { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var child in base.GetRoutableChildren())
        {
            yield return child;
        }

        foreach (var model in View)
        {
            yield return model;
        }
    }

    public override IExportInfo Source => IoModule.Instance;
}
