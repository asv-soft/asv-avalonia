using System.Composition.Hosting;

namespace Asv.Avalonia;

public interface IContainerHost
{
    CompositionHost Host { get; }
}
