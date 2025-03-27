using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Example.Converters;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Packaging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example.PacketViewer;

[ExportPage(PageId)]
public class PacketViewerViewModel : PageViewModel<PacketViewerViewModel>
{
    public const string PageId = "packet_viewer";
    public const MaterialIconKind PageIcon = MaterialIconKind.Package;
    private readonly ILogger<PacketViewerViewModel> _logger;
    private readonly IAppPath _app;
    private readonly IUnitService _unit;
    private readonly IDeviceManager _deviceManager;
    
    private readonly IEnumerable<IPacketConverter> _converters;
    private readonly CompositeDisposable _disposables = new();
    public const int MaxPacketSize = 1000;
    
    private readonly ObservableList<PacketMessageViewModel> _packetsList = new();
    private readonly ObservableList<PacketFilterViewModel> _filtersBySourceList = new();
    private readonly ObservableList<PacketFilterViewModel> _filtersByTypeList = new();
    private readonly ObservableCollection<PacketMessageViewModel> _filteredPackets = new(); // Добавляем отфильтрованный список

    public BindableReactiveProperty<bool> IsPause { get; } = new(false);
    public BindableReactiveProperty<string> SearchText { get; } = new(string.Empty);
    public BindableReactiveProperty<PacketMessageViewModel?> SelectedPacket { get; } = new();
    
    // Изменённые свойства для привязки
    public ObservableCollection<PacketMessageViewModel> Packets => _filteredPackets; // Используем отфильтрованный список
    public IReadOnlyList<PacketFilterViewModel> FiltersBySource => _filtersBySourceList;
    public IReadOnlyList<PacketFilterViewModel> FiltersByType => _filtersByTypeList;
    
    public PacketViewerViewModel() 
        : this(DesignTime.CommandService, NullAppPath.Instance, NullLoggerFactory.Instance, NullUnitService.Instance, null!, NullDeviceManager.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        Title.OnNext(RS.PacketViewerViewDockPanelText);
        _packetsList.AddRange(new[]
        {
            new PacketMessageViewModel { DateTime = DateTime.Now, Source = "[1,1]", Type = "HEARTBEAT", Message = "Test message 1" },
            new PacketMessageViewModel { DateTime = DateTime.Now, Source = "[1,1]", Type = "HEARTBEAT", Message = "Test message 2" },
            new PacketMessageViewModel { DateTime = DateTime.Now, Source = "[1,1]", Type = "HEARTBEAT", Message = "Test message 3" },
        });
    }

    [ImportingConstructor]
    public PacketViewerViewModel(ICommandService cmd, IAppPath app, ILoggerFactory logFactory, IUnitService unit, 
        [ImportMany] IEnumerable<IPacketConverter> converters, IDeviceManager deviceManager)
        : base(PageId, cmd)
    {
        Title.OnNext(RS.PacketViewerViewDockPanelText);
        _app = app;
        _logger = logFactory.CreateLogger<PacketViewerViewModel>();
        _unit = unit;
        _converters = converters;
        _deviceManager = deviceManager;
        
        ExportToCsv = new ReactiveCommand(ExportToCsvAsync);
        ClearAll = new ReactiveCommand(ClearAllAsync);
        
        SetupSubscriptions();
        SetupFiltering(); // Добавляем настройку фильтрации
    }

