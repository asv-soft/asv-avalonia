using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia;

public class HomePageDefaultToolsExtension : IExtensionFor<IHomePage>
{
    private readonly IEnumerable<IActionViewModel> _actions;
    public const string Contract = "home.tools";
    public HomePageDefaultToolsExtension([FromKeyedServices(Contract)] IEnumerable<IActionViewModel> actions)
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