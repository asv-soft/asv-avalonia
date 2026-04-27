using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class HomePageDeviceItem : HomePageItem
{
    public HomePageDeviceItem(
        IClientDevice device,
        IDeviceManager deviceManager,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(
            "home_device_page",
            new NavArgs(new KeyValuePair<string, string?>("dev", device.Id.ToString())),
            loggerFactory,
            ext
        )
    {
        Device = device;
        Icon = deviceManager.GetIcon(device.Id);
        IconColor = deviceManager.GetDeviceColor(device.Id);
        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        Info.Add(
            new HeadlinedViewModel("id")
            {
                Icon = MaterialIconKind.IdCard,
                Header = RS.HomePageDeviceItem_Info_Id,
                Description = device.Id.AsString(),
            }
        );
        Info.Add(
            new HeadlinedViewModel("type")
            {
                Icon = MaterialIconKind.MergeType,
                Header = RS.HomePageDeviceItem_Info_Type,
                Description = device.Id.DeviceClass,
            }
        );
        var linkInfo = new HeadlinedViewModel("link")
        {
            Icon = MaterialIconKind.Network,
            Header = RS.HomePageDeviceItem_Info_Link,
        };
        device
            .Link.State.Subscribe(x => linkInfo.Description = x.ToString("G"))
            .DisposeItWith(Disposable);
        Info.Add(linkInfo);
        Description = string.Format(
            RS.HomePageDeviceItem_Description,
            device.Id.DeviceClass,
            device.Id
        );
    }

    public IClientDevice Device { get; }
}
