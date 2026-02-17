using Asv.Cfg;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class TcpPortViewModelConfig
{
    public Dictionary<string, string> HostHistory { get; set; } =
        new() { { "127.0.0.1", "localhost" }, { "172.16.0.1", "Base station" } };

    public Dictionary<string, string> PortHistory { get; set; } =
        new()
        {
            { "7341", "Base station" },
            { "5760", "SITL main" },
            { "5762", "SITL reserved" },
        };
}

[Export(TcpClientProtocolPort.Scheme, typeof(IPortViewModel))]
public class TcpPortViewModel : PortViewModel
{
    private readonly IConfiguration _cfgSvc;
    public const MaterialIconKind DefaultIcon = MaterialIconKind.UploadNetworkOutline;

    public TcpPortViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Config = new TcpPortViewModelConfig();
        UpdateTags(TcpClientProtocolPortConfig.CreateDefault());
        ConnectionString = TcpClientProtocolPortConfig.CreateDefault().AsUri().ToString();
    }

    public TcpPortViewModel(
        IConfiguration cfgSvc,
        IUnitService unitService,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : base($"{TcpClientProtocolPort.Scheme}-editor", unitService, loggerFactory, timeProvider)
    {
        _cfgSvc = cfgSvc;
        Icon = DefaultIcon;
        Config = _cfgSvc.Get<TcpPortViewModelConfig>();
        AddToValidation(Host = new BindableReactiveProperty<string>(), HostValidate);
        AddToValidation(PortNumber = new BindableReactiveProperty<string>(), PortValidate);
    }

    private Exception? PortValidate(string arg)
    {
        return null;
    }

    public TcpPortViewModelConfig Config { get; }

    private Exception? HostValidate(string arg)
    {
        return null;
    }

    public BindableReactiveProperty<string> Host { get; }
    public BindableReactiveProperty<string> PortNumber { get; }

    protected override void InternalLoadChanges(ProtocolPortConfig config)
    {
        base.InternalLoadChanges(config);
        if (config is TcpClientProtocolPortConfig tcpConfig)
        {
            UpdateTags(tcpConfig);
            Host.Value = tcpConfig.Host ?? string.Empty;
            PortNumber.Value = tcpConfig.Port?.ToString() ?? string.Empty;
        }
    }

    protected override void InternalSaveChanges(ProtocolPortConfig config)
    {
        base.InternalSaveChanges(config);
        if (config is TcpClientProtocolPortConfig tcpConfig)
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

    private void UpdateTags(TcpClientProtocolPortConfig config)
    {
        ConfigTag.Value = $"{config.Host}:{config.Port}";
        TypeTag.Value = RS.TcpPortViewModel_TagViewModel_Value;
        TypeTag.Color = AsvColorKind.Info2;
    }
}
