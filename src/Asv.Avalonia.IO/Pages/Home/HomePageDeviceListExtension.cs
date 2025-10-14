using System.Composition;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<IHomePage>]
public class HomePageDeviceListExtension : IExtensionFor<IHomePage>
{
    private readonly IDeviceManager _svc;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILayoutService _layoutService;

    [ImportingConstructor]
    public HomePageDeviceListExtension(
        IDeviceManager svc,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
    {
        ArgumentNullException.ThrowIfNull(svc);
        ArgumentNullException.ThrowIfNull(layoutService);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _svc = svc;
        _layoutService = layoutService;
        _loggerFactory = loggerFactory;
    }

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        _svc.Explorer.InitializedDevices.PopulateTo(context.Items, TryAdd, Remove)
            .DisposeItWith(contextDispose);
    }

    private static bool Remove(IClientDevice model, HomePageDeviceItem vm)
    {
        return model.Id == vm.Device.Id;
    }

    private HomePageDeviceItem TryAdd(IClientDevice device)
    {
        return new HomePageDeviceItem(device, _svc, _layoutService, _loggerFactory);
    }
}
