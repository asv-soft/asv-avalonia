using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Defines a contract for an extension that can be applied to a specific type <typeparamref name="TContext"/>.
/// This interface allows modular enhancements to be dynamically applied to existing objects.
/// </summary>
/// <typeparam name="TContext">The type of object that the extension applies to.</typeparam>
public interface IExtensionFor<in TContext> : ISupportId<string>
{
    /// <summary>
    /// Applies the extension logic to the given context.
    /// </summary>
    /// <param name="context">The target object to extend.</param>
    /// <param name="contextDispose">Disposable collection, that disposed with context.</param>
    void Extend(TContext context, CompositeDisposable contextDispose);
}
