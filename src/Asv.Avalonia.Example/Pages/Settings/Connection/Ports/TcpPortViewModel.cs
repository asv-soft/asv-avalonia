using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia.Example;

public partial class TcpPortViewModel : ViewModelBaseWithValidation
{
    private IDisposable _sub1;
    private IDisposable _sub2;
    private IDisposable _sub3;
    private IRoutable _parent;
    private ILogger _log;
    private static readonly Regex IpRegex = IpRegexCtor();
    private readonly IProtocolPort _oldPort;
    private IMavlinkConnectionService _connectionService;

    [ImportingConstructor]
    public TcpPortViewModel(
        string id,
        IMavlinkConnectionService connectionService,
        ILoggerFactory logFactory,
        IRoutable parent
    )
        : base(id)
    {
        _parent = parent;
        CreationNumber =
            connectionService.Connections.Count(_ => _.Value.TypeInfo.Scheme == "tcp") + 1;
        Title = new BindableReactiveProperty<string>($"New TCP {CreationNumber}").EnableValidation();
        PortInput = new BindableReactiveProperty<string>().EnableValidation();
        IpAddressInput = new BindableReactiveProperty<string>("0.0.0.0").EnableValidation();
        _log = logFactory.CreateLogger<TcpPortViewModel>();
        SubscribeToValidation();
    }

    public TcpPortViewModel(IProtocolPort oldport, string name, IMavlinkConnectionService connectionService,
        IRoutable parent) : base("dialog.TcpEdit")
    {
        _connectionService = connectionService;
        _oldPort = oldport;
        _parent = parent;

        switch (oldport)
        {
            case TcpClientProtocolPort client:
                Title = new BindableReactiveProperty<string>(name).EnableValidation();
                PortInput = new BindableReactiveProperty<string>(client.Config.Port.ToString()!);
                IpAddressInput = new BindableReactiveProperty<string>(client.Config.Host!);
                IsTcpIpServer = new BindableReactiveProperty<bool>();
                break;
            case TcpServerProtocolPort server:
                Title = new BindableReactiveProperty<string>(name).EnableValidation();
                PortInput = new BindableReactiveProperty<string>(server.Config.Port.ToString()!);
                IpAddressInput = new BindableReactiveProperty<string>(server.Config.Host!);
                IsTcpIpServer = new BindableReactiveProperty<bool>(true);
                break;
        }

        SubscribeToValidation();
    }

    private void SubscribeToValidation()
    {
        _sub1 = Title.Subscribe(t =>
        {
            if (string.IsNullOrWhiteSpace(t))
            {
                Title.OnErrorResume(new Exception("Name required"));
            }
        });
        _sub2 = IpAddressInput.Subscribe(i =>
        {
            if (!IpRegex.IsMatch(i))
            {
                IpAddressInput.OnErrorResume(new Exception("Invalid IP address"));
            }
        });
        _sub3 = PortInput.Subscribe(p =>
        {
            if (int.TryParse(p, out var port))
            {
                if (port is > ushort.MaxValue or < ushort.MinValue)
                {
                    PortInput.OnErrorResume(new Exception("Port value out of bounds"));
                }
            }
            else
            {
                PortInput.OnErrorResume(new Exception("Invalid port value"));
            }
        });
        SubscribeToErrorsChanged();
    }

    public async Task ApplyAddDialog()
    {
        var dialog = new ContentDialog()
        {
            PrimaryButtonText = "Create",
            SecondaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = IsValid.CurrentValue.IsSuccess,
            IsSecondaryButtonEnabled = true,
            Content = this,
            PrimaryButtonCommand = new ReactiveCommand(_ =>
            {
                var persistable = PersistInputValueTcp();
                var cmd = new InternalContextCommand(AddConnectionPortHistoryCommand.Id, _parent, persistable);
                cmd.Execute(persistable);
            }),
        };
        IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled.IsSuccess);

        await dialog.ShowAsync();
    }

    public async Task ApplyEditDialog()
    {
        var dialog = new ContentDialog()
        {
            PrimaryButtonText = "Apply",
            SecondaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = IsValid.CurrentValue.IsSuccess,
            IsSecondaryButtonEnabled = true,
            Content = this,
            PrimaryButtonCommand = new ReactiveCommand(_ =>
            {
                _connectionService.RemovePort(_oldPort, false);
                var persistable = PersistInputValueTcp();
                var cmd = new InternalContextCommand(AddConnectionPortHistoryCommand.Id, _parent, persistable);
                cmd.Execute(persistable);
            }),
        };
        IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled.IsSuccess);

        await dialog.ShowAsync();
    }

    private Persistable<KeyValuePair<string, string>> PersistInputValueTcp()
    {
        if (!IsValid.CurrentValue.IsSuccess)
        {
            _log.ZLogError($"Unable To create TCP connection. Input is not valid");
            return default;
        }

        var connection = (IsTcpIpServer.CurrentValue ? "tcps" : "tcp") + $"://{IpAddressInput.CurrentValue}:{PortInput.CurrentValue}"
                         + (IsTcpIpServer.CurrentValue ? "?srv=true" : string.Empty);
        return
            new Persistable<KeyValuePair<string, string>>(
                new KeyValuePair<string, string>(Title.CurrentValue, connection));
    }

    private int CreationNumber { get; set; }
    public BindableReactiveProperty<string> Title { get; set; }
    public BindableReactiveProperty<string> IpAddressInput { get; set; }
    public BindableReactiveProperty<string> PortInput { get; set; }
    public BindableReactiveProperty<bool> IsTcpIpServer { get; set; } = new(false);

    [GeneratedRegex(@"^(\d{0,3}\.?){0,4}$")]
    private static partial Regex IpRegexCtor();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
        }

        base.Dispose(disposing);
    }
}