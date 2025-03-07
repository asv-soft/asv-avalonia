using R3;

namespace Asv.Avalonia;

/// <summary>
/// Represents a routable view model that supports navigation, hierarchical structure, and event propagation.
/// This interface extends <see cref="IViewModel"/> to include routing-related functionalities.
/// </summary>
public interface IRoutable : IViewModel
{
    /// <summary>
    /// Gets or sets the parent <see cref="IRoutable"/> in the navigation hierarchy.
    /// </summary>
    IRoutable? Parent { get; set; }

    /// <summary>
    /// Raises an asynchronous routed event within the navigation hierarchy.
    /// This allows child elements to propagate events upward through their parents.
    /// </summary>
    /// <param name="e">The routed event to be raised.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Rise(AsyncRoutedEvent e);

    /// <summary>
    /// Navigates to a child routable element based on its identifier.
    /// This method is used to locate and return a routable child.
    /// </summary>
    /// <param name="id">The unique identifier of the target routable.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation,
    /// returning the found <see cref="IRoutable"/>.
    /// </returns>
    ValueTask<IRoutable> Navigate(NavigationId id);

    /// <summary>
    /// Retrieves all direct child elements of the current <see cref="IRoutable"/>.
    /// </summary>
    /// <returns>An enumerable collection of child <see cref="IRoutable"/> elements.</returns>
    IEnumerable<IRoutable> GetRoutableChildren();
}

/// <summary>
/// Represents an asynchronous routed event that propagates through a hierarchical structure of <see cref="IRoutable"/> components.
/// This event supports different routing strategies such as bubbling, tunneling, and direct invocation.
/// </summary>
public abstract class AsyncRoutedEvent(
    IRoutable source,
    RoutingStrategy routingStrategy = RoutingStrategy.Bubble
)
{
    /// <summary>
    /// Gets or sets the routing strategy used for this event.
    /// The strategy determines how the event propagates through the hierarchy.
    /// </summary>
    public RoutingStrategy RoutingStrategy { get; set; } = routingStrategy;

    /// <summary>
    /// Gets the source <see cref="IRoutable"/> that initially raised the event.
    /// </summary>
    public IRoutable Source { get; } = source;

    /// <summary>
    /// Gets or sets a value indicating whether the event has been handled.
    /// If set to <c>true</c>, further propagation of the event may be stopped.
    /// </summary>
    public bool IsHandled { get; set; }

    /// <summary>
    /// Creates a shallow copy of the current <see cref="AsyncRoutedEvent"/> instance.
    /// </summary>
    /// <returns>A cloned instance of the current event.</returns>
    public virtual AsyncRoutedEvent Clone()
    {
        return (AsyncRoutedEvent)MemberwiseClone();
    }
}

/// <summary>
/// Specifies the strategy used to propagate a routed event in the <see cref="IRoutable"/> hierarchy.
/// </summary>
public enum RoutingStrategy
{
    /// <summary>
    /// The event starts at the source and propagates upwards through the parent chain.
    /// </summary>
    Bubble,

    /// <summary>
    /// The event starts from the root and propagates down to the source.
    /// </summary>
    Tunnel,

    /// <summary>
    /// The event is only handled at the source and does not propagate.
    /// </summary>
    Direct,
}
