using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class DeviceActionExample(ILoggerFactory loggerFactory) : IExtensionFor<IHomePageItem>
{
    public void Extend(IHomePageItem context, R3.CompositeDisposable contextDispose)
    {
        context.Actions.Add(OpenDebugWindowCommand.StaticInfo.CreateAction(loggerFactory));
    }
}
