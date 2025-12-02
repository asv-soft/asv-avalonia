using System.Diagnostics;
using Asv.Common;
using Asv.IO;
using R3;

namespace Asv.Avalonia.IO;

public abstract class HomePageDeviceItemAction : AsyncDisposableOnce, IExtensionFor<IHomePageItem>
{
    private readonly SerialDisposable _sub1 = new();

    private IActionViewModel? _action;
    private IClientDevice? _device;
    private HomePageDeviceItem? _context;

    public void Extend(IHomePageItem context, CompositeDisposable contextDispose)
    {
        if (context is HomePageDeviceItem item)
        {
            _context = item;
            _device = item.Device;

            // we need to wait for the device to be initialized
            _sub1.Disposable = _device
                .State.Where(x => x == ClientDeviceState.Complete)
                .Take(1)
                .Subscribe(AddActions);
        }
    }

    private void AddActions(ClientDeviceState clientDeviceState)
    {
        Debug.Assert(_context != null, "_context != null");
        Debug.Assert(_device != null, "_device != null");
        Debug.Assert(_action == null, "_action == null");

        if (_device.State.CurrentValue != ClientDeviceState.Complete)
        {
            return;
        }

        _action = TryCreateAction(_device, _context);
        if (_action != null)
        {
            _context.Actions.Add(_action);
        }
    }

    protected abstract IActionViewModel? TryCreateAction(
        IClientDevice device,
        HomePageDeviceItem context
    );

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _action?.Dispose();
        }

        base.Dispose(disposing);
    }
}
