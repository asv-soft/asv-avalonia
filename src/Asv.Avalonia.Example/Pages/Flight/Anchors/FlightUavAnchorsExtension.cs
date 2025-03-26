using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using Asv.Avalonia.IO;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IFlightMode>]
[method: ImportingConstructor]
public class FlightUavAnchorsExtension(IDeviceManager conn) : IExtensionFor<IFlightMode>
{
    public void Extend(IFlightMode context, CompositeDisposable contextDispose)
    {
        conn.Explorer.Devices.PopulateTo(context.Anchors, TryCreateAnchor, RemoveAnchor)
            .DisposeItWith(contextDispose);
        conn.Explorer.Devices.PopulateTo(context.Widgets, TryCreateWidget, RemoveWidget)
            .DisposeItWith(contextDispose);
    }

    private UavWidgetViewModel? TryCreateWidget(KeyValuePair<DeviceId, IClientDevice> device)
    {
        var pos = device.Value.GetMicroservice<IPositionClientEx>();
        return pos != null ? new UavWidgetViewModel(device.Value) : null;
    }

    private bool RemoveWidget(KeyValuePair<DeviceId, IClientDevice> model, UavWidgetViewModel vm)
    {
        return model.Key == vm.Device.Id;
    }

    private static UavAnchor? TryCreateAnchor(KeyValuePair<DeviceId, IClientDevice> device)
    {
        var pos = device.Value.GetMicroservice<IPositionClientEx>();
        return pos != null ? new UavAnchor(device.Key, device.Value, pos) : null;
    }

    private static bool RemoveAnchor(KeyValuePair<DeviceId, IClientDevice> dev, UavAnchor anchor)
    {
        return anchor.DeviceId == dev.Key;
    }
}
