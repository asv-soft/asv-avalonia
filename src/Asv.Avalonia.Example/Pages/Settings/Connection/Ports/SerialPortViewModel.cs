using System;
using System.Collections.Generic;
using System.Composition;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.IO;
using Avalonia.Controls;
using DotNext.Collections.Generic;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia.Example;

public class SerialPortViewModel : ViewModelBaseWithValidation
{
    #region Subs

    private readonly IDisposable _titleSub;
    private readonly IDisposable _bufferSizeSub;
    private readonly IDisposable _selectedPortSub;
    private readonly IDisposable _boundRateSub;
    private readonly IDisposable _paritySub;
    private readonly IDisposable _timeoutSub;
    private readonly IDisposable _stopBitsSub;
    private readonly IDisposable _dataBitsSub;

    #endregion
    
    private readonly IMavlinkConnectionService? _service;
    private readonly IRoutable _parent;
    private readonly ILogger _log;
    private int _requestNotComplete;
    private const int WriteBufferSizeConst = 40960;
    private const int WriteTimeoutConst = 1000;
    private const int BoundRateConst = 115200;
    private const int DataBitsConst = 8;
    private readonly ObservableList<string> _myCache = [];
    private readonly IProtocolPort _oldPort;

    [ImportingConstructor]
    public SerialPortViewModel(
        string id,
        IMavlinkConnectionService service,
        ILoggerFactory logFactory,
        IRoutable parent
    )
        : base(id)
    {
        _parent = parent;
        _log = logFactory.CreateLogger<SerialPortViewModel>();
        _service = service;
        var currentIndex =
            service.Connections.Count(pair => pair.Value.TypeInfo.Scheme == "serial") + 1;
        Title = new BindableReactiveProperty<string>($"New Serial {currentIndex}").EnableValidation();
        WriteBufferSizeInput = new BindableReactiveProperty<string>(WriteBufferSizeConst.ToString()).EnableValidation();
        SelectedPortInput = new BindableReactiveProperty<string>().EnableValidation();
        SelectedBaudRateInput = new BindableReactiveProperty<string>(BoundRateConst.ToString()).EnableValidation();
        ParityInput = new BindableReactiveProperty<Parity?>(Parity.None).EnableValidation();
        WriteTimeOutInput = new BindableReactiveProperty<string>(WriteTimeoutConst.ToString()).EnableValidation();
        StopBitsInput = new BindableReactiveProperty<StopBits?>(StopBits.None).EnableValidation();
        DataBitsInput = new BindableReactiveProperty<string>(DataBitsConst.ToString()).EnableValidation();
        Ports = _myCache.ToNotifyCollectionChanged();
        _titleSub = Title.Subscribe(t =>
        {
            if (string.IsNullOrWhiteSpace(t))
            {
                Title.OnErrorResume(
                    new Exception("Name is required")
                );
            }
        });
        _bufferSizeSub = WriteBufferSizeInput.Subscribe(b =>
        {
            if (!int.TryParse(b, out _))
            {
                WriteBufferSizeInput.OnErrorResume(new Exception("Invalid size of buffer"));
            }
        });
        _selectedPortSub = SelectedPortInput.Subscribe(p =>
        {
            if (string.IsNullOrWhiteSpace(p))
            {
                SelectedPortInput.OnErrorResume(new Exception("Port is required"));
            }
        });
        _boundRateSub = SelectedBaudRateInput.Subscribe(b =>
        {
            if (!int.TryParse(b, out _))
            {
                SelectedBaudRateInput.OnErrorResume(new Exception("Invalid baud rate"));
            }
        });
        _paritySub = ParityInput.Subscribe(p =>
        {
            if (p is null)
            {
                ParityInput.OnErrorResume(new Exception("Invalid parity"));
            }
        });
        _timeoutSub = WriteTimeOutInput.Subscribe(t =>
        {
            if (!int.TryParse(t, out _))
            {
                WriteTimeOutInput.OnErrorResume(new Exception("Invalid timeout value"));
            }
        });
        _dataBitsSub = DataBitsInput.Subscribe(d =>
        {
            if (!int.TryParse(d, out var bits))
            {
                DataBitsInput.OnErrorResume(new Exception("Invalid data bits value"));
            }

            if (bits is > 8 or < 5)
            {
                DataBitsInput.OnErrorResume(new Exception("Data bits should be digit value from 5 to 8"));
            }
        });
        _stopBitsSub = StopBitsInput.Subscribe(s =>
        {
            if (s is null)
            {
                StopBitsInput.OnErrorResume(new Exception("Invalid value of stop bits"));
            }
        });

        Observable
            .Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
            .Subscribe(_ => UpdateSerialPorts());
        SubscribeToErrorsChanged();
    }

