using System;
using System.Collections.Generic;
using System.Composition;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using DotNext.Collections.Generic;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia.Example;

public class SerialPortViewModel : RoutableViewModel
{
    private readonly IMavlinkConnectionService? _service;
    private readonly ILogger _log;
    private int _requestNotComplete;
    private const int WriteBufferSizeConst = 40960;
    private const int WriteTimeoutConst = 1000;
    private const int DataBitsConst = 8;

    private readonly ObservableList<string> _myCache = [];

    [ImportingConstructor]
    public SerialPortViewModel(
        string id,
        IMavlinkConnectionService service,
        ILoggerFactory logFactory
    )
        : base(id)
    {
        _log = logFactory.CreateLogger<SerialPortViewModel>();
        _service = service;
        var currentIndex =
            service.Connections.Count(pair => pair.Value.TypeInfo.Scheme == "serial") + 1;
        Title = new BindableReactiveProperty<string>($"New Serial {currentIndex}");
        WriteBufferSizeInput.Subscribe(_ => ValidateAndUpdate());
        DataBitsInput.Subscribe(_ => ValidateAndUpdate());
        WriteTimeOutInput.Subscribe(_ => ValidateAndUpdate());
        SelectedBoundRateInput.Subscribe(_ => ValidateAndUpdate());
        Title.Subscribe(_ => ValidateAndUpdate());
        SelectedPortInput.Subscribe(_ => ValidateAndUpdate());
        ParityInput.Subscribe(_ => ValidateAndUpdate());
        StopBitsInput.Subscribe(_ => ValidateAndUpdate());

        Ports = _myCache.ToNotifyCollectionChanged();
        Observable
            .Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
            .Subscribe(_ => UpdateSerialPorts())
            .DisposeItWith(Disposable);
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public void AddSerialPort()
    {
        if (_service == null)
        {
            return;
        }

        try
        {
            var connectionString =
                $"serial:{SelectedPortInput.CurrentValue}"
                + $"?br={SelectedBoundRateInput.CurrentValue}"
                + $"&wrt={WriteTimeOut.CurrentValue}"
                + $"&parity={ParityInput.CurrentValue}"
                + $"&dataBits={DataBits.CurrentValue}"
                + $"&stopBits={StopBitsInput.CurrentValue}"
                + $"&ws={WriteBufferSize.CurrentValue}";
            _service.AddConnection(Title.CurrentValue, connectionString);
        }
        catch (Exception? e)
        {
            _log.ZLogError($"Error to create port:{e.Message}", e);
        }
        finally
        {
            UpdateSerialPorts();
        }
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

    private void ValidateAndUpdate()
    {
        var isWriteBufferValid = int.TryParse(
            WriteBufferSizeInput.CurrentValue,
            out var writeBuffer
        );
        var isDataBitsValid = int.TryParse(DataBitsInput.CurrentValue, out var dataBits);
        var isWriteTimeoutValid = int.TryParse(
            WriteTimeOutInput.CurrentValue,
            out var writeTimeout
        );
        var isBoundRateValid = int.TryParse(SelectedBoundRateInput.CurrentValue, out _);
        var isTitleValid = !string.IsNullOrEmpty(Title.CurrentValue);
        var isPortValid = !string.IsNullOrEmpty(SelectedPortInput.CurrentValue);
        var isStopBitsValid = StopBitsInput.CurrentValue is not null;
        var isParityValid = ParityInput.CurrentValue is not null;
        var isValid =
            isWriteBufferValid
            && isDataBitsValid
            && isWriteTimeoutValid
            && isBoundRateValid
            && isTitleValid
            && isPortValid
            && isStopBitsValid
            && isParityValid;

        if (isValid)
        {
            WriteBufferSize.Value = writeBuffer;
            DataBits.Value = dataBits;
            WriteTimeOut.Value = writeTimeout;
        }

        IsValid.Value = isValid;
    }

    public NotifyCollectionChangedSynchronizedViewList<string> Ports { get; set; }

    public BindableReactiveProperty<Array> BoundRates { get; } =
        new(new[] { 9600, 14400, 19200, 38400, 56000, 57600, 115200, 128000, 256000 });

    public ReactiveProperty<bool> IsValid { get; set; } = new(false);
    public BindableReactiveProperty<string> Title { get; set; }
    public BindableReactiveProperty<string> SelectedBoundRateInput { get; set; } = new();
    public BindableReactiveProperty<string> SelectedPortInput { get; set; } = new();
    public BindableReactiveProperty<Array> ParityValues => new(Enum.GetValues<Parity>());
    public BindableReactiveProperty<Parity?> ParityInput { get; set; } = new();
    public BindableReactiveProperty<string> WriteTimeOutInput { get; set; } =
        new(WriteTimeoutConst.ToString());
    private ReactiveProperty<int> WriteTimeOut { get; set; } = new(WriteTimeoutConst);
    public BindableReactiveProperty<string> WriteBufferSizeInput { get; set; } =
        new(WriteBufferSizeConst.ToString());
    private ReactiveProperty<int> WriteBufferSize { get; set; } = new(WriteBufferSizeConst);
    public BindableReactiveProperty<string> DataBitsInput { get; set; } =
        new(DataBitsConst.ToString());
    private ReactiveProperty<int> DataBits { get; set; } = new(DataBitsConst);
    public BindableReactiveProperty<StopBits?> StopBitsInput { get; set; } = new();

    public BindableReactiveProperty<Array> StopBitsArr => new(Enum.GetValues<StopBits>());
}
