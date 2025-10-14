using System.Reactive.Linq;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.IO;

public class StatisticViewModel : RoutableViewModel
{
    private readonly IncrementalRateCounter _rxBytes;
    private readonly IncrementalRateCounter _txBytes;
    private readonly IncrementalRateCounter _rxPackets;
    private readonly IncrementalRateCounter _txPackets;

    public StatisticViewModel()
        : this(
            DesignTime.Id,
            NullLayoutService.Instance,
            DesignTime.LoggerFactory,
            TimeProvider.System
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        var stat = new Statistic();

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                stat.AddParserBytes(Random.Shared.Next(0, 1_000_000));
                stat.AddRxBytes(Random.Shared.Next(0, 1_000_000));
                stat.AddTxBytes(Random.Shared.Next(0, 1_000_000));
                for (int i = 0; i < Random.Shared.Next(0, 100); i++)
                {
                    stat.IncrementRxMessage();
                }
                for (int i = 0; i < Random.Shared.Next(0, 100); i++)
                {
                    stat.IncrementTxMessage();
                }
                Update(stat);
            });
    }

    public StatisticViewModel(
        NavigationId id,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : base(id, layoutService, loggerFactory)
    {
        _rxBytes = new IncrementalRateCounter(5, timeProvider);
        _txBytes = new IncrementalRateCounter(5, timeProvider);
        _rxPackets = new IncrementalRateCounter(5, timeProvider);
        _txPackets = new IncrementalRateCounter(5, timeProvider);
    }

    public string Rx
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string RxRate
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;
    public string RxMessages
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string RxMessagesRate
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public void Update(IStatistic statistic)
    {
        Rx = DataFormatter.DataSize.Print(statistic.RxBytes);
        RxRate = DataFormatter.ByteRate.Print(_rxBytes.Calculate(statistic.RxBytes));
        RxMessages = statistic.RxMessages.ToString("N0");
        RxMessagesRate = _rxPackets.Calculate(statistic.RxMessages).ToString("N1");
        RxDropped = statistic.DroppedRxMessages.ToString("N0");
        RxErrors = statistic.RxError.ToString("N0");

        Tx = DataFormatter.DataSize.Print(statistic.TxBytes);
        TxRate = DataFormatter.ByteRate.Print(_txBytes.Calculate(statistic.TxBytes));
        TxMessages = statistic.TxMessages.ToString("N0");
        TxMessagesRate = _txPackets.Calculate(statistic.TxMessages).ToString("N1");
        TxDropped = statistic.DroppedTxMessages.ToString("N0");
        TxErrors = statistic.TxError.ToString("N0");

        BadCrcError = statistic.BadCrcError.ToString("N0");
        ParsedBytes = DataFormatter.DataSize.Print(statistic.ParsedBytes);
        ParsedPackets = statistic.ParsedMessages.ToString("N0");
        MessagePublishError = statistic.MessagePublishError.ToString("N0");
        DeserializeError = statistic.DeserializeError.ToString("N0");
        UnknownMessage = statistic.UnknownMessages.ToString("N0");
        MessageReadNotAllData = statistic.MessageReadNotAllData.ToString("N0");
    }

    public string MessageReadNotAllData
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string UnknownMessage
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string DeserializeError
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string MessagePublishError
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string ParsedPackets
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string ParsedBytes
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string BadCrcError
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string TxErrors
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string RxErrors
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string TxDropped
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string RxDropped
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string TxMessagesRate
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;
    public string TxMessages
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;
    public string TxRate
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;
    public string Tx
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