    public SerialPortViewModel(SerialProtocolPort oldPort, string name,  IMavlinkConnectionService service,  IRoutable parent)
        : base("dialog.serialEdit")
    {
        _oldPort = oldPort;
        _service = service;
        _parent = parent;
        if (oldPort.Config is not SerialProtocolPortConfig config)
        {
            return;
        }
        
        Title = new BindableReactiveProperty<string>(name).EnableValidation();
       
            SelectedBaudRateInput =
                new BindableReactiveProperty<string>(config.BoundRate.ToString()).EnableValidation();
        SelectedPortInput =
            new BindableReactiveProperty<string>(config.PortName ?? string.Empty).EnableValidation();
        ParityInput = new BindableReactiveProperty<Parity?>(config.Parity).EnableValidation();
        DataBitsInput = new BindableReactiveProperty<string>(config.DataBits.ToString()).EnableValidation();
        StopBitsInput = new BindableReactiveProperty<StopBits?>(config.StopBits).EnableValidation();
        WriteTimeOutInput =
            new BindableReactiveProperty<string>(config.WriteTimeout.ToString()).EnableValidation();
        WriteBufferSizeInput =
            new BindableReactiveProperty<string>(config.WriteBufferSize.ToString()).EnableValidation();
         Ports = _myCache.ToNotifyCollectionChanged();
        _titleSub = Title.Subscribe(t =>
        {
            if (string.IsNullOrWhiteSpace(t))
            {
                Title.OnErrorResume(
                    new Exception("Name is required")
                );
            }
        });
        _bufferSizeSub = WriteBufferSizeInput.Subscribe(b =>
        {
            if (!int.TryParse(b, out _))
            {
                WriteBufferSizeInput.OnErrorResume(new Exception("Invalid size of buffer"));
            }
        });
        _selectedPortSub = SelectedPortInput.Subscribe(p =>
        {
            if (string.IsNullOrWhiteSpace(p))
            {
                SelectedPortInput.OnErrorResume(new Exception("Port is required"));
            }
        });
        _boundRateSub = SelectedBaudRateInput.Subscribe(b =>
        {
            if (!int.TryParse(b, out _))
            {
                SelectedBaudRateInput.OnErrorResume(new Exception("Invalid baud rate"));
            }
        });
        _paritySub = ParityInput.Subscribe(p =>
        {
            if (p is null)
            {
                ParityInput.OnErrorResume(new Exception("Invalid parity"));
            }
        });
        _timeoutSub = WriteTimeOutInput.Subscribe(t =>
        {
            if (!int.TryParse(t, out _))
            {
                WriteTimeOutInput.OnErrorResume(new Exception("Invalid timeout value"));
            }
        });
        _dataBitsSub = DataBitsInput.Subscribe(d =>
        {
            if (!int.TryParse(d, out var bits))
            {
                DataBitsInput.OnErrorResume(new Exception("Invalid data bits value"));
            }

            if (bits is > 8 or < 5)
            {
                DataBitsInput.OnErrorResume(new Exception("Data bits should be digit value from 5 to 8"));
            }
        });
        _stopBitsSub = StopBitsInput.Subscribe(s =>
        {
            if (s is null)
            {
                StopBitsInput.OnErrorResume(new Exception("Invalid value of stop bits"));
            }
        });

        Observable
            .Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
            .Subscribe(_ => UpdateSerialPorts());
        SubscribeToErrorsChanged();
    }
    
