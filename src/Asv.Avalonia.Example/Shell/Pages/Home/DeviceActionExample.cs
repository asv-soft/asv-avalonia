using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia.Example;

public class DeviceActionExample : IExtensionFor<IHomePageItem>
{
    public const string StaticId = "ext.home.device-action.debug";

    string ISupportId<string>.Id => StaticId;

    public void Extend(IHomePageItem context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("debug")
        {
            Header = "Debug",
            Icon = MaterialIconKind.Bug,
        }.DisposeItWith(contextDispose);

        context.Actions.Add(action);
    }
}
