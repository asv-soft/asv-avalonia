using System;
using Asv.Avalonia.Map;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Material.Icons;

namespace Asv.Avalonia.Example;

public static class DeviceIconMixin
{
    public static readonly ImmutableSolidColorBrush[] DeviceColors = new[]
    {
        new ImmutableSolidColorBrush(Color.Parse("#AA00FF")),
        new ImmutableSolidColorBrush(Color.Parse("#6200EA")),
        new ImmutableSolidColorBrush(Color.Parse("#304FFE")),
        new ImmutableSolidColorBrush(Color.Parse("#2962FF")),
        new ImmutableSolidColorBrush(Color.Parse("#0091EA")),
        new ImmutableSolidColorBrush(Color.Parse("#00B8D4")),
        new ImmutableSolidColorBrush(Color.Parse("#00BFA5")),
    };

    public static MaterialIconKind? GetIcon(DeviceId deviceId)
    {
        switch (deviceId.DeviceClass)
        {
            case Vehicles.PlaneDeviceClass:
                return MaterialIconKind.Plane;
            case Vehicles.CopterDeviceClass:
                return MaterialIconKind.Navigation;
            case GbsClientDevice.DeviceClass:
                return MaterialIconKind.RouterWireless;
            default:
                return null;
        }
    }

    public static IBrush GetIconBrush(DeviceId deviceId)
    {
        return DeviceColors[Math.Abs(deviceId.GetHashCode()) % DeviceColors.Length];
    }

    public static HorizontalOffset GetIconCenterX(DeviceId deviceId)
    {
        switch (deviceId.DeviceClass)
        {
            case Vehicles.PlaneDeviceClass:
                return HorizontalOffset.Default;
            case Vehicles.CopterDeviceClass:
                return HorizontalOffset.Default;
            case GbsClientDevice.DeviceClass:
                return HorizontalOffset.Default;
            default:
                return HorizontalOffset.Default;
        }
    }

    public static VerticalOffset GetIconCenterY(DeviceId deviceId)
    {
        switch (deviceId.DeviceClass)
        {
            case Vehicles.PlaneDeviceClass:
                return VerticalOffset.Default;
            case Vehicles.CopterDeviceClass:
                return VerticalOffset.Default;
            case GbsClientDevice.DeviceClass:
                return VerticalOffset.Default;
            default:
                return VerticalOffset.Default;
        }
    }
}
