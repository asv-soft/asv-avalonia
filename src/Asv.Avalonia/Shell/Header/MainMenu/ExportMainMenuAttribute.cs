using System.Composition;
using Asv.Common;

namespace Asv.Avalonia;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportMainMenuAttribute() : ExportAttribute(Contract, typeof(IMenuItem))
{
    public const string Contract = "shell.menu.main";
}

[ExportExtensionFor<IShell>]
[method: ImportingConstructor]
public class MainMenuDefaultMenuExtender(
    [ImportMany(ExportMainMenuAttribute.Contract)] IEnumerable<IMenuItem> items
) : AsyncDisposableOnce, IExtensionFor<IShell>
{
    public void Extend(IShell context)
    {
        context.MainMenu.AddRange(items);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var item in items)
            {
                item.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
