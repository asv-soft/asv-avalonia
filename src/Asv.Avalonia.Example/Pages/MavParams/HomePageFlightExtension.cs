﻿using System.Composition;
using Asv.Avalonia.IO;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePageItem>]
public class HomePageParamsDeviceItemAction : HomePageDeviceItemAction
{
    protected override IActionViewModel? TryCreateAction(
        IClientDevice device,
        HomePageDeviceItem context
    )
    {
        if (device.GetMicroservice<IParamsClientEx>() == null)
        {
            return null;
        }

        return new ActionViewModel("params")
        {
            Icon = MaterialIconKind.CogTransferOutline,
            Header = "Params editor",
            Description = "Edit mavlink device parameters",
            Command = new BindableAsyncCommand(OpenMavParamsCommand.Id, context),
            CommandParameter = new StringCommandArg(device.Id.AsString()),
        };
    }
}
