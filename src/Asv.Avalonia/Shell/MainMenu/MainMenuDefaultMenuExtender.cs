using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia;

public class MainMenuDefaultMenuExtender(
    [FromKeyedServices(MainMenuDefaultMenuExtender.Contract)] IEnumerable<IMenuItem> items
) : IExtensionFor<IShell>
{
    public const string Contract = "shell.menu.main";

    public void Extend(IShell context, CompositeDisposable contextDispose)
    {
        context.MainMenu.AddRange(
            items.Select(x =>
            {
                if (x.ParentId != NavId.Empty)
                {
                    var parent = items.FirstOrDefault(item => item.Id == x.ParentId);

                    if (parent is not null)
                    {
                        x.SetRoutableParent(parent);
                    }
                }
                else
                {
                    x.SetRoutableParent(context);
                }

                x.DisposeItWith(contextDispose);
                return x;
            })
        );
    }
}
