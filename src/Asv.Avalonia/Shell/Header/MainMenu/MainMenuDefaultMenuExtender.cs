using System.Composition;
using Asv.Common;
using R3;

namespace Asv.Avalonia;

[ExportExtensionFor<IShell>]
[method: ImportingConstructor]
public class MainMenuDefaultMenuExtender(
    [ImportMany(ExportMainMenuAttribute.Contract)] IEnumerable<IMenuItem> items
) : IExtensionFor<IShell>
{
    public void Extend(IShell context, CompositeDisposable contextDispose)
    {
        context.MainMenu.AddRange(
            items.Select(x =>
            {
                if (x.ParentId != NavigationId.Empty)
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
