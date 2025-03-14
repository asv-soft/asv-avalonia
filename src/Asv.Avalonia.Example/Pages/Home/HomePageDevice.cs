using System.Collections.Generic;
using System.Composition;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using R3;

namespace Asv.Avalonia.Example;

public class HomePageDevice : HomePageItem
{
    public HomePageDevice(IClientDevice device)
        : base(NavigationId.NormalizeTypeId(device.Id.AsString()))
    {
        Device = device;
        Icon = DeviceIconMixin.GetIcon(device.Id);
        IconBrush = DeviceIconMixin.GetIconBrush(device.Id);
        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        Info.Add(
            new HeadlinedViewModel("id")
            {
                Icon = MaterialIconKind.IdCard,
                Header = "Id",
                Description = device.Id.AsString(),
            }
        );
        Info.Add(
            new HeadlinedViewModel("type")
            {
                Icon = MaterialIconKind.MergeType,
                Header = "Type",
                Description = device.Id.DeviceClass,
            }
        );
        var linkInfo = new HeadlinedViewModel("link")
        {
            Icon = MaterialIconKind.Network,
            Header = "Link",
        };
        device
            .Link.State.Subscribe(x =>
            {
                linkInfo.Description = x.ToString("G");
            })
            .DisposeItWith(Disposable);
        Info.Add(linkInfo);
        Description = $"Device {device.Id.DeviceClass} with address {device.Id}";
    }

    public IClientDevice Device { get; }
}
