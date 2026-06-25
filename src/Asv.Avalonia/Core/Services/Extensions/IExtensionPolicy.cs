using Asv.Modeling;

namespace Asv.Avalonia;

/// <summary>
/// Defines a policy that can inspect and transform the set of extensions before they are
/// applied to a context.
/// </summary>
/// <typeparam name="TContext">The type of object being extended.</typeparam>
/// <remarks>
/// Policies are evaluated by <see cref="IExtensionService"/> before
/// <see cref="IExtensionFor{TContext}.Extend"/> is called. A policy can disable extensions,
/// replace them with other registered extensions, or change their order. Implementations should
/// not call <see cref="IExtensionFor{TContext}.Extend"/> directly.
/// </remarks>
public interface IExtensionPolicyFor<TContext> : ISupportOrder, ISupportId<string>
{
    /// <summary>
    /// Filters, reorders, or replaces extensions for the specified context.
    /// </summary>
    /// <param name="context">The target object that will receive the extensions.</param>
    /// <param name="extensions">The extensions resolved for the target object.</param>
    /// <returns>The extensions that should be applied to the target object.</returns>
    IEnumerable<IExtensionFor<TContext>> Filter(
        TContext context,
        IEnumerable<IExtensionFor<TContext>> extensions
    );
}
