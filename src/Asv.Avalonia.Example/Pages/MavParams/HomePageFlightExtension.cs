using System.Composition;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePageItem>]
public class HomePageParamsDeviceAction : HomePageDeviceAction
{
    protected override IActionViewModel? TryCreateAction(
        IClientDevice device,
        HomePageDevice context
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
            CommandParameter = new Persistable<string>(device.Id.AsString()),
        };
    }
}
