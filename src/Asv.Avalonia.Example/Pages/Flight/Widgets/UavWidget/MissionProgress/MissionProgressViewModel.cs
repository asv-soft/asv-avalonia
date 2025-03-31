using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

public class MissionProgressViewModel : DisposableViewModel
{
    private readonly ILogger _log;
    private readonly IClientDevice _device;
    private readonly IFlightMode _flightContext;
    private readonly PositionClientEx _positionClient;
    private readonly MissionClientEx _missionClient;
    private readonly GnssClientEx _gnssClientEx;
    private List<MissionItem> _items;
    private CancellationTokenSource _cts = new();
    private double _passedDistance;
    private double _distanceBeforeMission;
    private bool _isOnMission;

    public MissionProgressViewModel()
        : base(string.Empty)
    {
    }

    [ImportingConstructor]
    public MissionProgressViewModel(
        NavigationId id,
        IClientDevice device,
        IUnitService unitService,
        IFlightMode flightContext,
        ILoggerFactory loggerFactory
    )
        : base(id)
    {
        _log = loggerFactory.CreateLogger<MissionProgressViewModel>();
        _device = device;
        _flightContext = flightContext;
        DistanceUnitItem.Value = unitService.Units[DistanceBase.Id];
        _missionClient = device.GetMicroservice<MissionClientEx>() ??
                         throw new ArgumentException(
                             $"Unable to get {nameof(MissionClientEx)} service from {device.Id}");
        _gnssClientEx = device.GetMicroservice<GnssClientEx>() ??
                        throw new ArgumentException(
                            $"Unable to get {nameof(GnssClientEx)} service from {device.Id}");
        _positionClient = device.GetMicroservice<PositionClientEx>() ??
                          throw new ArgumentException(
                              $"Unable to get {nameof(PositionClientEx)} service from {device.Id}");
        var mode = device.GetMicroservice<ModeClient>() ?? throw new ArgumentException(
            $"Unable to get {nameof(ModeClient)} service from {device.Id}");
        
        Task.Run(async () => await InitiateMissionPoints(_cts.Token));
        _missionClient.MissionItems.CollectionChanged += (in NotifyCollectionChangedEventArgs<MissionItem> args) =>
        {
            _items = _missionClient.MissionItems.Where(item =>
                item.Command.Value
                    is MavCmd.MavCmdNavWaypoint
                    or MavCmd.MavCmdNavReturnToLaunch
                    or MavCmd.MavCmdNavSplineWaypoint
                    or MavCmd.MavCmdMissionStart).ToList();
        };  
        
        mode.CurrentMode.Subscribe(m =>
            {
                if (m == ArduCopterMode.Auto || m == ArduPlaneMode.Auto)
                {
                    if (_isOnMission)
                    {
                        return;
                    }

                    _isOnMission = true;
                    return;
                }

                if (m == ArduCopterMode.Rtl || m == ArduPlaneMode.Rtl)
                {
                    _isOnMission = false;
                    ReachedIndex.Value = 0;
                }
            })
            .DisposeItWith(Disposable);
        PathProgress
            .Subscribe(p =>
            {
                switch (p)
                {
                    case > 1:
                        PathProgress.Value = 1;
                        return;
                    case < 0:
                        PathProgress.Value = 0;
                        return;
                    default:
                        PathProgress.Value = p;
                        break;
                }
            })
            .DisposeItWith(Disposable);

        _positionClient
            .HomeDistance.Subscribe(d => HomeDistance.Value = DistanceUnitItem.Value.Current.Value.Print(d, "N2"))
            .DisposeItWith(Disposable);
        _positionClient
            .TargetDistance.Subscribe(d =>
            {
                TargetDistance.Value = d is double.NaN
                    ? RS.Not_Available
                    : DistanceUnitItem.Value.Current.Value.Print(d, "N2");
            })
            .DisposeItWith(Disposable);
        _missionClient
            .AllMissionsDistance.Subscribe(d =>
            {
                if (_items is null)
                {
                    return;
                }

                var missionDistance = d * 1000;
                var totalDistance = missionDistance;
                MissionDistance.Value = DistanceUnitItem.Value.Current.Value.Print(totalDistance, "N2");
                var start = _items.FirstOrDefault();
                var stop = _items.LastOrDefault(_ =>
                    _.Command.Value != MavCmd.MavCmdNavReturnToLaunch
                );
                if (start != null && stop != null)
                {
                    totalDistance += GeoMath.Distance(
                        start.Location.Value,
                        _positionClient.Home.CurrentValue
                    );
                    totalDistance += GeoMath.Distance(
                        stop.Location.Value,
                        _positionClient?.Home.CurrentValue
                    );
                }

                if (totalDistance < 1)
                {
                    totalDistance = missionDistance;
                }

                TotalDistance.Value = DistanceUnitItem.Value.Current.Value.Print(totalDistance, "N2");
            })
            .DisposeItWith(Disposable);

        CurrentIndex
            .Subscribe(c =>
            {
                if (_items is null)
                {
                    return;
                }

                if (_items.Count == 0)
                {
                    return;
                }

                _passedDistance = 0;
                var items = _items.Where(item => item.Index <= c).ToList();
                if (items.Count < 2)
                {
                    return;
                }

                for (var i = 1; i < items.Count; i++)
                {
                    _passedDistance += GeoMath.Distance(
                        items[i - 1].Location.Value,
                        items[i].Location.Value
                    );
                }
            })
            .DisposeItWith(Disposable);

        _missionClient.Reached.Subscribe(i => ReachedIndex.Value = i).DisposeItWith(Disposable);
        _missionClient.Current.Subscribe(i => CurrentIndex.Value = i).DisposeItWith(Disposable);
        Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .Subscribe(_ => CalculateMissionProgress())
            .DisposeItWith(Disposable);
    }

