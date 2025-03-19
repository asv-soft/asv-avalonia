﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia.Example;

public class TcpPortViewModel : DialogViewModelBase
{
    private IDisposable _sub1;
    private IDisposable _sub2;
    private IDisposable _sub3;
    private readonly IRoutable _parent;
    private readonly ILogger _log;
    private readonly IProtocolPort _oldPort;
    private readonly IMavlinkConnectionService _connectionService;
    private readonly INavigationService _navigation;
    private const string DefaultIpAddressConst = "172.16.0.1";
    private const string DefaultPortConst = "7341";

    [ImportingConstructor]
    public TcpPortViewModel(
        string id,
        IMavlinkConnectionService connectionService,
        ILoggerFactory logFactory,
        IRoutable parent,
        INavigationService navigation
    )
        : base(id)
    {
        _navigation = navigation;
        _parent = parent;
        CreationNumber =
            connectionService.Connections.Count(_ => _.Value.TypeInfo.Scheme == "tcp") + 1;
        Title = new BindableReactiveProperty<string>(
            $"New TCP {CreationNumber}"
        ).EnableValidation();
        PortInput = new BindableReactiveProperty<string>(DefaultPortConst).EnableValidation();
        IpAddressInput = new BindableReactiveProperty<string>(
            DefaultIpAddressConst
        ).EnableValidation();
        _log = logFactory.CreateLogger<TcpPortViewModel>();
        SubscribeToValidation();
    }

    public TcpPortViewModel(
        IProtocolPort oldport,
        string name,
        IMavlinkConnectionService connectionService,
        INavigationService navigation,
        IRoutable parent
    )
        : base("dialog.TcpEdit")
    {
        _connectionService = connectionService;
        _navigation = navigation;
        _oldPort = oldport;
        _parent = parent;

        switch (oldport)
        {
            case TcpClientProtocolPort client:
                Title = new BindableReactiveProperty<string>(name);
                PortInput = new BindableReactiveProperty<string>(client.Config.Port.ToString()!);
                IpAddressInput = new BindableReactiveProperty<string>(client.Config.Host!);
                IsTcpIpServer = new BindableReactiveProperty<bool>();
                break;
            case TcpServerProtocolPort server:
                Title = new BindableReactiveProperty<string>(name);
                PortInput = new BindableReactiveProperty<string>(server.Config.Port.ToString()!);
                IpAddressInput = new BindableReactiveProperty<string>(server.Config.Host!);
                IsTcpIpServer = new BindableReactiveProperty<bool>(true);
                break;
        }

        SubscribeToValidation();
    }

    private void SubscribeToValidation()
    {
        _sub1 = Title.EnableValidation(
            t =>
            {
                if (string.IsNullOrWhiteSpace(t))
                {
                    return ValueTask.FromResult<ValidationResult>(
                        new Exception("Title must be not empty")
                    );
                }

                return ValidationResult.Success;
            },
            this,
            true
        );
        _sub2 = IpAddressInput.EnableValidation(
            i =>
            {
                if (!IPEndPoint.TryParse(i, out _))
                {
                    return ValueTask.FromResult<ValidationResult>(
                        new Exception("Invalid IP address")
                    );
                }

                return ValidationResult.Success;
            },
            this,
            true
        );
        _sub3 = PortInput.EnableValidation(
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
                    PersistInputValueTcp()
                );
                var cmd = new InternalContextCommand(
                    AddConnectionPortHistoryCommand.Id,
                    _parent,
                    persistable
                );
                Task.Run(() => cmd.Execute(persistable));
            }),
        };
        IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled);

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
                        NewValue = PersistInputValueTcp(),
                        Port = _oldPort,
                    }
                );
                var cmd = new InternalContextCommand(
                    AddConnectionPortHistoryCommand.Id,
                    _parent,
                    persistable
                );
                cmd.Execute(persistable);
            }),
        };
        IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled);
        dialog.ShowAsync();
    }

    private KeyValuePair<string, string> PersistInputValueTcp()
    {
        if (!IsValid.CurrentValue)
        {
            _log.ZLogError($"Unable To create TCP connection. Input is not valid");
            return default;
        }

        var connection =
            (IsTcpIpServer.CurrentValue ? "tcps" : "tcp")
            + $"://{IpAddressInput.CurrentValue}:{PortInput.CurrentValue}"
            + (IsTcpIpServer.CurrentValue ? "?srv=true" : string.Empty);
        return new KeyValuePair<string, string>(Title.CurrentValue, connection);
    }

    private int CreationNumber { get; set; }
    public BindableReactiveProperty<string> Title { get; set; }
    public BindableReactiveProperty<string> IpAddressInput { get; set; }
    public static string[] PresetIpValues => [DefaultIpAddressConst, "127.0.0.1"];
    public BindableReactiveProperty<string> PortInput { get; set; }
    public BindableReactiveProperty<bool> IsTcpIpServer { get; set; } = new(false);

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

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
