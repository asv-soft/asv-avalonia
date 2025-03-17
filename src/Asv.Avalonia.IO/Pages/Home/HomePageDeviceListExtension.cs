using System.Composition;
using Asv.Common;
using Asv.IO;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<IHomePage>]
public class HomePageDeviceListExtension : IExtensionFor<IHomePage>
{
    private readonly IDeviceManager _svc;

    [ImportingConstructor]
    public HomePageDeviceListExtension(IDeviceManager svc)
    {
        ArgumentNullException.ThrowIfNull(svc);
        _svc = svc;
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
        return new HomePageDeviceItem(device, _svc);
    }
}
