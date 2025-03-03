using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Asv.Cfg;
using Asv.IO;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

[ExportSettings(SubPageId)]
public class SettingsConnectionViewModel : RoutableViewModel, ISettingsSubPage
{
    private ISynchronizedView<
        KeyValuePair<string, IProtocolPort>,
        SettingsConnectionItemViewModel
    > Connections =>
        _connectionService.Connections.CreateView(x => new SettingsConnectionItemViewModel(
            x.Key,
            x.Value,
            _connectionService
        ));
    private readonly IMavlinkConnectionService _connectionService;
    private IConfiguration _cfg;
    public BindableReactiveProperty<SettingsConnectionItemViewModel> SelectedItem { get; set; }
    public const string SubPageId = "settings.connection";
    public NotifyCollectionChangedSynchronizedViewList<SettingsConnectionItemViewModel> Items { get; set; }

    [ImportingConstructor]
    public SettingsConnectionViewModel(
        IConfiguration cfg,
        IMavlinkConnectionService connectionService,
        ILoggerFactory logFactory
    )
        : base(SubPageId)
    {
        _cfg = cfg;
        _connectionService = connectionService;

        Items = Connections.ToNotifyCollectionChanged();
        connectionService.Router.PortAdded.Subscribe(_ => UpdateView());
        connectionService.Router.PortRemoved.Subscribe(_ => UpdateView());
        AddSerialPortCommand = new ReactiveCommand(
            async (_, __) =>
            {
                var serial = new SerialPortViewModel(
                    "serial.dialog",
                    connectionService,
                    logFactory
                );
                var dialog = new ContentDialog()
                {
                    PrimaryButtonText = "Create",
                    SecondaryButtonText = "Cancel",
                    IsPrimaryButtonEnabled = serial.IsValid.CurrentValue,
                    IsSecondaryButtonEnabled = true,
                    Content = serial,
                    PrimaryButtonCommand = new ReactiveCommand(_ => serial.AddSerialPort()),
                };
                serial.IsValid.Subscribe(enabled =>
                {
                    dialog.IsPrimaryButtonEnabled = enabled;
                });

                await dialog.ShowAsync();
            }
        );
        AddUdpPortCommand = new ReactiveCommand(
            async (_, ct) =>
            {
                var udp = new UdpPortViewModel("serial.dialog", connectionService, logFactory);
                var dialog = new ContentDialog()
                {
                    PrimaryButtonText = "Create",
                    SecondaryButtonText = "Cancel",
                    IsPrimaryButtonEnabled = udp.IsInputValid.CurrentValue,
                    IsSecondaryButtonEnabled = true,
                    Content = udp,
                    PrimaryButtonCommand = new ReactiveCommand(_ =>
                    {
                        udp.AddUdpPort();
                    }),
                };

                udp.IsInputValid.Subscribe(enabled =>
                {
                    dialog.IsPrimaryButtonEnabled = enabled;
                });

                await dialog.ShowAsync();
            }
        );
        AddTcpPortCommand = new ReactiveCommand(
            async (_, ct) =>
            {
                var tcp = new TcpPortViewModel("serial.dialog", connectionService, logFactory);
                var dialog = new ContentDialog()
                {
                    PrimaryButtonText = "Create",
                    SecondaryButtonText = "Cancel",
                    IsPrimaryButtonEnabled = tcp.IsValid.CurrentValue,
                    IsSecondaryButtonEnabled = true,
                    Content = tcp,
                    PrimaryButtonCommand = new ReactiveCommand(_ =>
                    {
                        tcp.AddTcpPort();
                    }),
                };

                tcp.IsValid.Subscribe(enabled =>
                {
                    dialog.IsPrimaryButtonEnabled = enabled;
                });

                await dialog.ShowAsync();
            }
        );
    }

    private void UpdateView() { }

    public ReactiveCommand AddSerialPortCommand { get; set; }
    public ReactiveCommand AddUdpPortCommand { get; set; }
    public ReactiveCommand AddTcpPortCommand { get; set; }

    public override ValueTask<IRoutable> Navigate(string id)
    {
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
