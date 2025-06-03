using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Example.Converters;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example.PacketViewer;

[ExportPage(PageId)]
public class PacketViewerViewModel : PageViewModel<PacketViewerViewModel>
{
    public const string PageId = "packet-viewer";
    public const int MaxPacketsAmount = 1000;

    public const MaterialIconKind PageIcon = MaterialIconKind.Package;
    private readonly ILogger<PacketViewerViewModel> _logger;
    private readonly IAppPath _app;
    private readonly IUnitService _unit;
    private readonly IDeviceManager _deviceManager;
    private readonly IEnumerable<IPacketConverter> _converters;

    private readonly ObservableFixedSizeRingBuffer<PacketMessageViewModel> _packetsBuffer;
    private readonly ObservableHashSet<PacketFilterViewModel> _filtersBySourceSet;
    private readonly ObservableHashSet<PacketFilterViewModel> _filtersByTypeSet;

    public BindableReactiveProperty<bool> IsPause { get; }
    public BindableReactiveProperty<string> SearchText { get; }
    public BindableReactiveProperty<bool> IsCheckedAllSources { get; }
    public BindableReactiveProperty<bool> IsCheckedAllTypes { get; }
    public BindableReactiveProperty<PacketMessageViewModel?> SelectedPacket { get; }
    public INotifyCollectionChangedSynchronizedViewList<PacketMessageViewModel> Packets { get; }
    public INotifyCollectionChangedSynchronizedViewList<PacketFilterViewModel> FiltersBySource { get; }
    public INotifyCollectionChangedSynchronizedViewList<PacketFilterViewModel> FiltersByType { get; }

    public PacketViewerViewModel()
        : this(
            DesignTime.CommandService,
            NullAppPath.Instance,
            NullLoggerFactory.Instance,
            NullUnitService.Instance,
            null!,
            NullDeviceManager.Instance,
            NullDialogService.Instance,
            NullNavigationService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        _disposables = new CompositeDisposable();
        _packetsBuffer.AddLastRange(
            new[]
            {
                new PacketMessageViewModel
                {
                    DateTime = DateTime.Now,
                    Source = "[1,1]",
                    Type = "HEARTBEAT",
                    Message = "Test message 1",
                },
                new PacketMessageViewModel
                {
                    DateTime = DateTime.Now,
                    Source = "[1,1]",
                    Type = "HEARTBEAT",
                    Message = "Test message 2",
                },
                new PacketMessageViewModel
                {
                    DateTime = DateTime.Now,
                    Source = "[1,1]",
                    Type = "HEARTBEAT",
                    Message = "Test message 3",
                },
            }
        );
    }

