using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia.IO;

public class EndpointViewModel : HeadlinedViewModel
{
    private readonly IProtocolEndpoint? _protocolEndpoint;
    private readonly IncrementalRateCounter _rxBytes;
    private readonly IncrementalRateCounter _txBytes;
    private readonly IncrementalRateCounter _rxPackets;
    private readonly IncrementalRateCounter _txPackets;

    private EndpointViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : base(id, loggerFactory)
    {
        _rxBytes = new IncrementalRateCounter(5, timeProvider);
        _txBytes = new IncrementalRateCounter(5, timeProvider);
        _rxPackets = new IncrementalRateCounter(5, timeProvider);
        _txPackets = new IncrementalRateCounter(5, timeProvider);
        Icon = MaterialIconKind.SwapVertical;
        TagsView = TagsSource
            .ToNotifyCollectionChangedSlim(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);
        TagsSource.Add(
            RxTag = new TagViewModel("rx", loggerFactory)
            {
                Icon = MaterialIconKind.ArrowDownBold,
                TagType = TagType.Success,
                Value = DataFormatter.ByteRate.Print(double.NaN),
            }
        );
        TagsSource.Add(
            TxTag = new TagViewModel("tx", loggerFactory)
            {
                Icon = MaterialIconKind.ArrowUpBold,
                TagType = TagType.Success,
                Value = DataFormatter.ByteRate.Print(double.NaN),
            }
        );
    }

    internal EndpointViewModel()
        : this(DesignTime.Id, DesignTime.LoggerFactory, TimeProvider.System)
    {
        DesignTime.ThrowIfNotDesignMode();
        Header = "127.0.0.1:7574";
    }

    public EndpointViewModel(
        IProtocolEndpoint protocolEndpoint,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : this(protocolEndpoint.Id, loggerFactory, timeProvider)
    {
        _protocolEndpoint = protocolEndpoint;
        Header = protocolEndpoint.Id;
    }

    public void UpdateStatistic()
    {
        var rxBytes = DataFormatter.ByteRate.Print(
            _rxBytes.Calculate(_protocolEndpoint?.Statistic.RxBytes ?? 0)
        );
        var txBytes = DataFormatter.ByteRate.Print(
            _txBytes.Calculate(_protocolEndpoint?.Statistic.TxBytes ?? 0)
        );
        var rxPackets = _rxPackets
            .Calculate(_protocolEndpoint?.Statistic.RxMessages ?? 0)
            .ToString("F1");
        var txPackets = _txPackets
            .Calculate(_protocolEndpoint?.Statistic.TxMessages ?? 0)
            .ToString("F1");
        RxTag.Value = $"{rxBytes} / {rxPackets} Hz";
        TxTag.Value = $"{txBytes} / {txPackets} Hz";
    }

    public TagViewModel RxTag { get; }
    public TagViewModel TxTag { get; }

    protected ObservableList<TagViewModel> TagsSource { get; } = [];

    public NotifyCollectionChangedSynchronizedViewList<TagViewModel> TagsView { get; }
}
