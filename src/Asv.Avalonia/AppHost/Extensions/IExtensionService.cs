using Asv.Common;
using R3;

namespace Asv.Avalonia;

public interface IExtensionService
{
    void Extend<TInterface>(TInterface owner, string ownerKey, CompositeDisposable ownerDisposable);
}
