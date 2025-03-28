using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using R3;

namespace Asv.Avalonia.Example;

public class MissionProgressViewModel : DisposableViewModel
{
    private readonly IClientDevice _device;
    private readonly PositionClientEx? _positionClient;
    private readonly MissionClientEx? _missionClient;
    private readonly GnssClientEx? _gnssClientEx;
    private List<MissionItem> _items = [];
    private double _totalMissionDistance;
    private double _passedDistance;
    private double _distanceBeforeMission;
    private bool _isOnMission = false;

    public MissionProgressViewModel()
        : base(string.Empty)
    {
    }

    public MissionProgressViewModel(NavigationId id, IClientDevice device, IUnitService unitService) : base(id)
    {
        _device = device;
        DistanceUnitItem.Value = unitService.Units.Values.First(_ => _.UnitId == DistanceBase.Id).Current.Value;
        _missionClient = device.GetMicroservice<MissionClientEx>();
        _gnssClientEx = device.GetMicroservice<GnssClientEx>();
        _positionClient = device.GetMicroservice<PositionClientEx>();
        var mode = device.GetMicroservice<ModeClient>();

        if (mode is null)
        {
            return;
        }

        if (_missionClient is null || _gnssClientEx is null || _positionClient is null )
        {
            return;
        }

        Task.Run(async () => await InitiateMissionPoints(new CancellationToken()));
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
        });
        PathProgress.Subscribe(p =>
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
        }).DisposeItWith(Disposable);
        
        _positionClient.HomeDistance.Subscribe(d => HomeDistance.Value = DistanceUnitItem.Value.Print(d, "N2")).DisposeItWith(Disposable);
        _positionClient.TargetDistance.Subscribe(d =>
        {
            TargetDistance.Value = d is double.NaN
                ? Avalonia.RS.Not_Available
                : DistanceUnitItem.Value.Print(d, "N2");
        }).DisposeItWith(Disposable);

        _missionClient.AllMissionsDistance.Subscribe(d =>
            {
                var missionDistance = d * 1000;
                var totalDistance = missionDistance;
                MissionDistance.Value =
                    DistanceUnitItem.Value.Print(totalDistance, "N2");
                var start = _items.FirstOrDefault();
                var stop = _items.LastOrDefault(
                    _ => _.Command.Value != MavCmd.MavCmdNavReturnToLaunch);
                if (start != null && stop != null)
                {
                    totalDistance +=
                        GeoMath.Distance(
                            start.Location.Value,
                            _positionClient?.Home.CurrentValue);
                    totalDistance +=
                        GeoMath.Distance(
                            stop.Location.Value,
                            _positionClient?.Home.CurrentValue);
                }

                if (totalDistance < 1)
                {
                    totalDistance = missionDistance;
                }

                TotalDistance.Value = DistanceUnitItem.Value.Print(totalDistance, "N2");
            })
            .DisposeItWith(Disposable);

        CurrentIndex.Subscribe(c =>
        {
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
                _passedDistance += GeoMath.Distance(items[i - 1].Location.Value, items[i].Location.Value);
            }
        });

        _missionClient.Reached.Subscribe(i => ReachedIndex.Value = i);
        _missionClient.Current.Subscribe(i => CurrentIndex.Value = i);
        Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .Subscribe(_ => CalculateMissionProgress())
            .DisposeItWith(Disposable);
    }

    private async Task InitiateMissionPoints(CancellationToken cancel)
    {
        await DownloadMissionsImpl(cancel);
    }

    private async Task DownloadMissionsImpl(CancellationToken cancel)
    {
        try
        {
            if (_missionClient != null)
            {
                await _missionClient.Download(cancel, p => DownloadProgress.Value = p * 100);
            }
            else
            {
                return;
            }

            double homeAlt = 0;

            if (_positionClient?.Home.CurrentValue != null)
            {
                homeAlt = _positionClient.Home.CurrentValue.Value.Altitude;
            }

            for (var i = 0; i < _items?.Count(); i++)
            {
                if (i == 0 && _device is ArduPlaneClientDevice)
                {
                    continue;
                }

                var item = _items[i];

                if (item.Command.Value == MavCmd.MavCmdNavWaypoint && item.Location.Value.Altitude <= homeAlt)
                {
                    //TODO: Notify user on alt lower than start value
                }
            }

            _items = _missionClient?.MissionItems.Where(_ => _.Command.Value
                is MavCmd.MavCmdNavWaypoint
                or MavCmd.MavCmdNavReturnToLaunch
                or MavCmd.MavCmdNavSplineWaypoint).ToList() ?? [];
            _totalMissionDistance = _missionClient?.AllMissionsDistance.CurrentValue ?? 0;
            TotalDistance.Value = DistanceUnitItem.Value.Print(_totalMissionDistance * 1000, "#.##");
            _passedDistance += _distanceBeforeMission;
            IsDownloaded.Value = true;
        }
        catch (Exception e)
        {
            // _log.Error("MissionStatus", e.Message);
        }
    }
    


    private void CalculateMissionProgress()
    {
        if (_positionClient is null || _missionClient is null || _gnssClientEx is null)
        {
            return;
        }

        var toTargetDistance =
            GeoMath.Distance(_positionClient.Target.CurrentValue, _positionClient.Current.CurrentValue);
        var missionDistance = _totalMissionDistance * 1000;
        var distance = Math.Abs(missionDistance - _passedDistance + toTargetDistance);
        var time = distance / _gnssClientEx.Main.GroundVelocity.CurrentValue;

        PathProgress.Value = CalculatePathProgressValue(missionDistance, distance);
        MissionFlightTime.Value = CalculateMissionFlightTime(time);
    }

    private string CalculateMissionFlightTime(double time)
    {
        if (time is double.NaN or double.PositiveInfinity)
        {
            return $"- min";
        }

        var minute = Math.Round(time / 60);

        if (minute < 1)
        {
            return $"<1 min";
        }

        return $"{minute} min";
    }

    private double
        CalculatePathProgressValue(double missionDistance, double distance) //TDOD: extend logic for plane clients
    {
        switch (_device)
        {
            case ArduCopterClientDevice:
                if (_isOnMission && ReachedIndex.Value > 0)
                {
                    return Math.Abs((missionDistance - distance - _distanceBeforeMission) /
                                    (missionDistance - _distanceBeforeMission));
                }

                if (CurrentIndex.Value == _missionClient?.MissionItems.Count)
                {
                    return 1;
                }

                if (CurrentIndex.CurrentValue == 0)
                {
                    return 0;
                }

                return 0;
            default:
                return Math.Abs((missionDistance - distance) / missionDistance);
        }
    }

    public BindableReactiveProperty<string> MissionFlightTime { get; set; } = new($"- min");

    public BindableReactiveProperty<IUnitItem> DistanceUnitItem { get; set; } = new();

    public BindableReactiveProperty<double> DownloadProgress { get; set; } = new();

    public bool EnablePolygon { get; set; } = true;

    public bool EnableAnchors { get; set; } = true;

    public BindableReactiveProperty<string> MissionDistance { get; set; } = new(Avalonia.RS.Not_Available);

    public BindableReactiveProperty<string> TotalDistance { get; set; } = new(Avalonia.RS.Not_Available);
    public BindableReactiveProperty<string> HomeDistance { get; set; } = new(Avalonia.RS.Not_Available);
    public BindableReactiveProperty<string> TargetDistance { get; set; } = new(Avalonia.RS.Not_Available);

    public BindableReactiveProperty<bool> IsDownloaded { get; set; } = new(false);

    /// <summary>
    /// Gets or sets represents progress of the mission.
    /// Changes from 0 to 1
    /// </summary>
    public BindableReactiveProperty<double> PathProgress { get; set; } = new(0);

    public ReactiveProperty<ushort> CurrentIndex { get; set; } = new();

    public ReactiveProperty<ushort> ReachedIndex { get; set; } = new();
}