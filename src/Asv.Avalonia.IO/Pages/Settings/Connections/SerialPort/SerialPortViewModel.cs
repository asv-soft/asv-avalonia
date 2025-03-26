using System.Composition;
using System.IO.Ports;
using System.Text;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

[Export(SerialProtocolPort.Scheme, typeof(IPortViewModel))]
public class SerialPortViewModel : PortViewModel
{
    private readonly ObservableList<string> _portSource;
    public const MaterialIconKind DefaultIcon = MaterialIconKind.SerialPort;

    public SerialPortViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        UpdateTags(SerialProtocolPortConfig.CreateDefault());
    }

    [ImportingConstructor]
    public SerialPortViewModel(ILoggerFactory loggerFactory)
        : base($"{SerialProtocolPort.Scheme}-editor")
    {
        Icon = DefaultIcon;

        AddToValidation(PortName = new BindableReactiveProperty<string?>(), ValidatePortName);
        _portSource = [];
        PortNames = _portSource
            .ToNotifyCollectionChangedSlim(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);
        TimeProvider
            .System.CreateTimer(UpdatePortNames, null, TimeSpan.Zero, TimeSpan.FromSeconds(3))
            .DisposeItWith(Disposable);

        AddToValidation(BaudRate = new BindableReactiveProperty<string?>(), ValidateBaudRate);
    }

    private static Exception? ValidatePortName(string? arg) => null;

    private static Exception? ValidateBaudRate(string? arg)
    {
        if (arg == null)
        {
            return new Exception("Baud rate is required");
        }
        if (int.TryParse(arg, out int baudRate) == false)
        {
            return new Exception("Invalid baud rate: must be a number");
        }
        if (baudRate < 0)
        {
            return new Exception("Invalid baud rate: must be a positive number");
        }

        return null;
    }

    private void UpdatePortNames(object? state)
    {
        _portSource.SyncCollection(
            SerialPort.GetPortNames(),
            x => _portSource.Remove(x),
            x => _portSource.Add(x),
            StringComparer.InvariantCultureIgnoreCase
        );
    }

    public NotifyCollectionChangedSynchronizedViewList<string> PortNames { get; set; }
    public BindableReactiveProperty<string?> PortName { get; }
    public BindableReactiveProperty<string?> BaudRate { get; }

    public IEnumerable<string> BaudRates { get; } =
        ["9600", "19200", "38400", "57600", "115200", "230400", "460800", "921600"];

    protected override void InternalLoadChanges(ProtocolPortConfig config)
    {
        base.InternalLoadChanges(config);
        if (config is SerialProtocolPortConfig serialConfig)
        {
            PortName.Value = serialConfig.PortName;
            BaudRate.Value = serialConfig.BoundRate.ToString();
            UpdateTags(serialConfig);
        }
    }

    protected override void InternalSaveChanges(ProtocolPortConfig config)
    {
        base.InternalSaveChanges(config);
        if (config is SerialProtocolPortConfig serialConfig)
        {
            serialConfig.PortName = PortName.Value;
            if (BaudRate.Value != null)
            {
                serialConfig.BoundRate = int.Parse(BaudRate.Value);
            }
        }
    }

    private void UpdateTags(SerialProtocolPortConfig config)
    {
        TagsSource.Clear();
        TagsSource.Add(
            new TagViewModel(nameof(config.Scheme)) { Value = "SERIAL", TagType = TagType.Info }
        );
        TagsSource.Add(
            new TagViewModel(nameof(config.PortName))
            {
                Value = GetOptions(config),
                TagType = TagType.Success,
            }
        );
    }

    private static string GetOptions(SerialProtocolPortConfig config)
    {
        var sb = new StringBuilder();
        sb.Append(config.PortName)
            .Append(' ')
            .Append(config.BoundRate)
            .Append(' ')
            .Append(config.DataBits);
        switch (config.Parity)
        {
            case Parity.None:
                sb.Append("N");
                break;
            case Parity.Odd:
                sb.Append("O");
                break;
            case Parity.Even:
                sb.Append("E");
                break;
            case Parity.Mark:
                sb.Append("M");
                break;
            case Parity.Space:
                sb.Append("S");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (config.StopBits)
        {
            case StopBits.None:
                sb.Append("0");
                break;
            case StopBits.One:
                sb.Append("1");
                break;
            case StopBits.Two:
                sb.Append("2");
                break;
            case StopBits.OnePointFive:
                sb.Append("1.5");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return sb.ToString();
    }
}
