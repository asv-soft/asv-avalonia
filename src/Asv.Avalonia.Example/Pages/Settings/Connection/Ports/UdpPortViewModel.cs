﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia.Example;

public class UdpPortViewModel : DialogViewModelBase
{
    private readonly IRoutable _parent;
    private readonly ILogger _log;
    private readonly IProtocolPort _oldPort;
    private readonly IMavlinkConnectionService _connectionService;
    private readonly INavigationService _navigation;

    private const string DefaultIpAddressConst = "172.16.0.1";
    private const string DefaultPortConst = "7341";

    [ImportingConstructor]
    public UdpPortViewModel(
        string id,
        IMavlinkConnectionService connectionService,
        ILoggerFactory logFactory,
        IRoutable parent,
        INavigationService navigation
    )
        : base(id)
    {
        _navigation = navigation;
        _connectionService = connectionService;
        _parent = parent;
        _log = logFactory.CreateLogger<UdpPortViewModel>();
        var currentIndex =
            connectionService.Connections.Count(pair => pair.Value.TypeInfo.Scheme == "udp") + 1;
        TitleInput = new BindableReactiveProperty<string>($"New UDP {currentIndex}");
        LocalIpAddressInput = new BindableReactiveProperty<string>(DefaultIpAddressConst);
        LocalPortInput = new BindableReactiveProperty<string>(DefaultPortConst);
        RemotePortInput = new BindableReactiveProperty<string>(DefaultPortConst);
        RemoteIpAddressInput = new BindableReactiveProperty<string>(DefaultIpAddressConst);
        IsRemoteInput = new BindableReactiveProperty<bool>();

        SubscribeToValidation();
    }

    public UdpPortViewModel(
        UdpProtocolPort oldPort,
        string name,
        IMavlinkConnectionService service,
        IRoutable parent,
        INavigationService navigation
    )
        : base("dialog.udpEdit")
    {
        _navigation = navigation;
        _parent = parent;
        _connectionService = service;
        _oldPort = oldPort;
        if (oldPort.Config is not UdpProtocolPortConfig cfg)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(cfg.Port);

        var remote = cfg.GetRemoteEndpoint();
        TitleInput = new BindableReactiveProperty<string>(name);
        LocalIpAddressInput = new BindableReactiveProperty<string>(cfg.Host ?? string.Empty);
        LocalPortInput = new BindableReactiveProperty<string>(cfg.Port.Value.ToString());
        IsRemoteInput = new BindableReactiveProperty<bool>(remote is not null);
        RemotePortInput = new BindableReactiveProperty<string>();
        RemoteIpAddressInput = new BindableReactiveProperty<string>();
        if (remote != null)
        {
            RemotePortInput.Value = remote.Port.ToString();
            RemoteIpAddressInput.Value = remote.Address.ToString();
        }

        SubscribeToValidation();
    }

    private void SubscribeToValidation()
    {
        _sub1 = TitleInput.EnableValidation(
            t =>
                string.IsNullOrWhiteSpace(t)
                    ? ValueTask.FromResult<ValidationResult>(new Exception("Name is required"))
                    : ValidationResult.Success,
            this,
            true
        );
        _sub2 = LocalIpAddressInput.EnableValidation(
            t =>
                !IPEndPoint.TryParse(t, out _)
                    ? ValueTask.FromResult<ValidationResult>(
                        new Exception("Wrong IP address value")
                    )
                    : ValidationResult.Success,
            this,
            true
        );
        _sub3 = RemoteIpAddressInput.EnableValidation(
            t =>
            {
                if (!IsRemoteInput.CurrentValue)
                {
                    return ValidationResult.Success;
                }

                return !IPEndPoint.TryParse(t, out _)
                    ? ValueTask.FromResult<ValidationResult>(
                        new Exception("Wrong IP address value")
                    )
                    : ValidationResult.Success;
            },
            this,
            true
        );
        _sub4 = LocalPortInput.EnableValidation(
            p =>
            {
                if (int.TryParse(p, out var port))
                {
                    if (port is > ushort.MaxValue or < ushort.MinValue)
                    {
                        return ValueTask.FromResult<ValidationResult>(
                            new Exception("Port value out of bounds")
                        );
                    }
                }
                else
                {
                    return ValueTask.FromResult<ValidationResult>(
                        new Exception("Invalid port value")
                    );
                }

                return ValidationResult.Success;
            },
            this,
            true
        );
        _sub5 = RemotePortInput.EnableValidation(
            p =>
            {
                if (!IsRemoteInput.CurrentValue)
                {
                    return ValidationResult.Success;
                }

                if (int.TryParse(p, out var port))
                {
                    if (port is > ushort.MaxValue or < ushort.MinValue)
                    {
                        return ValueTask.FromResult<ValidationResult>(
                            new Exception("Port value out of bounds")
                        );
                    }
                }
                else
                {
                    return ValueTask.FromResult<ValidationResult>(
                        new Exception("Invalid port value")
                    );
                }

                return ValidationResult.Success;
            },
            this,
            true
        );
    }

