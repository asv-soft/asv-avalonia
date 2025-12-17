using System.Composition;
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
    void Extend(T context, DisposableBag contextDispose);
}

/// <summary>
/// Marks a class as an exported extension for a specific type <typeparamref name="T"/>.
/// This attribute is used with the Managed Extensibility Framework (MEF2) to enable automatic
/// discovery and composition of extensions.
/// </summary>
/// <typeparam name="T">The type that this extension applies to.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportExtensionForAttribute<T> : ExportAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExportExtensionForAttribute{T}"/> class.
    /// This attribute allows MEF2 to recognize and register implementations of <see cref="IExtensionFor{T}"/>.
    /// </summary>
    public ExportExtensionForAttribute()
        : base(typeof(IExtensionFor<T>)) { }
}
