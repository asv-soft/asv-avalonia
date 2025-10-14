using System.Buffers;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Asv.Common;
using Asv.IO;
using DotNext.Buffers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.IO;

[ExportStatusItem]
public class ConnectionRateStatusViewModel : StatusItem
{
    public const string NavId = $"{DefaultId}.connection_rate";

    private readonly ILoggerFactory _loggerFactory;
    private readonly TimeProvider _timeProvider;
    private readonly INavigationService _nav;

    private readonly IncrementalRateCounter _rxBytes;
    private readonly IncrementalRateCounter _txBytes;
    private readonly IncrementalRateCounter _rxPackets;
    private readonly IncrementalRateCounter _txPackets;

    public ConnectionRateStatusViewModel()
        : this(
            NullLayoutService.Instance,
            NullLoggerFactory.Instance,
            TimeProvider.System,
            DesignTime.Navigation
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
                UpdateStatistic(stat);
            });
    }

    [ImportingConstructor]
    public ConnectionRateStatusViewModel(
        IDeviceManager deviceManager,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider,
        INavigationService nav
    )
        : this(layoutService, loggerFactory, timeProvider, nav)
    {
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ => UpdateStatistic(deviceManager.Router.Statistic))
            .DisposeItWith(Disposable);
        this.ObservePropertyChanged(x => x.IsFlyoutOpen)
            .Subscribe(_ => UpdateStatistic(deviceManager.Router.Statistic))
            .DisposeItWith(Disposable);
    }

    private ConnectionRateStatusViewModel(
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider,
        INavigationService nav
    )
        : base(NavId, layoutService, loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _timeProvider = timeProvider;
        _nav = nav;
        _rxBytes = new IncrementalRateCounter(5, timeProvider);
        _txBytes = new IncrementalRateCounter(5, timeProvider);
        _rxPackets = new IncrementalRateCounter(5, timeProvider);
        _txPackets = new IncrementalRateCounter(5, timeProvider);
    }

    private void UpdateStatistic(IStatistic stat)
    {
        var rxBytes = DataFormatter.ByteRate.Print(_rxBytes.Calculate(stat.RxBytes));
        var txBytes = DataFormatter.ByteRate.Print(_txBytes.Calculate(stat.TxBytes));
        var rxPackets = _rxPackets.Calculate(stat.RxMessages).ToString("F1");
        var txPackets = _txPackets.Calculate(stat.TxMessages).ToString("F1");
        TotalRateInString = $"{rxBytes} / {rxPackets} Hz";
        TotalRateOutString = $"{txBytes} / {txPackets} Hz";

        if (IsFlyoutOpen)
        {
            FullStatistic.Update(stat);
        }
    }

    [field: AllowNull, MaybeNull]
    public StatisticViewModel FullStatistic
    {
        get
        {
            return field ??= new StatisticViewModel(
                $"{NavId}.statistic",
                LayoutService,
                _loggerFactory,
                _timeProvider
            );
        }
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override int Order => 256;

    public string TotalRateInString
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string TotalRateOutString
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public bool IsFlyoutOpen
    {
        get;
        set => SetField(ref field, value);
    }

    public void NavigateToSettings()
    {
        _nav.GoTo(
                new NavigationPath(
                    SettingsPageViewModel.PageId,
                    SettingsConnectionViewModel.SubPageId
                )
            )
            .SafeFireAndForget();
    }
}
