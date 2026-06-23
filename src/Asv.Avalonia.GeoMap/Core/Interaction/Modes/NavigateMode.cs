using Material.Icons;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class NavigateMode : IMapInteractionMode
{
    public static IMapInteractionMode Instance { get; } = new NavigateMode();

    private static readonly ReadOnlyReactiveProperty<string?> NoStatus =
        new ReactiveProperty<string?>(null);

    private NavigateMode() { }

    public string Title => "Navigate";

    public MaterialIconKind Icon => MaterialIconKind.CursorMove;

    public ReadOnlyReactiveProperty<string?> StatusText => NoStatus;

    public AsvColorKind Accent => AsvColorKind.Info5;

    public void Dispose() { }
}
