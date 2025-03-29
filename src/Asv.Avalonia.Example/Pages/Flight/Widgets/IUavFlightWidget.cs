using Asv.Avalonia.Map;
using Asv.IO;

namespace Asv.Avalonia.Example;

public interface IUavFlightWidget : IMapWidget
{
    IClientDevice Device { get; }
}
