using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class RulerModeExtension(IUnitService unitService) : IExtensionFor<IMap>
{
    public void Extend(IMap context, CompositeDisposable contextDispose) =>
        context.Modes.Add(
            new RulerMode(unitService.GetRequiredUnitOfType<DistanceUnit>(DistanceUnit.Id))
        );
}
