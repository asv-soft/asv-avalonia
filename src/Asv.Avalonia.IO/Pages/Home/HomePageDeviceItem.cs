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
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
        : base(NavigationId.NormalizeTypeId(device.Id.AsString()), layoutService, loggerFactory)
    {
        Device = device;
        Icon = deviceManager.GetIcon(device.Id);
        IconBrush = deviceManager.GetDeviceBrush(device.Id);
        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        Info.Add(
            new HeadlinedViewModel("id", NullLayoutService.Instance, DesignTime.LoggerFactory)
            {
                Icon = MaterialIconKind.IdCard,
                Header = RS.HomePageDeviceItem_Info_Id,
                Description = device.Id.AsString(),
            }
        );
        Info.Add(
            new HeadlinedViewModel("type", NullLayoutService.Instance, DesignTime.LoggerFactory)
            {
                Icon = MaterialIconKind.MergeType,
                Header = RS.HomePageDeviceItem_Info_Type,
                Description = device.Id.DeviceClass,
            }
        );
        var linkInfo = new HeadlinedViewModel(
            "link",
            NullLayoutService.Instance,
            DesignTime.LoggerFactory
        )
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
