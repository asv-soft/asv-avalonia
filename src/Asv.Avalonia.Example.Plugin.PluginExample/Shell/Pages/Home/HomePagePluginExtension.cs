using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

public class HomePagePluginExtension : IExtensionFor<IHomePage>
{
    public const string StaticId = "ext.home.plugin-example";

    string ISupportId<string>.Id => StaticId;

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open-plugin-example")
        {
            Header = "Plugin example action",
            Description = "This is example action from plugin",
            Icon = ExamplePageViewModel.PageIcon,
            Command = new ReactiveCommand(_ =>
                context.GoTo(new NavPath(new NavId(ExamplePageViewModel.PageId)))
            ).DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);

        context.Tools.Add(action);
    }
}
