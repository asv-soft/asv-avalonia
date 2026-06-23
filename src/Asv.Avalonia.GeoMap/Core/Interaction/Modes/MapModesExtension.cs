using Asv.Modeling;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class MapModesExtension : IExtensionFor<IMap>
{
    public const string StaticId = "ext.map-modes";

    string ISupportId<string>.Id => StaticId;

    public void Extend(IMap context, CompositeDisposable contextDispose)
    {
        context.Interaction.AddMode(new PointInputMode());
    }
}
