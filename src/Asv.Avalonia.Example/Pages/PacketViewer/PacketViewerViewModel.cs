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
    private readonly ILogger<PacketViewerViewModel> _logger;
    private readonly IAppPath _app;
    private readonly IUnitService _unit;
    private readonly IDeviceManager _deviceManager;
    
   // private readonly IMavlinkConnectionService _connectionService;
    private readonly IEnumerable<IPacketConverter> _converters;
    private readonly CompositeDisposable _disposables = new();
    public const int MaxPacketSize = 1000;
    
    private readonly ObservableList<PacketMessageViewModel> _packetsList = new();
    private readonly ObservableList<PacketFilterViewModel> _filtersBySourceList = new();
    private readonly ObservableList<PacketFilterViewModel> _filtersByTypeList = new();
    public BindableReactiveProperty<bool> IsPause { get; } = new(false);
    public BindableReactiveProperty<string> SearchText { get; } = new(string.Empty);
    public BindableReactiveProperty<PacketMessageViewModel?> SelectedPacket { get; } = new();
    
    public ISynchronizedView<PacketMessageViewModel, PacketMessageViewModel> Packets { get; }
    public ISynchronizedView<PacketFilterViewModel, PacketFilterViewModel> FiltersBySource { get; }
    public ISynchronizedView<PacketFilterViewModel, PacketFilterViewModel> FiltersByType { get; }
    
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
    public PacketViewerViewModel(ICommandService cmd, IAppPath app, ILoggerFactory logFactory, IUnitService unit, [ImportMany] IEnumerable<IPacketConverter> converters, IDeviceManager deviceManager)
        : base(PageId, cmd)
    {
        Title.OnNext(RS.PacketViewerViewDockPanelText);
        _app = app;
        _logger = logFactory.CreateLogger<PacketViewerViewModel>();
        _unit = unit;
        _converters = converters;
        _deviceManager = deviceManager;
        
        Packets = _packetsList.CreateView(x => x);
        FiltersBySource = _filtersBySourceList.CreateView(x => x);
        FiltersByType = _filtersByTypeList.CreateView(x => x);
        
        ExportToCsv = new ReactiveCommand(ExportToCsvAsync);
        ClearAll = new ReactiveCommand(ClearAllAsync);

        /*_deviceManager.Router.OnRxMessage
            .Where(_ => !IsPause.Value)
            .ThrottleFirst(TimeSpan.FromMilliseconds(100))
            .Select(ConvertPacketsAndUpdateFilters)
            .Subscribe(packets =>
            {
                if (_packetsList.Count + packets.Count > MaxPacketSize)
                {
                    var toRemove = _packetsList.Count + packets.Count - MaxPacketSize;
                    _packetsList.RemoveRange(0, toRemove);
                }
                
                _packetsList.AddRange(packets);
            })
            .DisposeItWith(_disposables);*/
        
        #region Setup Subscriptions
        
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
            .DisposeItWith(_disposables);
        
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
            .DisposeItWith(_disposables);
        
        SelectedPacket
            .Where(packet => packet != null)
            .Subscribe(packet =>
            {
                foreach (var item in _packetsList)
                {
                    item.Highlight = item.Type == packet?.Type;
                }
            })
            .DisposeItWith(_disposables);
        
        #endregion 
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
        return ValueTask.CompletedTask;
    }
    
    #endregion
    
    private async Task<IList<PacketMessageViewModel>> ConvertPacketsAndUpdateFilters(MavlinkMessage packet)
    {
        var result = new List<PacketMessageViewModel>();
        var converter = _converters.FirstOrDefault(_ => _.CanConvert(packet)) ?? new DefaultPacketConverter();
        var vm = await Dispatcher.UIThread.InvokeAsync(() => new PacketMessageViewModel(packet, converter));
        result.Add(vm);

        var sourceFilter = _filtersBySourceList.FirstOrDefault(x => x.Source.Value == vm.Source);
        if (sourceFilter != null)
        {
            sourceFilter.UpdateRates();
        }
        else
        {
            _filtersBySourceList.Add(new PacketFilterViewModel(vm, _unit));
        }

        var typeFilter = _filtersByTypeList.FirstOrDefault(x => x.Type.Value == vm.Type);
        if (typeFilter != null)
        {
            typeFilter.UpdateRates();
        }
        else
        {
            _filtersByTypeList.Add(new PacketFilterViewModel(vm, _unit));
        }

        return result;
    }
    
    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}