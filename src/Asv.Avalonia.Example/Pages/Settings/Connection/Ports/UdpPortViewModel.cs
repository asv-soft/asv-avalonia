using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia.Example;

public partial class UdpPortViewModel : ViewModelBaseWithValidation
{
    #region Subs

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private readonly IDisposable _sub3;
    private readonly IDisposable _sub4;
    private readonly IDisposable _sub5;

    #endregion

    private static readonly Regex IpRegex = IpRegexCtor();
    private readonly IRoutable _parent;
    private readonly ILogger _log;

    [ImportingConstructor]
    public UdpPortViewModel(
        string id,
        IMavlinkConnectionService connectionService,
        ILoggerFactory logFactory,
        IRoutable parent
    )
        : base(id)
    {
        _parent = parent;
        _log = logFactory.CreateLogger<UdpPortViewModel>();
        var currentIndex =
            connectionService.Connections.Count(pair => pair.Value.TypeInfo.Scheme == "udp") + 1;
        TitleInput = new BindableReactiveProperty<string>($"New UDP {currentIndex}").EnableValidation();
        LocalIpAddressInput = new BindableReactiveProperty<string>("0.0.0.0").EnableValidation();
        LocalPortInput = new BindableReactiveProperty<string>("0").EnableValidation();
        RemotePortInput = new BindableReactiveProperty<string>("0").EnableValidation();
        RemoteIpAddressInput = new BindableReactiveProperty<string>("0.0.0.0").EnableValidation();
        IsRemoteInput = new BindableReactiveProperty<bool>();

        _sub1 = TitleInput.Subscribe(t =>
        {
            if (string.IsNullOrWhiteSpace(t))
            {
                TitleInput.OnErrorResume(new Exception("Name is required"));
            }
        });
        _sub2 = LocalIpAddressInput.Subscribe(t =>
        {
            if (!IpRegex.IsMatch(t))
            {
                LocalIpAddressInput.OnErrorResume(new Exception("Wrong IP address value"));
            }
        });
        _sub3 = RemoteIpAddressInput.Subscribe(t =>
        {
            if (!IsRemoteInput.CurrentValue)
            {
                return;
            }

            if (!IpRegex.IsMatch(t))
            {
                RemoteIpAddressInput.OnErrorResume(new Exception("Wrong IP address value"));
            }
        });
        _sub4 = LocalPortInput.Subscribe(p =>
        {
            if (int.TryParse(p, out var port))
            {
                if (port is > ushort.MaxValue or < ushort.MinValue)
                {
                    LocalPortInput.OnErrorResume(new Exception("Port value out of bounds"));
                }
            }
            else
            {
                LocalPortInput.OnErrorResume(new Exception("Invalid port value"));
            }
        });
        _sub5 = RemotePortInput.Subscribe(p =>
        {
            if (!IsRemoteInput.CurrentValue)
            {
                return;
            }

            if (int.TryParse(p, out var port))
            {
                if (port is > ushort.MaxValue or < ushort.MinValue)
                {
                    RemotePortInput.OnErrorResume(new Exception("Port value out of bounds"));
                }
            }
            else
            {
                RemotePortInput.OnErrorResume(new Exception("Invalid port value"));
            }
        });
        SubscribeToErrorsChanged();
    }

    public async Task ApplyDialog()
    {
        var dialog = new ContentDialog
        {
            PrimaryButtonText = "Create",
            SecondaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = IsValid.CurrentValue.IsSuccess,
            IsSecondaryButtonEnabled = true,
            Content = this,
            PrimaryButtonCommand = new ReactiveCommand(_ =>
            {
                var persistable = PersistInputValueUdp();
                var cmd = new InternalContextCommand(AddConnectionPortHistoryCommand.Id, _parent, persistable);
                cmd.Execute(persistable);
            }),
        };

        IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled.IsSuccess);

        await dialog.ShowAsync();
    }

    private Persistable<KeyValuePair<string, string>> PersistInputValueUdp()
    {
        if (!IsValid.CurrentValue.IsSuccess)
        {
            _log.ZLogError($"Unable To create UDP connection. Input is not valid");
            return default;
        }

        var connectionString =
            $"udp://{LocalIpAddressInput.CurrentValue}:{LocalPortInput.CurrentValue}"
            + (
                IsRemoteInput.CurrentValue
                    ? $"?rhost={RemoteIpAddressInput.CurrentValue}&rport={RemotePortInput.CurrentValue}"
                    : string.Empty
            );
        return new Persistable<KeyValuePair<string, string>>(
            new KeyValuePair<string, string>(TitleInput.Value, connectionString));
    }

    public BindableReactiveProperty<string> TitleInput { get; set; }
    public BindableReactiveProperty<string> LocalIpAddressInput { get; set; } 
    public BindableReactiveProperty<string> LocalPortInput { get; set; } 
    public BindableReactiveProperty<bool> IsRemoteInput { get; set; }
    public BindableReactiveProperty<string> RemoteIpAddressInput { get; set; } 
    public BindableReactiveProperty<string> RemotePortInput { get; set; }

    [GeneratedRegex(@"^(\d{0,3}\.?){0,4}$")]
    private static partial Regex IpRegexCtor();
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
            _sub4.Dispose();
            _sub5.Dispose();
        }

        base.Dispose(disposing);
    }
}