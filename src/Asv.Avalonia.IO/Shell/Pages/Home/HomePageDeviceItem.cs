using Asv.Common;
using Asv.IO;
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
        : base(NavigationId.NormalizeTypeId(device.Id.AsString()), loggerFactory, ext)
    {
        Device = device;
        Icon = deviceManager.GetIcon(device.Id);
        IconColor = deviceManager.GetDeviceColor(device.Id);
        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        Info.Add(
            new HeadlinedViewModel("id", loggerFactory)
            {
                Icon = MaterialIconKind.IdCard,
                Header = RS.HomePageDeviceItem_Info_Id,
                Description = device.Id.AsString(),
            }
        );
        Info.Add(
            new HeadlinedViewModel("type", loggerFactory)
            {
                Icon = MaterialIconKind.MergeType,
                Header = RS.HomePageDeviceItem_Info_Type,
                Description = device.Id.DeviceClass,
            }
        );
        var linkInfo = new HeadlinedViewModel("link", loggerFactory)
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
