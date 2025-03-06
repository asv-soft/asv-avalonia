using Asv.IO;
using Asv.Mavlink;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePageItem>]
public class HomePageFileBrowserExtension : HomePageDeviceAction
{
    protected override IActionViewModel? TryCreateAction(
        IClientDevice device,
        HomePageDevice context
    )
    {
        if (device.GetMicroservice<IFtpClient>() == null)
        {
            return null;
        }

        return new ActionViewModel("ftp")
        {
            Header = OpenFileBrowserCommand.StaticInfo.Name,
            Description = OpenFileBrowserCommand.StaticInfo.Description,
            Icon = OpenFileBrowserCommand.StaticInfo.Icon,
            Command = new BindableAsyncCommand(OpenFileBrowserCommand.Id, context),
            CommandParameter = new Persistable<IClientDevice>(device),
        };
    }
}
