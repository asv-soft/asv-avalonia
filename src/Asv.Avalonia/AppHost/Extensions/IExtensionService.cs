using R3;

namespace Asv.Avalonia;

/// <summary>
/// Resolves and applies registered <see cref="IExtensionFor{TInterface}"/> implementations
/// to a target instance. Used by <see cref="ExtendableViewModel{TSelfInterface}"/> to load extensions from DI.
/// </summary>
public interface IExtensionService
{
    /// <summary>
    /// Applies extensions for <typeparamref name="TInterface"/> to the specified <paramref name="owner"/>.
    /// </summary>
    /// <param name="owner">The target object to extend.</param>
    /// <param name="ownerKey">Key used to resolve keyed extensions from the DI container in addition to non-keyed ones.</param>
    /// <param name="ownerDisposable">Disposable collection tied to the owner's lifetime. Extensions and their disposables are registered here.</param>
    /// <typeparam name="TInterface">Type being extended.</typeparam>
    void Extend<TInterface>(TInterface owner, string ownerKey, CompositeDisposable ownerDisposable);
}
