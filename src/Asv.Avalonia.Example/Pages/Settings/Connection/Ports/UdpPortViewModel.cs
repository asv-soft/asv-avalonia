using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia.Example;

public partial class UdpPortViewModel : RoutableViewModel
{
    private readonly IMavlinkConnectionService _connectionService;
    private readonly ILogger _log;
    private static readonly Regex IpRegex = IpRegexCtor();

    [ImportingConstructor]
    public UdpPortViewModel(
        string id,
        IMavlinkConnectionService connectionService,
        ILoggerFactory logFactory
    )
        : base(id)
    {
        _connectionService = connectionService;
        _log = logFactory.CreateLogger<UdpPortViewModel>();
        var currentIndex =
            connectionService.Connections.Count(pair => pair.Value.TypeInfo.Scheme == "udp") + 1;
        TitleInput = new BindableReactiveProperty<string>($"New UDP {currentIndex}");
        LocalPortInput.Subscribe(_ => ValidateAndUpdate());
        RemotePortInput.Subscribe(_ => ValidateAndUpdate());
        LocalIpAddressInput.Subscribe(_ => ValidateAndUpdate());
        RemoteIpAddressInput.Subscribe(_ => ValidateAndUpdate());
        IsRemoteInput.Subscribe(_ => ValidateAndUpdate());
    }

    public void AddUdpPort()
    {
        if (!IsInputValid.CurrentValue)
        {
            _log.ZLogError($"Unable To create TCP connection. Input is not valid");
            return;
        }

        var connectionString =
            $"udp://{LocalIpAddressInput.CurrentValue}:{LocalPortInput.CurrentValue}"
            + (
                IsRemoteInput.CurrentValue
                    ? $"?rhost={RemoteIpAddressInput.CurrentValue}&rport={RemotePortInput.CurrentValue}"
                    : string.Empty
            );
        _connectionService.AddConnection(TitleInput.CurrentValue, connectionString);
    }

    private void ValidateAndUpdate()
    {
        var isLocalIpValid = IpRegex.IsMatch(LocalIpAddressInput.CurrentValue);
        var isLocalPortValid =
            int.TryParse(LocalPortInput.CurrentValue, out var localPort)
            && localPort is <= ushort.MaxValue and >= 0;
        var isLocalValid = isLocalIpValid && isLocalPortValid;
        IsInputValid.Value = isLocalValid;
        if (!IsRemoteInput.CurrentValue)
        {
            return;
        }

        var isRemoteIpValid = IpRegex.IsMatch(RemoteIpAddressInput.CurrentValue);
        var isRemotePortValid =
            int.TryParse(RemotePortInput.CurrentValue, out var remotePort)
            && remotePort is <= ushort.MaxValue and >= 0;
        IsInputValid.Value = isLocalValid && isRemoteIpValid && isRemotePortValid;
    }

    public BindableReactiveProperty<string> TitleInput { get; set; }
    public ReactiveProperty<bool> IsInputValid { get; set; } = new();
    public BindableReactiveProperty<string> LocalIpAddressInput { get; set; } = new(string.Empty);
    public BindableReactiveProperty<string> LocalPortInput { get; set; } = new();
    public BindableReactiveProperty<bool> IsRemoteInput { get; set; } = new(false);
    public BindableReactiveProperty<string> RemoteIpAddressInput { get; set; } = new(string.Empty);
    public BindableReactiveProperty<string> RemotePortInput { get; set; } = new();

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    [GeneratedRegex(@"^(\d{0,3}\.?){0,4}$")]
    private static partial Regex IpRegexCtor();
}