    private async Task InitiateMissionPoints(CancellationToken cancel)
    {
        await DownloadMissionsImpl(cancel);
    }

    private async ValueTask DownloadMissionsImpl(CancellationToken cancel)
    {
        try
        {
            await _missionClient.Download(cancel, p => DownloadProgress.Value = p * 100);
            double homeAlt = 0;
            if (_positionClient.Home.CurrentValue != null)
            {
                homeAlt = _positionClient.Home.CurrentValue.Value.Altitude;
            }
            
            if (_missionClient.MissionItems.Count > 0)
            {
                for (var i = 1; i < _items.Count; i++)
                {
                    _flightContext.Anchors.Add(
                        new MissionAnchor(
                            i,
                            _items[i].Location.Value,
                            _items[i - 1].Location.Value
                        )
                    );
                }
            }
            
            TotalDistance.Value = DistanceUnitItem.Value.Current.Value.Print(_missionClient.AllMissionsDistance.CurrentValue * 1000, "N2");
            _passedDistance += _distanceBeforeMission;
            IsDownloaded.Value = true;

            if (_items is null)
            {
                return;
            }

            for (var i = 0; i < _items.Count; i++)
            {
                if (i == 0 && _device is ArduPlaneClientDevice)
                {
                    continue;
                }

                var item = _items[i];

                if (
                    item.Command.Value == MavCmd.MavCmdNavWaypoint
                    && item.Location.Value.Altitude <= homeAlt
                )
                {
                    _log.LogInformation($"Point #{item.Index} ({item.Location}) lower than start point ");
                }
            }
        }
        catch (Exception e)
        {
            _log.LogError(e, "Mission download failed");
        }
    }

    private void CalculateMissionProgress()
    {
        var toTargetDistance = GeoMath.Distance(
            _positionClient.Target.CurrentValue,
            _positionClient.Current.CurrentValue
        );
        var missionDistance = _missionClient.AllMissionsDistance.CurrentValue * 1000;
        var distance = Math.Abs(missionDistance - _passedDistance + toTargetDistance);
        var time = distance / _gnssClientEx.Main.GroundVelocity.CurrentValue;

        PathProgress.Value = CalculatePathProgressValue(missionDistance, distance);
        MissionFlightTime.Value = CalculateMissionFlightTime(time);
    }

    private string CalculateMissionFlightTime(double time)
    {
        if (time is double.NaN or double.PositiveInfinity)
        {
            return "- min";
        }

        var minute = Math.Round(time / 60);

        if (minute < 1)
        {
            return "<1 min";
        }

        return $"{minute} min";
    }

    private double CalculatePathProgressValue(double missionDistance, double distance) // TODO: extend logic for plane clients
    {
        switch (_device)
        {
            case ArduCopterClientDevice:
                if (_isOnMission && ReachedIndex.Value > 0)
                {
                    return Math.Abs(
                        (missionDistance - distance - _distanceBeforeMission)
                        / (missionDistance - _distanceBeforeMission)
                    );
                }

                return 0;
            default:
                return Math.Abs((missionDistance - distance) / missionDistance);
        }
    }

    public BindableReactiveProperty<string> MissionFlightTime { get; } = new("- min");

    public BindableReactiveProperty<IUnit> DistanceUnitItem { get; } = new();

    public BindableReactiveProperty<double> DownloadProgress { get; } = new();

    public BindableReactiveProperty<string> MissionDistance { get; } = new(RS.Not_Available);

    public BindableReactiveProperty<string> TotalDistance { get; } = new(RS.Not_Available);
    public BindableReactiveProperty<string> HomeDistance { get; } = new(RS.Not_Available);
    public BindableReactiveProperty<string> TargetDistance { get; } = new(RS.Not_Available);

    public BindableReactiveProperty<bool> IsDownloaded { get; } = new(false);

    /// <summary>
    /// Gets or sets represents progress of the mission.
    /// Changes from 0 to 1
    /// </summary>
    public BindableReactiveProperty<double> PathProgress { get; } = new(0);

    public ReactiveProperty<ushort> CurrentIndex { get; } = new();

    public ReactiveProperty<ushort> ReachedIndex { get; } = new();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts.Cancel();
            MissionFlightTime.Dispose();
            DistanceUnitItem.Dispose();
            DownloadProgress.Dispose();
            MissionDistance.Dispose();
            TotalDistance.Dispose();
            HomeDistance.Dispose();
            TargetDistance.Dispose();
            IsDownloaded.Dispose();
            PathProgress.Dispose();
            CurrentIndex.Dispose();
            ReachedIndex.Dispose();
        }

        base.Dispose(disposing);
    }
}