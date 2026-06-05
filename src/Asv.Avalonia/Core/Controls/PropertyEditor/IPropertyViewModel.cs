using Material.Icons;

namespace Asv.Avalonia;

public interface IPropertyViewModel : IHeadlinedViewModel
{
    HashSet<string> DisplayScopes { get; }
}