    public void ApplyAddDialog()
    {
        var dialog = new ContentDialog(_navigation)
        {
            PrimaryButtonText = "Create",
            SecondaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = IsValid.CurrentValue,
            IsSecondaryButtonEnabled = true,
            Content = this,
            PrimaryButtonCommand = new ReactiveCommand(_ =>
            {
                var persistable = new Persistable<KeyValuePair<string, string>>(
                    PersistInputValueUdp()
                );
                var cmd = new InternalContextCommand(
                    AddConnectionPortHistoryCommand.Id,
                    _parent,
                    persistable
                );
                Task.Run(() => cmd.Execute(persistable));
            }).DisposeItWith(Disposable),
        };

        _sub6 = IsValid
            .Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled)
            .DisposeItWith(Disposable);

        dialog.ShowAsync();
    }

    public void ApplyEditDialog()
    {
        var dialog = new ContentDialog(_navigation)
        {
            PrimaryButtonText = "Apply",
            SecondaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = IsValid.CurrentValue,
            IsSecondaryButtonEnabled = true,
            Content = this,
            PrimaryButtonCommand = new ReactiveCommand(_ =>
            {
                _connectionService.RemovePort(_oldPort, false);
                var persistable = new Persistable<EditConnectionPersistable>(
                    new EditConnectionPersistable()
                    {
                        NewValue = PersistInputValueUdp(),
                        Port = _oldPort,
                    }
                );
                var cmd = new InternalContextCommand(
                    EditConnectionPortHistoryCommand.Id,
                    _parent,
                    persistable
                );
                cmd.Execute(persistable);
            }).DisposeItWith(Disposable),
        };

        _sub7 = IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled);

        dialog.ShowAsync();
    }

    private KeyValuePair<string, string> PersistInputValueUdp()
    {
        if (!IsValid.CurrentValue)
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
        return new KeyValuePair<string, string>(TitleInput.Value, connectionString);
    }

    public BindableReactiveProperty<string> TitleInput { get; }
    public BindableReactiveProperty<string> LocalIpAddressInput { get; }
    public BindableReactiveProperty<string> LocalPortInput { get; }
    public BindableReactiveProperty<bool> IsRemoteInput { get; }
    public static string[] PresetIpValues => [DefaultIpAddressConst, "127.0.0.1"];
    public BindableReactiveProperty<string> RemoteIpAddressInput { get; }
    public BindableReactiveProperty<string> RemotePortInput { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    #region Dispose

    private IDisposable _sub1;
    private IDisposable _sub2;
    private IDisposable _sub3;
    private IDisposable _sub4;
    private IDisposable _sub5;
    private IDisposable _sub6;
    private IDisposable _sub7;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
            _sub4.Dispose();
            _sub5.Dispose();
            _sub6.Dispose();
            _sub7.Dispose();
            TitleInput.Dispose();
            LocalIpAddressInput.Dispose();
            LocalPortInput.Dispose();
            RemoteIpAddressInput.Dispose();
            RemotePortInput.Dispose();
            IsRemoteInput.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
