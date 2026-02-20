using Asv.Cfg;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class TcpServerPortViewModelConfig
{
    public Dictionary<string, string> HostHistory { get; set; } =
        new() { { "127.0.0.1", "localhost" }, { "0.0.0.0", "All local endpoint" } };

    public Dictionary<string, string> PortHistory { get; set; } =
        new() { { "7341", "Base station" } };
}

public class TcpServerPortViewModel : PortViewModel
{
    public const MaterialIconKind DefaultIcon = MaterialIconKind.DownloadNetworkOutline;

    private readonly IConfiguration _cfgSvc;
    private readonly IUnitService _unitService;

    public TcpServerPortViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Config = new TcpServerPortViewModelConfig();
        UpdateTags(TcpServerProtocolPortConfig.CreateDefault());
        ConnectionString = TcpServerProtocolPortConfig.CreateDefault().AsUri().ToString();
    }

    public TcpServerPortViewModel(
        IUnitService unitService,
        IConfiguration cfgSvc,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : base($"{TcpServerProtocolPort.Scheme}-editor", unitService, loggerFactory, timeProvider)
    {
        _cfgSvc = cfgSvc;
        _unitService = unitService;
        Icon = DefaultIcon;
        Config = _cfgSvc.Get<TcpServerPortViewModelConfig>();
        AddToValidation(Host = new BindableReactiveProperty<string>(), HostValidate);
        AddToValidation(PortNumber = new BindableReactiveProperty<string>(), PortValidate);
    }

    public TcpServerPortViewModelConfig Config { get; }

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
        if (config is TcpServerProtocolPortConfig tcpConfig)
        {
            UpdateTags(tcpConfig);
            Host.Value = tcpConfig.Host ?? string.Empty;
            PortNumber.Value = tcpConfig.Port?.ToString() ?? string.Empty;
        }
    }

    protected override void InternalSaveChanges(ProtocolPortConfig config)
    {
        base.InternalSaveChanges(config);
        if (config is TcpServerProtocolPortConfig tcpConfig)
        {
            tcpConfig.Host = Host.Value;
            tcpConfig.Port = int.Parse(PortNumber.Value);

            Config.PortHistory.TryAdd(
                tcpConfig.Port?.ToString() ?? throw new InvalidOperationException(),
                string.Empty
            );
            Config.HostHistory.TryAdd(tcpConfig.Host, string.Empty);
            _cfgSvc.Set(Config);
        }
    }

    private void UpdateTags(TcpServerProtocolPortConfig config)
    {
        ConfigTag.Value = $"{config.Host}:{config.Port}";
        TypeTag.Value = RS.TcpServerPortViewModel_TagViewModel_Value;
        TypeTag.Color = AsvColorKind.Info3;
    }

    protected override EndpointViewModel EndpointFactory(IProtocolEndpoint arg)
    {
        return new TcpServerEndpointViewModel(arg, _unitService, LoggerFactory, TimeProvider);
    }
}
