using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Asv.IO;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

[ExportSettings(SubPageId)]
public class SettingsConnectionViewModel : RoutableViewModel, ISettingsSubPage
{
    public const string SubPageId = "settings.connection";

    private readonly ISynchronizedView<
        KeyValuePair<string, IProtocolPort>,
        SettingsConnectionItemViewModel
    > _connections;

    private readonly IMavlinkConnectionService _connectionService;
    public BindableReactiveProperty<SettingsConnectionItemViewModel> SelectedItem { get; } = new();
    public NotifyCollectionChangedSynchronizedViewList<SettingsConnectionItemViewModel> Items { get; set; }

    [ImportingConstructor]
    public SettingsConnectionViewModel(
        IMavlinkConnectionService connectionService,
        ILoggerFactory logFactory,
        INavigationService navigationService
    )
        : base(SubPageId)
    {
        _connectionService = connectionService;
        _connections = _connectionService.Connections.CreateView(
            x => new SettingsConnectionItemViewModel(x.Key, x.Value, _connectionService)
        );
        Items = _connections.ToNotifyCollectionChanged();
        AddSerialPortCommand = new ReactiveCommand(_ =>
        {
            var serial = new SerialPortViewModel(
                "serial.dialog",
                connectionService,
                logFactory,
                this,
                navigationService
            );
            serial.ApplyAddDialog();
        });
        AddUdpPortCommand = new ReactiveCommand(_ =>
        {
            var udp = new UdpPortViewModel(
                "udp.dialog",
                connectionService,
                logFactory,
                this,
                navigationService
            );
            udp.ApplyAddDialog();
        });
        AddTcpPortCommand = new ReactiveCommand(_ =>
        {
            var tcp = new TcpPortViewModel(
                "tcp.dialog",
                connectionService,
                logFactory,
                this,
                navigationService
            );
            tcp.ApplyAddDialog();
        });
        EditPortCommand = new ReactiveCommand(_ =>
        {
            if (SelectedItem is null)
            {
                return;
            }

            switch (SelectedItem.CurrentValue.Port.CurrentValue)
            {
                case SerialProtocolPort serialProtocolPort:
                {
                    var dialog = new SerialPortViewModel(
                        serialProtocolPort,
                        SelectedItem.CurrentValue.Name.CurrentValue,
                        connectionService,
                        this,
                        navigationService
                    );
                    dialog.ApplyEditDialog();
                    break;
                }

                case UdpProtocolPort udpProtocolPort:
                {
                    var dialog = new UdpPortViewModel(
                        udpProtocolPort,
                        SelectedItem.CurrentValue.Name.CurrentValue,
                        connectionService,
                        this,
                        navigationService
                    );
                    dialog.ApplyEditDialog();
                    break;
                }

                case TcpClientProtocolPort
                or TcpServerProtocolPort:
                {
                    var dialog = new TcpPortViewModel(
                        SelectedItem.CurrentValue.Port.CurrentValue,
                        SelectedItem.CurrentValue.Name.CurrentValue,
                        connectionService,
                        navigationService,
                        this
                    );
                    dialog.ApplyEditDialog();
                    break;
                }
            }
        });
    }

    public SettingsConnectionViewModel()
        : base(string.Empty)
    {
        if (!Design.IsDesignMode)
        {
            return;
        }
    }

    public ReactiveCommand AddSerialPortCommand { get; set; }
    public ReactiveCommand AddUdpPortCommand { get; set; }
    public ReactiveCommand AddTcpPortCommand { get; set; }
    public ReactiveCommand EditPortCommand { get; set; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public IExportInfo Source => SystemModule.Instance;

    public ValueTask Init(ISettingsPage context)
    {
        return ValueTask.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            AddTcpPortCommand.Dispose();
            AddUdpPortCommand.Dispose();
            AddSerialPortCommand.Dispose();
            EditPortCommand.Dispose();
            Items.Dispose();
            _connections.Dispose();
            SelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }
}
