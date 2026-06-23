using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class MapModeExtension<TMode> : IExtensionFor<IMap>
    where TMode : IMapInteractionMode, new()
{
    public void Extend(IMap context, CompositeDisposable contextDispose) =>
        context.Modes.Add(new TMode());
}
