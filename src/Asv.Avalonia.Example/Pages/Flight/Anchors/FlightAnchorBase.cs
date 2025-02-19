using Asv.IO;
using Asv.Mavlink;

namespace Asv.Avalonia.Example;

public class FlightAnchorBase
{
    public FlightAnchorBase(VehicleClientDevice vehicle, string name) // TODO: унаследоваться от якоря на карте
    {
        Vehicle = vehicle;
    }

    public VehicleClientDevice Vehicle { get; }
}
