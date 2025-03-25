using System.Composition;
using System.IO.Ports;
using System.Text;
using System.Windows.Input;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Primitives;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

[Export(SerialProtocolPort.Scheme, typeof(IPortViewModel))]
public class SerialPortViewModel : PortViewModel
{
    private string _id;
    private readonly ObservableList<string> _portSource;
    public const string SerialId = "serial-port";

    public SerialPortViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        UpdateTags(SerialProtocolPortConfig.CreateDefault());
    }

    [ImportingConstructor]
    public SerialPortViewModel(IDeviceManager deviceManager)
        : base(SerialId)
    {
        Icon = MaterialIconKind.SerialPort;
        SaveChangesCommand = new ReactiveCommand(SaveChanges).DisposeItWith(Disposable);
        PortName = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        _portSource = [];
        PortNames = _portSource.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        TimeProvider
            .System.CreateTimer(UpdateNames, null, TimeSpan.Zero, TimeSpan.FromSeconds(3))
            .DisposeItWith(Disposable);
    }

    private void UpdateNames(object? state)
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

    private ValueTask SaveChanges(Unit arg1, CancellationToken arg2)
    {
        var config = SerialProtocolPortConfig.CreateDefault();
        config.Name = Name.Value;
        config.PortName = PortName.Value;
        return this.ExecuteCommand(
            ProtocolPortCommand.StaticInfo.Id,
            new CommandParameterAction(
                _id,
                config.AsUri().ToString(),
                CommandParameterActionType.Change
            )
        );
    }

    public ICommand SaveChangesCommand { get; }

    public override void Init(IProtocolPort protocolPort)
    {
        _id = protocolPort.Id;
        var serialPort = (SerialProtocolPort)protocolPort;
        var config = (SerialProtocolPortConfig)serialPort.Config;
        PortName.Value = config.PortName;
        UpdateTags(config);
        base.Init(protocolPort);
    }

    private void UpdateTags(SerialProtocolPortConfig config)
    {
        TagsSource.Add(
            new TagViewModel(nameof(config.Scheme))
            {
                Key = "Type",
                Value = "Serial",
                TagType = TagType.Success,
            }
        );
        TagsSource.Add(
            new TagViewModel(nameof(config.PortName))
            {
                Key = "Path",
                Value = config.PortName,
                TagType = TagType.Info,
            }
        );
        TagsSource.Add(
            new TagViewModel(nameof(config.Parity))
            {
                Key = "Options",
                Value = GetOptions(config),
                TagType = TagType.Info2,
            }
        );
    }

    private static string GetOptions(SerialProtocolPortConfig config)
    {
        var sb = new StringBuilder();
        sb.Append(config.BoundRate).Append(' ').Append(config.DataBits);
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
