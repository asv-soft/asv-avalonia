using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia;

public class HomePageDefaultToolsExtension : IExtensionFor<IHomePage>
{
    public const string StaticId = "ext.home.tools";

    private readonly IEnumerable<IActionViewModel> _actions;
    public const string Contract = "home.tools";

    string Asv.Modeling.ISupportId<string>.Id => StaticId;

    public HomePageDefaultToolsExtension(
        [FromKeyedServices(Contract)] IEnumerable<IActionViewModel> actions
    )
    {
        _actions = actions;
    }

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        foreach (var action in _actions)
        {
            context.Tools.Add(action);
            action.DisposeItWith(contextDispose);
        }
    }
}
