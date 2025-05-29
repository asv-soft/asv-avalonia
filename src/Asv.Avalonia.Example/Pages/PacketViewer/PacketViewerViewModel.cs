using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Example.Converters;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Mavlink;
using Avalonia.Threading;
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
    public const MaterialIconKind PageIcon = MaterialIconKind.Package;
    private readonly ILogger<PacketViewerViewModel> _logger;
    private readonly IAppPath _app;
    private readonly IUnitService _unit;
    private readonly IDeviceManager _deviceManager;
    private readonly IEnumerable<IPacketConverter> _converters;
    public const int MaxPacketSize = 1000;

    private readonly ObservableList<PacketMessageViewModel> _packetsList;
    private readonly ObservableList<PacketFilterViewModel> _filtersBySourceList;
    private readonly ObservableList<PacketFilterViewModel> _filtersByTypeList;
    private NotifyCollectionChangedSynchronizedViewList<PacketMessageViewModel> _filteredPackets;

    public BindableReactiveProperty<bool> IsPause { get; }
    public BindableReactiveProperty<string> SearchText { get; }
    public BindableReactiveProperty<PacketMessageViewModel?> SelectedPacket { get; }

    public INotifyCollectionChangedSynchronizedViewList<PacketMessageViewModel> Packets =>
        _filteredPackets;
    public ObservableList<PacketFilterViewModel> FiltersBySource => _filtersBySourceList;
    public ObservableList<PacketFilterViewModel> FiltersByType => _filtersByTypeList;

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
        _packetsList.AddRange(
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

        _packetsList = [];
        _filtersBySourceList = [];
        _filtersByTypeList = [];
        _packetsList.DisposeMany().DisposeItWith(Disposable);
        _filtersBySourceList.DisposeMany().DisposeItWith(Disposable);
        _filtersByTypeList.DisposeMany().DisposeItWith(Disposable);
        IsPause = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        SearchText = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        SelectedPacket = new BindableReactiveProperty<PacketMessageViewModel?>().DisposeItWith(
            Disposable
        );

        // ExportToCsv = new ReactiveCommand(ExportToCsvAsync);
        ClearAll = new ReactiveCommand(ClearAllAsync);

        _deviceManager
            .Router.OnRxMessage.Where(_ => !IsPause.Value)
            .ThrottleFirst(TimeSpan.FromMilliseconds(100))
            .SelectAwait(
                async (packet, ct) =>
                    await ConvertPacketsAndUpdateFiltersAsync((MavlinkMessage)packet, ct)
            )
            .Subscribe(packets =>
            {
                _logger.LogInformation("Received {ResultCount} packets", packets.Count);
                if (_packetsList.Count + packets.Count > MaxPacketSize)
                {
                    var toRemove = _packetsList.Count + packets.Count - MaxPacketSize;
                    _packetsList.RemoveRange(0, toRemove);
                }

                _packetsList.AddRange(packets);
            })
            .DisposeItWith(Disposable);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    foreach (var filter in _filtersBySourceList)
                    {
                        filter.UpdateRateText();
                    }

                    foreach (var filter in _filtersByTypeList)
                    {
                        filter.UpdateRateText();
                    }
                });
            })
            .DisposeItWith(Disposable);

        SelectedPacket
            .WhereNotNull()
            .Subscribe(selectedPacket =>
            {
                foreach (var item in _packetsList)
                {
                    item.Highlight = item.Type == selectedPacket.Type;
                }
            })
            .DisposeItWith(Disposable);

        _filteredPackets = _packetsList.CreateView(x => x).ToNotifyCollectionChanged();

        SetupFiltering();

        _filtersBySourceList
            .ObserveChanged()
            .Subscribe(_ =>
                _logger.LogInformation(
                    "FiltersBySource updated, count: {Count}",
                    _filtersBySourceList.Count
                )
            )
            .DisposeItWith(Disposable);
        _filtersByTypeList
            .ObserveChanged()
            .Subscribe(_ =>
                _logger.LogInformation(
                    "FiltersByType updated, count: {Count}",
                    _filtersByTypeList.Count
                )
            )
            .DisposeItWith(Disposable);
    }

    private void SetupFiltering()
    {
        var view = _packetsList.CreateView(x => x);
        _filteredPackets = view.ToNotifyCollectionChanged();

        Observable
            .Merge(
                _packetsList.ObserveChanged().Select(_ => Unit.Default),
                _filtersBySourceList.ObserveChanged().Select(_ => Unit.Default),
                _filtersByTypeList.ObserveChanged().Select(_ => Unit.Default),
                SearchText.Select(_ => Unit.Default),
                SelectedPacket.Select(_ => Unit.Default)
            )
            .ThrottleFirst(TimeSpan.FromMilliseconds(100))
            .Subscribe(_ =>
            {
                view.AttachFilter(x =>
                {
                    var sourceMatch =
                        _filtersBySourceList.Count == 0
                        || _filtersBySourceList.Any(f =>
                            f.IsChecked.Value && f.Source.Value == x.Source
                        );
                    var typeMatch =
                        _filtersByTypeList.Count == 0
                        || _filtersByTypeList.Any(f => f.IsChecked.Value && f.Type.Value == x.Type);
                    var searchMatch =
                        string.IsNullOrEmpty(SearchText.Value)
                        || x.Message.Contains(SearchText.Value, StringComparison.OrdinalIgnoreCase);

                    return sourceMatch && typeMatch && searchMatch;
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

    public ValueTask ClearAllAsync(Unit unit, CancellationToken cancellationToken)
    {
        _packetsList.RemoveAll();
        _filtersBySourceList.RemoveAll();
        _filtersByTypeList.RemoveAll();
        return ValueTask.CompletedTask;
    }
    #endregion

    private async ValueTask<IList<PacketMessageViewModel>> ConvertPacketsAndUpdateFiltersAsync(
        MavlinkMessage packet,
        CancellationToken cancel
    )
    {
        var result = new List<PacketMessageViewModel>();
        var converter =
            _converters.FirstOrDefault(_ => _.CanConvert(packet)) ?? new DefaultPacketConverter();
        var vm = await Dispatcher.UIThread.InvokeAsync(
            () => new PacketMessageViewModel(packet, converter)
        );
        result.Add(vm);
        await UpdateFiltersAsync(vm, cancel);
        return result;
    }

    private async ValueTask UpdateFiltersAsync(PacketMessageViewModel vm, CancellationToken cancel)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            cancel.ThrowIfCancellationRequested();
            var sourceFilter = _filtersBySourceList.FirstOrDefault(x =>
                x.Source.Value == vm.Source
            );
            if (sourceFilter != null)
            {
                sourceFilter.UpdateRates();
            }
            else
            {
                var newSourceFilter = new PacketFilterViewModel(vm, _unit);
                using var sub = newSourceFilter.IsChecked.Subscribe(_ => { });
                _filtersBySourceList.Add(newSourceFilter);
                _logger.LogInformation("Added new source filter: {Source}", vm.Source);
            }

            var typeFilter = _filtersByTypeList.FirstOrDefault(x => x.Type.Value == vm.Type);
            if (typeFilter != null)
            {
                typeFilter.UpdateRates();
            }
            else
            {
                var newTypeFilter = new PacketFilterViewModel(vm, _unit);
                using var sub = newTypeFilter.IsChecked.Subscribe(_ => { });
                _filtersByTypeList.Add(newTypeFilter);
                _logger.LogInformation("Added new type filter: {Type}", vm.Type);
            }
        });
    }

    public override IEnumerable<IRoutable> GetRoutableChildren() => [];

    protected override void AfterLoadExtensions() { }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _packetsList.RemoveAll();
            _filtersBySourceList.RemoveAll();
            _filtersByTypeList.RemoveAll();
        }

        base.Dispose(disposing);
    }

    public override IExportInfo Source => SystemModule.Instance;
}
