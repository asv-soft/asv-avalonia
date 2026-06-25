using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class HomePageDeviceListExtension : IExtensionFor<IHomePage>
{
    public const string StaticId = "ext.home.device-list";

    string ISupportId<string>.Id => StaticId;

    private readonly IDeviceManager _svc;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IExtensionService _ext;

    public HomePageDeviceListExtension(
        IDeviceManager svc,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
    {
        ArgumentNullException.ThrowIfNull(svc);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _svc = svc;
        _loggerFactory = loggerFactory;
        _ext = ext;
    }

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var synchronizationContext = new AvaloniaSynchronizationContext(
            Dispatcher.UIThread,
            DispatcherPriority.Default
        );

        _svc.Explorer.InitializedDevices.PopulateTo(
                context.Items,
                TryAdd,
                Remove,
                synchronizationContext: synchronizationContext
            )
            .DisposeItWith(contextDispose);
    }

    private static bool Remove(IClientDevice model, HomePageDeviceItem vm)
    {
        return model.Id == vm.Device.Id;
    }

    private HomePageDeviceItem TryAdd(IClientDevice device)
    {
        return new HomePageDeviceItem(device, _svc, _loggerFactory, _ext);
    }
}
