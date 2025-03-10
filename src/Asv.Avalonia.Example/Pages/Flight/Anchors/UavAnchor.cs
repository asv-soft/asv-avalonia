using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

public class UavAnchor : MapAnchor
{
    public DeviceId DeviceId { get; }

    public UavAnchor()
        : base("uav_design_time")
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public UavAnchor(DeviceId deviceId, IClientDevice dev, IPositionClientEx pos)
        : base("uav")
    {
        DeviceId = deviceId;
        InitArgs(deviceId.AsString());
        IsReadOnly = true;
        IsVisible = true;
        Icon = MaterialIconKind.Navigation;
        dev.Name.Subscribe(x => Title = x ?? string.Empty).DisposeItWith(Disposable);
        pos.Current.Subscribe(x => Location = x).DisposeItWith(Disposable);
        pos.Yaw.Subscribe(x => Azimuth = x).DisposeItWith(Disposable);
    }
}

[ExportExtensionFor<IFlightMode>]
[method: ImportingConstructor]
public class FlightUavAnchorExtension(IMavlinkConnectionService conn)
    : AsyncDisposableOnce,
        IExtensionFor<IFlightMode>
{
    private IDisposable? _sub1;
    private IDisposable? _sub2;
    private IFlightMode? _context;

    public void Extend(IFlightMode context)
    {
        _context = context;
        _sub1 = conn.DevicesExplorer.Devices.ObserveAdd().Select(x => x.Value.Value).Subscribe(Add);
        _sub2 = conn
            .DevicesExplorer.Devices.ObserveRemove()
            .Select(x => x.Value.Value)
            .Subscribe(Remove);
        foreach (var device in conn.DevicesExplorer.Devices)
        {
            Add(device.Value);
        }
    }

    private void Remove(IClientDevice dev)
    {
        Debug.Assert(_context != null, "_context != null");
        var itemsToDelete = new List<IMapAnchor>();
        foreach (var anchor in _context.Anchors)
        {
            if (anchor is UavAnchor uavAnchor && uavAnchor.DeviceId == dev.Id)
            {
                itemsToDelete.Add(anchor);
            }
        }

        foreach (var anchor in itemsToDelete)
        {
            _context.Anchors.Remove(anchor);
        }
    }

    private void Add(IClientDevice device)
    {
        Debug.Assert(_context != null, "_context != null");
        var pos = device.GetMicroservice<IPositionClientEx>();
        if (pos != null)
        {
            _context.Anchors.Add(new UavAnchor(device.Id, device, pos));
            return;
        }
    }
}
