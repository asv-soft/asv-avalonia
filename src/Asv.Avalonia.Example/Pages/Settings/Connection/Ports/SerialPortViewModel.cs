using System;
using System.Collections.Generic;
using System.Composition;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.IO;
using Avalonia.Controls;
using DotNext.Collections.Generic;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia.Example;

public class SerialPortViewModel : DialogViewModelBase
{
    private readonly IMavlinkConnectionService? _service;
    private readonly INavigationService _navigation;
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
        IRoutable parent,
        INavigationService navigation
    )
        : base(id)
    {
        _navigation = navigation;
        _parent = parent;
        _log = logFactory.CreateLogger<SerialPortViewModel>();
        _service = service;
        var currentIndex =
            service.Connections.Count(pair => pair.Value.TypeInfo.Scheme == "serial") + 1;
        Title = new BindableReactiveProperty<string>($"New Serial {currentIndex}");
        WriteBufferSizeInput = new BindableReactiveProperty<string>(
            WriteBufferSizeConst.ToString()
        );
        SelectedPortInput = new BindableReactiveProperty<string>();
        SelectedBaudRateInput = new BindableReactiveProperty<string>(BoundRateConst.ToString());
        ParityInput = new BindableReactiveProperty<Parity?>(Parity.None);
        WriteTimeOutInput = new BindableReactiveProperty<string>(WriteTimeoutConst.ToString());
        StopBitsInput = new BindableReactiveProperty<StopBits?>(StopBits.None);
        DataBitsInput = new BindableReactiveProperty<string>(DataBitsConst.ToString());
        Ports = _myCache.ToNotifyCollectionChanged();

        Observable
            .Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
            .Subscribe(_ => UpdateSerialPorts());
        SubscribeToValidation();
    }

    public SerialPortViewModel(
        SerialProtocolPort oldPort,
        string name,
        IMavlinkConnectionService service,
        IRoutable parent,
        INavigationService navigation
    )
        : base("dialog.serialEdit")
    {
        _navigation = navigation;
        _oldPort = oldPort;
        _service = service;
        _parent = parent;
        IsValid.Value = false;
        if (oldPort.Config is not SerialProtocolPortConfig config)
        {
            return;
        }

        Title = new BindableReactiveProperty<string>(name);

        SelectedBaudRateInput = new BindableReactiveProperty<string>(config.BoundRate.ToString());
        SelectedPortInput = new BindableReactiveProperty<string>(config.PortName ?? string.Empty);
        ParityInput = new BindableReactiveProperty<Parity?>(config.Parity);
        DataBitsInput = new BindableReactiveProperty<string>(config.DataBits.ToString());
        StopBitsInput = new BindableReactiveProperty<StopBits?>(config.StopBits);
        WriteTimeOutInput = new BindableReactiveProperty<string>(config.WriteTimeout.ToString());
        WriteBufferSizeInput = new BindableReactiveProperty<string>(
            config.WriteBufferSize.ToString()
        );
        Ports = _myCache.ToNotifyCollectionChanged();

        _sub1 = Observable
            .Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
            .Subscribe(_ => UpdateSerialPorts());
        SubscribeToValidation();
    }

    private void SubscribeToValidation()
    {
        _titleSub = Title.EnableValidation(
            t =>
                string.IsNullOrWhiteSpace(t)
                    ? ValueTask.FromResult<ValidationResult>(new Exception("Name is required"))
                    : ValidationResult.Success,
            this,
            true
        );
        _bufferSizeSub = WriteBufferSizeInput.EnableValidation(
            b =>
                !int.TryParse(b, out _)
                    ? ValueTask.FromResult<ValidationResult>(
                        new Exception("Invalid size of buffer")
                    )
                    : ValidationResult.Success,
            this,
            true
        );
        _selectedPortSub = SelectedPortInput.EnableValidation(
            p =>
                string.IsNullOrWhiteSpace(p)
                    ? ValueTask.FromResult<ValidationResult>(new Exception("Port is required"))
                    : ValidationResult.Success,
            this,
            true
        );
        _boundRateSub = SelectedBaudRateInput.EnableValidation(
            b =>
                !int.TryParse(b, out _)
                    ? ValueTask.FromResult<ValidationResult>(new Exception("Invalid baud rate"))
                    : ValidationResult.Success,
            this,
            true
        );
        _paritySub = ParityInput.EnableValidation(
            p =>
                p is null
                    ? ValueTask.FromResult<ValidationResult>(new Exception("Invalid parity"))
                    : ValidationResult.Success,
            this,
            true
        );
        _timeoutSub = WriteTimeOutInput.EnableValidation(
            t =>
                !int.TryParse(t, out _)
                    ? ValueTask.FromResult<ValidationResult>(new Exception("Invalid timeout value"))
                    : ValidationResult.Success,
            this,
            true
        );
        _dataBitsSub = DataBitsInput.EnableValidation(
            d =>
            {
                if (!int.TryParse(d, out var bits))
                {
                    return ValueTask.FromResult<ValidationResult>(
                        new Exception("Invalid data bits value")
                    );
                }

                return bits is > 8 or < 5
                    ? ValueTask.FromResult<ValidationResult>(
                        new Exception("Data bits should be digit value from 5 to 8")
                    )
                    : ValidationResult.Success;
            },
            this,
            true
        );
        _stopBitsSub = StopBitsInput.EnableValidation(
            s =>
                s is null
                    ? ValueTask.FromResult<ValidationResult>(
                        new Exception("Invalid value of stop bits")
                    )
                    : ValidationResult.Success,
            this,
            true
        );
    }

    public SerialPortViewModel()
        : base(string.Empty)
    {
        if (Design.IsDesignMode) { }
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
                var persist = new Persistable<KeyValuePair<string, string>>(
                    PersistInputValueSerial()
                );
                var cmd = new InternalContextCommand(
                    AddConnectionPortHistoryCommand.Id,
                    _parent,
                    persist
                );
                Task.Run(() => cmd.Execute(persist));
            }).DisposeItWith(Disposable),
        };
        _sub2 = IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled);
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
                var persist = new Persistable<EditConnectionPersistable>(
                    new EditConnectionPersistable()
                    {
                        NewValue = PersistInputValueSerial(),
                        Port = _oldPort,
                    }
                );
                var cmd = new InternalContextCommand(
                    EditConnectionPortHistoryCommand.Id,
                    _parent,
                    persist
                );
                cmd.Execute(persist);
            }).DisposeItWith(Disposable),
        };
        _sub3 = IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled);
        dialog.ShowAsync();
    }

    private KeyValuePair<string, string> PersistInputValueSerial()
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

        return new KeyValuePair<string, string>(Title.CurrentValue, connectionString);
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

    public BindableReactiveProperty<string> Title { get; }
    public BindableReactiveProperty<string> SelectedBaudRateInput { get; }
    public BindableReactiveProperty<string> SelectedPortInput { get; }
    public BindableReactiveProperty<Array> ParityValues => new(Enum.GetValues<Parity>());
    public BindableReactiveProperty<Parity?> ParityInput { get; }

    public BindableReactiveProperty<string> WriteTimeOutInput { get; }

    public BindableReactiveProperty<string> WriteBufferSizeInput { get; }
    public BindableReactiveProperty<Array> DataBitsValues => new(new[] { 5, 6, 7, 8 });
    public BindableReactiveProperty<string> DataBitsInput { get; }
    public BindableReactiveProperty<StopBits?> StopBitsInput { get; }

    public BindableReactiveProperty<Array> StopBitsArr => new(Enum.GetValues<StopBits>());

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private IDisposable _sub2;
    private IDisposable _sub3;
    private IDisposable _titleSub;
    private IDisposable _bufferSizeSub;
    private IDisposable _selectedPortSub;
    private IDisposable _boundRateSub;
    private IDisposable _paritySub;
    private IDisposable _timeoutSub;
    private IDisposable _stopBitsSub;
    private IDisposable _dataBitsSub;

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
            Title.Dispose();
            WriteBufferSizeInput.Dispose();
            SelectedPortInput.Dispose();
            SelectedBaudRateInput.Dispose();
            ParityInput.Dispose();
            WriteTimeOutInput.Dispose();
            StopBitsInput.Dispose();
            DataBitsInput.Dispose();
            Ports.Dispose();
            ParityValues.Dispose();
            StopBitsInput.Dispose();
            StopBitsArr.Dispose();
            _myCache.Clear();
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
