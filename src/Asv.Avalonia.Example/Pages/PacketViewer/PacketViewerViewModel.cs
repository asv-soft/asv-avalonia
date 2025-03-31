using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Example.Converters;
using Asv.Avalonia.Example.PacketViewer.Dialogs;
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
    public const string PageId = "packet_viewer";
    public const MaterialIconKind PageIcon = MaterialIconKind.Package;
    private readonly ILoggerFactory _loggerFactory;
    private readonly INavigationService _navigationService;
    private readonly ILogger<PacketViewerViewModel> _logger;
    private readonly IAppPath _app;
    private readonly IUnitService _unit;
    private readonly IDeviceManager _deviceManager;
    private readonly IEnumerable<IPacketConverter> _converters;
    private readonly IDialogService _dialogService;
    private readonly CompositeDisposable _disposables = new();
    public const int MaxPacketSize = 1000;

    private readonly ObservableList<PacketMessageViewModel> _packetsList = new();
    private readonly ObservableList<PacketFilterViewModel> _filtersBySourceList = new();
    private readonly ObservableList<PacketFilterViewModel> _filtersByTypeList = new();
    private NotifyCollectionChangedSynchronizedViewList<PacketMessageViewModel> _filteredPackets;

    public BindableReactiveProperty<bool> IsPause { get; } = new(false);
    public BindableReactiveProperty<string> SearchText { get; } = new(string.Empty);
    public BindableReactiveProperty<PacketMessageViewModel?> SelectedPacket { get; } = new();

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
            null!,
            NullNavigationService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        Title.OnNext(RS.PacketViewerViewDockPanelText);
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
        Title.OnNext(RS.PacketViewerViewDockPanelText);
        _app = app;
        _logger = logFactory.CreateLogger<PacketViewerViewModel>();
        _loggerFactory = logFactory;
        _unit = unit;
        _converters = converters;
        _deviceManager = deviceManager;
        _dialogService = service;
        _navigationService = navigationService;

        ExportToCsv = new ReactiveCommand(ExportToCsvAsync);
        ClearAll = new ReactiveCommand(ClearAllAsync);

        _deviceManager
            .Router.OnRxMessage.Where(_ => !IsPause.Value)
            .ThrottleFirst(TimeSpan.FromMilliseconds(100))
            .Select(packet => ConvertPacketsAndUpdateFiltersAsync((MavlinkMessage)packet))
            .Subscribe(async packets =>
            {
                _logger.LogInformation($"Received {packets.Result.Count} packets");
                if (_packetsList.Count + packets.Result.Count > MaxPacketSize)
                {
                    var toRemove = _packetsList.Count + packets.Result.Count - MaxPacketSize;
                    _packetsList.RemoveRange(0, toRemove);
                }

                _packetsList.AddRange(packets.Result);
            })
            .AddTo(_disposables);

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
            .AddTo(_disposables);

        SelectedPacket
            .WhereNotNull()
            .Subscribe(selectedPacket =>
            {
                foreach (var item in _packetsList)
                {
                    item.Highlight = item.Type == selectedPacket.Type;
                }
            })
            .AddTo(_disposables);

        _filteredPackets = _packetsList.CreateView(x => x).ToNotifyCollectionChanged();

        SetupFilteringAsync().GetAwaiter().GetResult();

        _filtersBySourceList
            .ObserveChanged()
            .Subscribe(_ =>
                _logger.LogInformation(
                    $"FiltersBySource updated, count: {_filtersBySourceList.Count}"
                )
            )
            .AddTo(_disposables);
        _filtersByTypeList
            .ObserveChanged()
            .Subscribe(_ =>
                _logger.LogInformation($"FiltersByType updated, count: {_filtersByTypeList.Count}")
            )
            .AddTo(_disposables);
    }

    private Task SetupFilteringAsync()
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
            .AddTo(_disposables);

        return Task.CompletedTask;
    }

    #region Commands

    public ReactiveCommand ExportToCsv { get; }

    public async ValueTask ExportToCsvAsync(Unit unit, CancellationToken cancellationToken) { }

    public ReactiveCommand ClearAll { get; }

    public ValueTask ClearAllAsync(Unit unit, CancellationToken cancellationToken)
    {
        _packetsList.Clear();
        _filtersBySourceList.Clear();
        _filtersByTypeList.Clear();
        return ValueTask.CompletedTask;
    }
    #endregion

    private async ValueTask<IList<PacketMessageViewModel>> ConvertPacketsAndUpdateFiltersAsync(
        MavlinkMessage packet
    )
    {
        var result = new List<PacketMessageViewModel>();
        var converter =
            _converters.FirstOrDefault(_ => _.CanConvert(packet)) ?? new DefaultPacketConverter();
        var vm = await Dispatcher.UIThread.InvokeAsync(
            () => new PacketMessageViewModel(packet, converter)
        );
        result.Add(vm);
        await UpdateFiltersAsync(vm);
        return result;
    }

    private async ValueTask UpdateFiltersAsync(PacketMessageViewModel vm)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
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
                newSourceFilter.IsChecked.Subscribe(_ => { }).AddTo(_disposables);
                _filtersBySourceList.Add(newSourceFilter);
                _logger.LogInformation($"Added new source filter: {vm.Source}");
            }

            var typeFilter = _filtersByTypeList.FirstOrDefault(x => x.Type.Value == vm.Type);
            if (typeFilter != null)
            {
                typeFilter.UpdateRates();
            }
            else
            {
                var newTypeFilter = new PacketFilterViewModel(vm, _unit);
                newTypeFilter.IsChecked.Subscribe(_ => { }).AddTo(_disposables);
                _filtersByTypeList.Add(newTypeFilter);
                _logger.LogInformation($"Added new type filter: {vm.Type}");
            }
        });
    }

    public override IEnumerable<IRoutable> GetRoutableChildren() => [];

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}
