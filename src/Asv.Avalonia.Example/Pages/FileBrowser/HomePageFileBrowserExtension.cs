using System.Composition;
using Asv.Avalonia.IO;
using Asv.IO;
using Asv.Mavlink;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePageItem>]
[method: ImportingConstructor]
public class HomePageFileBrowserExtension(IFtpService ftp) : HomePageDeviceItemAction
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

        return new ActionViewModel("browser")
        {
            Header = OpenFileBrowserCommand.StaticInfo.Name,
            Description = OpenFileBrowserCommand.StaticInfo.Description,
            Icon = OpenFileBrowserCommand.StaticInfo.Icon,
            Command = new BindableAsyncCommand(OpenFileBrowserCommand.Id, context),
            CommandParameter = new StringCommandArg(device.Id.AsString()),
        };
    }
}
