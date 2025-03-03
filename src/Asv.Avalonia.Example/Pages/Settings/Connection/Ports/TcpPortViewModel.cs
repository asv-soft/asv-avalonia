using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

public partial class TcpPortViewModel : RoutableViewModel
{
    private ILogger _log;
    private readonly IMavlinkConnectionService _connectionService;
    private static readonly Regex IpRegex = IpRegexCtor();

    [ImportingConstructor]
    public TcpPortViewModel(
        string id,
        IMavlinkConnectionService connectionService,
        ILoggerFactory logFactory
    )
        : base(id)
    {
        _connectionService = connectionService;
        CreationNumber =
            connectionService.Connections.Count(_ => _.Value.TypeInfo.Scheme == "tcp") + 1;
        Title = new BindableReactiveProperty<string>($"New TCP {CreationNumber}");
        _log = logFactory.CreateLogger<TcpPortViewModel>();
        PortInput.Subscribe(_ => ValidateAndUpdate());
        IpAddressInput.Subscribe(_ => ValidateAndUpdate());
    }

    public void AddTcpPort()
    {
        if (IsValid.CurrentValue)
        {
            var connectionString =
                $"tcp://{IpAddressInput.CurrentValue}:{PortInput.CurrentValue}"
                + (IsTcpIpServer.CurrentValue ? "?srv=true" : string.Empty);
            _connectionService.AddConnection(Title.CurrentValue, connectionString);
        }
    }

    private void ValidateAndUpdate()
    {
        var isIpAddressValid = IpRegex.IsMatch(IpAddressInput.CurrentValue);
        var isPortValid =
            int.TryParse(PortInput.CurrentValue, out var result)
            && result is <= ushort.MaxValue and >= 0;
        var isValid = isIpAddressValid && isPortValid;

        IsValid.Value = isValid;
    }

    private int CreationNumber { get; set; }
    public BindableReactiveProperty<string> Title { get; set; }
    public BindableReactiveProperty<string> IpAddressInput { get; set; } = new(string.Empty);
    public BindableReactiveProperty<string> PortInput { get; set; } = new(string.Empty);
    public BindableReactiveProperty<bool> IsTcpIpServer { get; set; } = new(false);
    public ReactiveProperty<bool> IsValid { get; set; } = new(false);

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
