using R3;

namespace Asv.Avalonia;

public interface IPage : IRoutable
{
    public IReadOnlyBindableReactiveProperty<string> Title { get; }
}
