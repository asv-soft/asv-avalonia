using R3;

namespace Asv.Avalonia;

/// <summary>
/// Defines a contract for an extension that can be applied to a specific type <typeparamref name="T"/>.
/// This interface allows modular enhancements to be dynamically applied to existing objects.
/// </summary>
/// <typeparam name="T">The type of object that the extension applies to.</typeparam>
public interface IExtensionFor<in T>
{
    /// <summary>
    /// Applies the extension logic to the given context.
    /// </summary>
    /// <param name="context">The target object to extend.</param>
    /// <param name="contextDispose">Disposable collection, that disposed with context.</param>
    void Extend(T context, CompositeDisposable contextDispose);
}
