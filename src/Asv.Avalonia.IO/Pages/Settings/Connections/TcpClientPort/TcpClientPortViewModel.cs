using System.Composition;
using Asv.Cfg;
using Asv.IO;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

public class TcpClientPortViewModelConfig
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
public class TcpClientPortViewModel : PortViewModel
{
    private readonly IConfiguration _cfgSvc;
    private readonly ObservableList<string> _hostHistorySource;
    public const MaterialIconKind DefaultIcon = MaterialIconKind.UploadNetworkOutline;

    public TcpClientPortViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Config = new TcpClientPortViewModelConfig();
        UpdateTags(TcpClientProtocolPortConfig.CreateDefault());
    }

    [ImportingConstructor]
    public TcpClientPortViewModel(IConfiguration cfgSvc)
        : base($"{TcpClientProtocolPort.Scheme}-editor")
    {
        _cfgSvc = cfgSvc;
        Icon = DefaultIcon;
        _hostHistorySource = [];
        Config = _cfgSvc.Get<TcpClientPortViewModelConfig>();
        AddToValidation(Host = new BindableReactiveProperty<string>(), HostValidate);
        AddToValidation(PortNumber = new BindableReactiveProperty<string>(), PortValidate);
    }

    private Exception? PortValidate(string arg)
    {
        return null;
    }

    public TcpClientPortViewModelConfig Config { get; }

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
        TagsSource.Clear();
        TagsSource.Add(
            new TagViewModel(nameof(config.Scheme))
            {
                Value = "TCP CLIENT",
                TagType = TagType.Info2,
            }
        );
        TagsSource.Add(
            new TagViewModel(nameof(config.Host))
            {
                Value = $"{config.Host}:{config.Port}",
                TagType = TagType.Success,
            }
        );
    }
}
