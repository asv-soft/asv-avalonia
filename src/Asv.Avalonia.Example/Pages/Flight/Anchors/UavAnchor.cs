using Asv.Avalonia.Map;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Avalonia.Example;

public class UavAnchor : MapAnchor<UavAnchor>
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
        Icon = DeviceIconMixin.GetIcon(deviceId) ?? MaterialIconKind.Memory;
        Foreground = DeviceIconMixin.GetIconBrush(deviceId);
        CenterX = DeviceIconMixin.GetIconCenterX(deviceId);
        CenterY = DeviceIconMixin.GetIconCenterY(deviceId);
        dev.Name.Subscribe(x => Title = x ?? string.Empty).DisposeItWith(Disposable);
        pos.Current.Subscribe(x => Location = x).DisposeItWith(Disposable);
        pos.Yaw.Subscribe(x => Azimuth = x).DisposeItWith(Disposable);
        Polygon.Add(new GeoPoint(0, 0, 0));
        Polygon.Add(pos.Current.CurrentValue);
    }
}
