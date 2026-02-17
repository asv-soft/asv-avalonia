using System.Buffers;
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
    private readonly IUnit _frequencyUnit;

    private readonly IncrementalRateCounter _rxBytes;
    private readonly IncrementalRateCounter _txBytes;
    private readonly IncrementalRateCounter _rxPackets;
    private readonly IncrementalRateCounter _txPackets;

    public ConnectionRateStatusViewModel()
        : this(
            DesignTime.UnitService,
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

    public ConnectionRateStatusViewModel(
        IDeviceManager deviceManager,
        IUnitService unitService,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider,
        INavigationService nav
    )
        : this(unitService, loggerFactory, timeProvider, nav)
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
        IUnitService unitService,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider,
        INavigationService nav
    )
        : base(NavId, loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _timeProvider = timeProvider;
        _nav = nav;
        _frequencyUnit =
            unitService.Units[FrequencyUnit.Id]
            ?? throw new UnitException($"Unit {FrequencyUnit.Id} was not found");
        _rxBytes = new IncrementalRateCounter(5, timeProvider);
        _txBytes = new IncrementalRateCounter(5, timeProvider);
        _rxPackets = new IncrementalRateCounter(5, timeProvider);
        _txPackets = new IncrementalRateCounter(5, timeProvider);
    }

    [field: AllowNull]
    [field: MaybeNull]
    public StatisticViewModel FullStatistic
    {
        get
        {
            return field ??= new StatisticViewModel(
                $"{NavId}.statistic",
                _loggerFactory,
                _timeProvider
            );
        }
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

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
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

    private void UpdateStatistic(IStatistic stat)
    {
        var rxBytes = DataFormatter.ByteRate.Print(_rxBytes.Calculate(stat.RxBytes));
        var txBytes = DataFormatter.ByteRate.Print(_txBytes.Calculate(stat.TxBytes));
        var rxPackets = _rxPackets.Calculate(stat.RxMessages).ToString("F1");
        var txPackets = _txPackets.Calculate(stat.TxMessages).ToString("F1");
        var gzUnitSymbol = _frequencyUnit.AvailableUnits[FrequencyHertzUnitItem.Id].Symbol;
        TotalRateInString = $"{rxBytes} / {rxPackets} {gzUnitSymbol}";
        TotalRateOutString = $"{txBytes} / {txPackets} {gzUnitSymbol}";

        if (IsFlyoutOpen)
        {
            FullStatistic.Update(stat);
        }
    }
}
