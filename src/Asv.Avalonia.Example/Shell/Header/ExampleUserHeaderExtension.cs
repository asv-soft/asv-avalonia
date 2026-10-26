using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.Example;

public sealed class ExampleUserHeaderExtension : IExtensionFor<IShell>
{
    public string Id => "example.shell.header.user";

    public void Extend(IShell context, CompositeDisposable contextDispose)
    {
        var user = new ExampleUserHeaderViewModel();
        context.HeaderItems.Add(user);
        Disposable.Create(() => context.HeaderItems.Remove(user)).DisposeItWith(contextDispose);
    }
}