    public SerialPortViewModel()
        : base(string.Empty)
    {
        if (Design.IsDesignMode)
        {
        }
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
                var persist = PersistInputValueSerial();
                var cmd = new InternalContextCommand(AddConnectionPortHistoryCommand.Id, _parent, persist);
                cmd.Execute(persist);
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
                _service!.RemovePort(_oldPort, false);
                var persist = PersistInputValueSerial();
                var cmd = new InternalContextCommand(AddConnectionPortHistoryCommand.Id, _parent, persist);
                cmd.Execute(persist);
            }),
        };
        IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled.IsSuccess);
        await dialog.ShowAsync();
    }

    private Persistable<KeyValuePair<string, string>> PersistInputValueSerial()
    {
        var connectionString = string.Empty;
        if (_service == null)
        {
            return default;
        }

        try
        {
            connectionString =
                $"serial:{SelectedPortInput.CurrentValue}"
                + $"?br={SelectedBaudRateInput.CurrentValue}"
                + $"&wrt={WriteTimeOutInput.CurrentValue}"
                + $"&parity={ParityInput.CurrentValue}"
                + $"&dataBits={DataBitsInput.CurrentValue}"
                + $"&stopBits={StopBitsInput.CurrentValue}"
                + $"&ws={WriteBufferSizeInput.CurrentValue}";
        }
        catch (Exception? e)
        {
            _log.ZLogError($"Error to create port:{e.Message}", e);
        }
        finally
        {
            UpdateSerialPorts();
        }
        
        return new Persistable<KeyValuePair<string, string>>(new(Title.CurrentValue, connectionString));
    }

    private void UpdateSerialPorts()
    {
        if (Interlocked.CompareExchange(ref _requestNotComplete, 1, 0) != 0)
        {
            return;
        }

        try
        {
            var value = SerialPort.GetPortNames();
            var exist = _myCache.ToArray();
            var toDelete = exist.Except(value).ToArray();
            var toAdd = value.Except(exist).ToArray();
            foreach (var item in toDelete)
            {
                _myCache.Remove(item);
            }

            _myCache.AddAll(toAdd);
        }
        catch (Exception e)
        {
            _log.ZLogError($"Error to create port:{e.Message}", e);
        }
        finally
        {
            Interlocked.Exchange(ref _requestNotComplete, 0);
        }
    }

    public NotifyCollectionChangedSynchronizedViewList<string> Ports { get; set; }

    public BindableReactiveProperty<Array> BaudRates { get; } =
        new(new[] { 9600, 14400, 19200, 38400, 56000, 57600, 115200, 128000, 256000 });

    public BindableReactiveProperty<string> Title { get; set; }
    public BindableReactiveProperty<string> SelectedBaudRateInput { get; set; }
    public BindableReactiveProperty<string> SelectedPortInput { get; set; }
    public BindableReactiveProperty<Array> ParityValues => new(Enum.GetValues<Parity>());
    public BindableReactiveProperty<Parity?> ParityInput { get; set; }

    public BindableReactiveProperty<string> WriteTimeOutInput { get; set; } 

    public BindableReactiveProperty<string> WriteBufferSizeInput { get; set; }
    public BindableReactiveProperty<Array> DataBitsValues => new(new[] { 5, 6, 7, 8 });
    public BindableReactiveProperty<string> DataBitsInput { get; set; }
    public BindableReactiveProperty<StopBits?> StopBitsInput { get; set; }

    public BindableReactiveProperty<Array> StopBitsArr => new(Enum.GetValues<StopBits>());

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _titleSub.Dispose();
            _bufferSizeSub.Dispose();
            _selectedPortSub.Dispose();
            _boundRateSub.Dispose();
            _paritySub.Dispose();
            _stopBitsSub.Dispose();
            _dataBitsSub.Dispose();
            _timeoutSub.Dispose();
        }

        base.Dispose(disposing);
    }
}