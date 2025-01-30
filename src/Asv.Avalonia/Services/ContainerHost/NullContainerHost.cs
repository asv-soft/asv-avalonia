using System.Composition.Hosting;

namespace Asv.Avalonia;

public class NullContainerHost : IContainerHost
{
    public static IContainerHost Instance { get; } = new NullContainerHost();
    public CompositionHost Host { get; } = CompositionHost.CreateCompositionHost();
}
