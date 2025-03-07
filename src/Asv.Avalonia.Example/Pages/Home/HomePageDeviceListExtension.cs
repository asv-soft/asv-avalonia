using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePage>]
public class HomePageDeviceListExtension : AsyncDisposableOnce, IExtensionFor<IHomePage>
{
    private IDisposable? _sub1;
    private IDisposable? _sub2;
    private readonly IMavlinkConnectionService _svc;
    private readonly IContainerHost _container;
    private IHomePage _context;

    [ImportingConstructor]
    public HomePageDeviceListExtension(IMavlinkConnectionService svc, IContainerHost container)
    {
        ArgumentNullException.ThrowIfNull(svc);
        ArgumentNullException.ThrowIfNull(container);
        _svc = svc;
        _container = container;
    }

    public void Extend(IHomePage context)
    {
        Debug.Assert(_context == null, "_context == null");

        _context = context;
        _sub1 = _svc.DevicesExplorer.Devices.ObserveAdd().Select(x => x.Value).Subscribe(Add);

        _sub2 = _svc.DevicesExplorer.Devices.ObserveRemove().Select(x => x.Value).Subscribe(Remove);

        foreach (var device in _svc.DevicesExplorer.Devices)
        {
            Add(device);
        }
    }

    private void Add(KeyValuePair<DeviceId, IClientDevice> e)
    {
        Debug.Assert(_context != null, "_context != null");
        var device = new HomePageDevice(e.Value);
        _context.Items.Add(device);
    }

    private void Remove(KeyValuePair<DeviceId, IClientDevice> arg)
    {
        Debug.Assert(_context != null, "_context != null");
        var itemToDelete = _context.Items.FirstOrDefault(x =>
            x is HomePageDevice adapter && adapter.Device.Id == arg.Key
        );
        if (itemToDelete == null)
        {
            return;
        }

        _context.Items.Remove(itemToDelete);
        itemToDelete.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1?.Dispose();
            _sub2?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_sub1 != null)
        {
            await CastAndDispose(_sub1);
        }

        if (_sub2 != null)
        {
            await CastAndDispose(_sub2);
        }

        await base.DisposeAsyncCore();

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
            {
                await resourceAsyncDisposable.DisposeAsync();
            }
            else
            {
                resource.Dispose();
            }
        }
    }
}
