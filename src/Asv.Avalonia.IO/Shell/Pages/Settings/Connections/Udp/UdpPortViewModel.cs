using Asv.Cfg;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class UdpPortViewModelConfig
{
    public Dictionary<string, string> HostHistory { get; set; } =
        new() { { "127.0.0.1", "localhost" } };

    public Dictionary<string, string> PortHistory { get; set; } =
        new() { { "7341", "Base station" } };
}

public class UdpPortViewModel : PortViewModel
{
    public const MaterialIconKind DefaultIcon = MaterialIconKind.IpNetworkOutline;
    private readonly IConfiguration _cfgSvc;
    private readonly IUnitService _unitService;

    public UdpPortViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Config = new UdpPortViewModelConfig();
        UpdateTags(UdpProtocolPortConfig.CreateDefault());
        ConnectionString = UdpProtocolPortConfig.CreateDefault().AsUri().ToString();
    }

    public UdpPortViewModel(
        IConfiguration cfgSvc,
        IUnitService unitService,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : base($"{UdpProtocolPort.Scheme}-editor", unitService, loggerFactory, timeProvider)
    {
        _cfgSvc = cfgSvc;
        _unitService = unitService;
        Icon = DefaultIcon;
        Config = _cfgSvc.Get<UdpPortViewModelConfig>();
        AddToValidation(Host = new BindableReactiveProperty<string>(), HostValidate);
        AddToValidation(PortNumber = new BindableReactiveProperty<string>(), PortValidate);
        AddToValidation(RemoteHost = new BindableReactiveProperty<string>(), HostValidate);
        AddToValidation(RemotePort = new BindableReactiveProperty<string>(), PortValidate);
    }

    public UdpPortViewModelConfig Config { get; }

    private Exception? PortValidate(string arg)
    {
        return null;
    }

    private Exception? HostValidate(string arg)
    {
        return null;
    }

    public BindableReactiveProperty<string> Host { get; }
    public BindableReactiveProperty<string> PortNumber { get; }

    protected override void InternalLoadChanges(ProtocolPortConfig config)
    {
        base.InternalLoadChanges(config);
        if (config is UdpProtocolPortConfig udpConfig)
        {
            UpdateTags(udpConfig);
            Host.Value = udpConfig.Host ?? string.Empty;
            PortNumber.Value = udpConfig.Port?.ToString() ?? string.Empty;
            var remote = udpConfig.GetRemoteEndpoint();
            if (remote != null)
            {
                RemoteEnabled = true;
                RemoteHost.Value = remote.Address.ToString();
                RemotePort.Value = remote.Port.ToString();
            }
            else
            {
                RemoteEnabled = false;
                RemoteHost.Value = string.Empty;
                RemotePort.Value = string.Empty;
            }
        }
    }

    public bool RemoteEnabled
    {
        get;
        set => SetField(ref field, value);
    }

    public BindableReactiveProperty<string> RemoteHost { get; }
    public BindableReactiveProperty<string> RemotePort { get; }

    protected override void InternalSaveChanges(ProtocolPortConfig config)
    {
        base.InternalSaveChanges(config);
        if (config is UdpProtocolPortConfig udpConfig)
        {
            udpConfig.Host = Host.Value;
            udpConfig.Port = int.Parse(PortNumber.Value);

            if (RemoteEnabled)
            {
                udpConfig.Query[UdpProtocolPortConfig.RemoteHostKey] = RemoteHost.Value;
                udpConfig.Query[UdpProtocolPortConfig.RemotePortKey] = RemotePort.Value;
            }
            else
            {
                udpConfig.Query[UdpProtocolPortConfig.RemoteHostKey] = null;
                udpConfig.Query[UdpProtocolPortConfig.RemotePortKey] = null;
            }

            Config.PortHistory.TryAdd(
                udpConfig.Port?.ToString() ?? throw new InvalidOperationException(),
                string.Empty
            );
            Config.HostHistory.TryAdd(udpConfig.Host, string.Empty);
            _cfgSvc.Set(Config);
        }
    }

    private void UpdateTags(UdpProtocolPortConfig config)
    {
        ConfigTag.Value = $"{config.Host}:{config.Port}";
        TypeTag.Value = RS.UdpPortViewModel_TagViewModel_Value;
        TypeTag.Color = AsvColorKind.Info4;
    }

    protected override EndpointViewModel EndpointFactory(IProtocolEndpoint arg)
    {
        return new UdpEndpointViewModel(arg, _unitService, LoggerFactory, TimeProvider);
    }
}