    [ImportingConstructor]
    public PacketViewerViewModel(
        ICommandService cmd,
        IAppPath app,
        ILoggerFactory logFactory,
        IUnitService unit,
        [ImportMany] IEnumerable<IPacketConverter> converters,
        IDeviceManager deviceManager,
        IDialogService service,
        INavigationService navigationService
    )
        : base(PageId, cmd)
    {
        Title = "Packet Viewer";
        _app = app;
        _logger = logFactory.CreateLogger<PacketViewerViewModel>();
        _unit = unit;
        _converters = converters;
        _deviceManager = deviceManager;
        _disposables = new CompositeDisposable();

        _packetsBuffer = new ObservableFixedSizeRingBuffer<PacketMessageViewModel>(
            MaxPacketsAmount
        );
        _filtersBySourceSet = new ObservableHashSet<PacketFilterViewModel>(
            PacketFilterViewModelComparer.Instance
        );
        _filtersByTypeSet = new ObservableHashSet<PacketFilterViewModel>(
            PacketFilterViewModelComparer.Instance
        );
        _packetsBuffer.SetRoutableParent(this).DisposeItWith(Disposable);
        _filtersBySourceSet.SetRoutableParent(this).DisposeItWith(Disposable);
        _filtersByTypeSet.SetRoutableParent(this).DisposeItWith(Disposable);
        _packetsBuffer.DisposeMany().DisposeItWith(Disposable);
        _filtersBySourceSet.DisposeMany().DisposeItWith(Disposable);
        _filtersByTypeSet.DisposeMany().DisposeItWith(Disposable);

        var packetsView = _packetsBuffer.CreateView(x => x).DisposeItWith(Disposable);
        Packets = packetsView.ToNotifyCollectionChanged().DisposeItWith(Disposable);
        FiltersBySource = _filtersBySourceSet.ToNotifyCollectionChanged().DisposeItWith(Disposable);
        FiltersByType = _filtersByTypeSet.ToNotifyCollectionChanged().DisposeItWith(Disposable);

        IsPause = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        SearchText = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        IsCheckedAllSources = new BindableReactiveProperty<bool>(true).DisposeItWith(Disposable);
        IsCheckedAllTypes = new BindableReactiveProperty<bool>(true).DisposeItWith(Disposable);
        SelectedPacket = new BindableReactiveProperty<PacketMessageViewModel?>().DisposeItWith(
            Disposable
        );

        // ExportToCsv = new ReactiveCommand(ExportToCsvAsync);
        ClearAll = new ReactiveCommand(ClearAllImpl).DisposeItWith(Disposable);
        IsCheckedAllSources
            .Subscribe(isChecked =>
            {
                foreach (var packet in _filtersBySourceSet)
                {
                    packet.IsChecked.Value = isChecked;
                }
            })
            .DisposeItWith(Disposable);
        IsCheckedAllTypes
            .Subscribe(isChecked =>
            {
                foreach (var packet in _filtersByTypeSet)
                {
                    packet.IsChecked.Value = isChecked;
                }
            })
            .DisposeItWith(Disposable);
        _packetsBuffer
            .ObserveAdd()
            .ThrottleFirst(TimeSpan.FromMilliseconds(100))
            .Subscribe(item => UpdateFilters(item.Value))
            .DisposeItWith(Disposable);

        _deviceManager
            .Router.OnRxMessage.Where(_ => !IsPause.Value)
            .ThrottleLast(TimeSpan.FromMilliseconds(300))
            .FilterByType<MavlinkMessage>()
            .Select(ConvertToPacketMessage)
            .SubscribeAwait(
                async (packets, cancel) =>
                {
                    await Task.Run(
                        () =>
                        {
                            foreach (var item in packets)
                            {
                                _packetsBuffer.AddLast(item);
                            }

                            return Task.CompletedTask;
                        },
                        cancel
                    );
                }
            )
            .DisposeItWith(Disposable);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                foreach (var filter in _filtersBySourceSet)
                {
                    filter.UpdateRateText();
                }

                foreach (var filter in _filtersByTypeSet)
                {
                    filter.UpdateRateText();
                }
            })
            .DisposeItWith(Disposable);

        SelectedPacket
            .WhereNotNull()
            .Subscribe(selectedPacket =>
            {
                foreach (var item in _packetsBuffer)
                {
                    item.Highlight = item.Type == selectedPacket.Type;
                }
            })
            .DisposeItWith(Disposable);

        Observable
            .Merge(
                _packetsBuffer.ObserveChanged().Select(_ => Unit.Default),
                _filtersBySourceSet.ObserveChanged().Select(_ => Unit.Default),
                _filtersByTypeSet.ObserveChanged().Select(_ => Unit.Default),
                SearchText.Select(_ => Unit.Default),
                SelectedPacket.Select(_ => Unit.Default)
            )
            .ThrottleFirst(TimeSpan.FromMilliseconds(100))
            .Subscribe(_ =>
            {
                var isSourcesChecked = _filtersBySourceSet.Any(item => item.IsChecked.Value);
                var isTypesChecked = _filtersByTypeSet.Any(item => item.IsChecked.Value);
                var isSearchMatch = !string.IsNullOrEmpty(SearchText.Value);

                if (!isSourcesChecked && !isTypesChecked && !isSearchMatch)
                {
                    packetsView.ResetFilter();
                    return;
                }

                packetsView.AttachFilter(packet =>
                {
                    return packet.Match(p =>
                            p.Message.Contains(SearchText.Value, StringComparison.OrdinalIgnoreCase)
                        )
                        && packet.Match(p =>
                            _filtersByTypeSet.Any(f => f.IsChecked.Value && f.Type.Value == p.Type)
                        )
                        && packet.Match(p =>
                            _filtersBySourceSet.Any(f =>
                                f.IsChecked.Value && f.Source.Value == p.Source
                            )
                        );
                });
            })
            .DisposeItWith(Disposable);
    }

    #region Commands

    public ReactiveCommand ExportToCsv { get; }

    private async Task ExportToCsvAsync(IProgress<double> progress, CancellationToken cancel)
    {
        // try
        // {
        //     var separator = ";";
        //     var shieldSymbol = ",";
        //
        //     if (IsComa.Value)
        //     {
        //         separator = ",";
        //         shieldSymbol = ";";
        //     }
        //     else if (IsTab.Value)
        //     {
        //         separator = "\t";
        //         shieldSymbol = ",";
        //     }
        //
        //     var fullPath = FilePath.Value;
        //
        //     CsvHelper.SaveToCsv(
        //         _packetsList,
        //         fullPath,
        //         separator,
        //         shieldSymbol,
        //         new CsvColumn<PacketMessageViewModel>("Date", x => x.DateTime.ToString("G")),
        //         new CsvColumn<PacketMessageViewModel>("Type", x => x.Type),
        //         new CsvColumn<PacketMessageViewModel>("Source", x => x.Source),
        //         new CsvColumn<PacketMessageViewModel>("Message", x => x.Message)
        //     );
        //
        //     _logger.LogInformation("Файл сохранен по пути: {0}", fullPath);
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Ошибка при сохранении файла по пути: {0}", FilePath.Value);
        // }
    }

    public ReactiveCommand ClearAll { get; }

    private ValueTask ClearAllImpl(Unit unit, CancellationToken cancellationToken)
    {
        _packetsBuffer.RemoveAll();
        _filtersBySourceSet.RemoveAll();
        _filtersByTypeSet.RemoveAll();
        return ValueTask.CompletedTask;
    }

    #endregion

    private IEnumerable<PacketMessageViewModel> ConvertToPacketMessage(MavlinkMessage packet)
    {
        var converter =
            _converters.FirstOrDefault(_ => _.CanConvert(packet)) ?? new DefaultPacketConverter();
        var vm = new PacketMessageViewModel(packet, converter);
        _disposables.Add(vm);
        yield return vm;
    }

    private void UpdateFilters(PacketMessageViewModel vm)
    {
        UpdateSourceFilters(vm);
        UpdateTypeFilters(vm);
    }

    private void UpdateSourceFilters(PacketMessageViewModel vm)
    {
        var sourceFilter = _filtersBySourceSet.FirstOrDefault(x => x.Source.Value == vm.Source);
        if (sourceFilter is null)
        {
            var newSourceFilter = new PacketFilterViewModel(vm, _unit);
            _disposables.Add(newSourceFilter);
            _filtersBySourceSet.Add(newSourceFilter);
            _logger.LogInformation("Added new source filter: {Source}", vm.Source);
            return;
        }

        sourceFilter.UpdateRates();
    }

    private void UpdateTypeFilters(PacketMessageViewModel vm)
    {
        var typeFilter = _filtersByTypeSet.FirstOrDefault(x => x.Type.Value == vm.Type);
        if (typeFilter is null)
        {
            var newTypeFilter = new PacketFilterViewModel(vm, _unit);
            _disposables.Add(newTypeFilter);
            _filtersByTypeSet.Add(newTypeFilter);
            _logger.LogInformation("Added new type filter: {Type}", vm.Type);
            return;
        }

        typeFilter.UpdateRates();
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var item in _packetsBuffer)
        {
            yield return item;
        }

        foreach (var item in _filtersBySourceSet)
        {
            yield return item;
        }

        foreach (var item in _filtersByTypeSet)
        {
            yield return item;
        }
    }

    protected override void AfterLoadExtensions() { }

    #region Dispose

    private readonly CompositeDisposable _disposables;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _packetsBuffer.RemoveAll();
            _filtersBySourceSet.RemoveAll();
            _filtersByTypeSet.RemoveAll();
            _disposables.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion

    public override IExportInfo Source => SystemModule.Instance;
}

file class PacketFilterViewModelComparer : IEqualityComparer<PacketFilterViewModel>
{
    public static PacketFilterViewModelComparer Instance { get; } = new();

    public bool Equals(PacketFilterViewModel? x, PacketFilterViewModel? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null)
        {
            return false;
        }

        if (y is null)
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Id.Equals(y.Id)
            && x.Type.Value.Equals(y.Type.Value)
            && x.Source.Value.Equals(y.Source.Value)
            && x.MessageRateText.Value.Equals(y.MessageRateText.Value);
    }

    public int GetHashCode(PacketFilterViewModel obj)
    {
        return HashCode.Combine(
            obj.Id,
            obj.Type.Value,
            obj.Source.Value,
            obj.MessageRateText.Value
        );
    }
}
