using Asv.Common;
using Asv.IO;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

public class PortViewModel : RoutableViewModel, IPortViewModel
{
    private MaterialIconKind? _icon;

    public PortViewModel()
        : this("designTime")
    {
        DesignTime.ThrowIfNotDesignMode();
        InitArgs(Guid.NewGuid().ToString());
        Icon = MaterialIconKind.Connection;
        TagsSource.Add(new TagViewModel("ip") { Key = "ip", Value = "127.0.0.1" });
        TagsSource.Add(new TagViewModel("port") { Key = "port", Value = "7341" });
    }

    public PortViewModel(NavigationId id)
        : base(id)
    {
        Name = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        TagsView = TagsSource.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public virtual void Init(IProtocolPort protocolPort)
    {
        InitArgs(protocolPort.Id);
        if (protocolPort.Config.Name != null)
        {
            Name.Value = protocolPort.Config.Name;
        }
    }

    public BindableReactiveProperty<string> Name { get; }

    protected readonly ObservableList<TagViewModel> TagsSource = [];
    public NotifyCollectionChangedSynchronizedViewList<TagViewModel> TagsView { get; }

    private void UpdateStatus(ProtocolPortStatus status)
    {
        switch (status)
        {
            case ProtocolPortStatus.Disconnected:

                break;
            case ProtocolPortStatus.InProgress:
                break;
            case ProtocolPortStatus.Connected:
                break;
            case ProtocolPortStatus.Error:
                IsError = true;
                IsSuccess = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    public bool IsError { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsInProgress { get; set; }

    public MaterialIconKind? Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public IExportInfo Source => IoModule.Instance;
}