    private void SetupSubscriptions()
    {
        _deviceManager.Router.OnRxMessage
            .Where(_ => !IsPause.Value)
            .ThrottleFirst(TimeSpan.FromMilliseconds(100))
            .Select(packet => ConvertPacketsAndUpdateFilters((MavlinkMessage)packet))
            .Subscribe(packets =>
            {
                if (_packetsList.Count + packets.Result.Count > MaxPacketSize)
                {
                    var toRemove = _packetsList.Count + packets.Result.Count - MaxPacketSize;
                    _packetsList.RemoveRange(0, toRemove);
                }

                _packetsList.AddRange(packets.Result);
            })
            .AddTo(_disposables);
        
        // Обновление фильтров каждую секунду
        Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                foreach (var filter in _filtersBySourceList)
                {
                    filter.UpdateRateText();
                }

                foreach (var filter in _filtersByTypeList)
                {
                    filter.UpdateRateText();
                }
            })
            .AddTo(_disposables);
        
        // Подписка на выделение пакета с оптимизацией
        SelectedPacket
            .WhereNotNull()
            .Subscribe(selectedPacket =>
            {
                foreach (var item in _packetsList)
                {
                    bool shouldHighlight = item.Type == selectedPacket.Type;
                    if (item.Highlight != shouldHighlight)
                    {
                        item.Highlight = shouldHighlight;
                    }
                }
            })
            .AddTo(_disposables);
    }

    private void SetupFiltering()
    {
        // Функция для фильтрации
        void UpdateFilteredPackets()
        {
            var filtered = _packetsList.ToList()
                .Where(x => _filtersBySourceList.Any(f => f.IsChecked.Value && f.Source.Value == x.Source))
                .Where(x => _filtersByTypeList.Any(f => f.IsChecked.Value && f.Type.Value == x.Type))
                .Where(x => string.IsNullOrEmpty(SearchText.Value) || x.Message.Contains(SearchText.Value, StringComparison.OrdinalIgnoreCase))
                .ToList();

            _filteredPackets.Clear();
            _filteredPackets.AddRange(filtered);
        }

        // Инициализация отфильтрованного списка
        UpdateFilteredPackets();

        // Обновление при изменении исходного списка
        _packetsList.ObserveChanged()
            .Subscribe(_ => UpdateFilteredPackets())
            .AddTo(_disposables);

        Observable.Merge(
                SearchText.Select(_ => Unit.Default), // Преобразуем IObservable<string> в IObservable<Unit>
                _filtersBySourceList.ObserveChanged().Select(_ => Unit.Default), // Преобразуем IObservable<CollectionChanged<PacketFilterViewModel>> в IObservable<Unit>
                _filtersByTypeList.ObserveChanged().Select(_ => Unit.Default)) // Преобразуем IObservable<CollectionChanged<PacketFilterViewModel>> в IObservable<Unit>
            .Subscribe(_ => UpdateFilteredPackets())
            .AddTo(_disposables);
    }

    #region Commands
    public ReactiveCommand ExportToCsv { get; }
    
    public ValueTask ExportToCsvAsync(Unit unit, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
    
    public ReactiveCommand ClearAll { get; }
    
    public ValueTask ClearAllAsync(Unit unit, CancellationToken cancellationToken)
    {
        _packetsList.Clear();
        _filtersBySourceList.Clear();
        _filtersByTypeList.Clear();
        return ValueTask.CompletedTask;
    }
    #endregion
    
    private async Task<IList<PacketMessageViewModel>> ConvertPacketsAndUpdateFilters(MavlinkMessage packet)
    {
        var result = new List<PacketMessageViewModel>();
        var converter = _converters.FirstOrDefault(_ => _.CanConvert(packet)) ?? new DefaultPacketConverter();
        var vm = await Dispatcher.UIThread.InvokeAsync(() => new PacketMessageViewModel(packet, converter));
        result.Add(vm);

        UpdateFilters(vm);
        return result;
    }

    private void UpdateFilters(PacketMessageViewModel vm)
    {
        // Обновление фильтров по источнику
        var sourceFilter = _filtersBySourceList.FirstOrDefault(x => x.Source.Value == vm.Source);
        if (sourceFilter != null)
        {
            sourceFilter.UpdateRates();
        }
        else
        {
            _filtersBySourceList.Add(new PacketFilterViewModel(vm, _unit));
        }

        // Обновление фильтров по типу
        var typeFilter = _filtersByTypeList.FirstOrDefault(x => x.Type.Value == vm.Type);
        if (typeFilter != null)
        {
            typeFilter.UpdateRates();
        }
        else
        {
            _filtersByTypeList.Add(new PacketFilterViewModel(vm, _unit));
        }
    }
    
    public override IEnumerable<IRoutable> GetRoutableChildren() => [];
    protected override void AfterLoadExtensions() { }
    public override IExportInfo Source => SystemModule.Instance;
}