using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia.Example;

public class ShellLeftMenuExtenderExample : IExtensionFor<IShell>
{
    public const string StaticId = "ext.shell.left-menu.example";

    string ISupportId<string>.Id => StaticId;

    public void Extend(IShell context, CompositeDisposable contextDispose)
    {
        context.LeftMenu.Add(
            new MenuItem("home", RS.ShellLeftMenuExtenderExample_HomeItem_Header)
            {
                Icon = MaterialIconKind.Home,
                Command = new ReactiveCommand(_ =>
                    context.GoTo(new NavPath(new NavId(HomePageViewModel.PageId)))
                ).DisposeItWith(contextDispose),
            }.DisposeItWith(contextDispose)
        );
    }
}
