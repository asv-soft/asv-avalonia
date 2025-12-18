using Asv.Common;

namespace Asv.Avalonia;

public abstract class AsyncRoutedEventWithRestrictionsBase(
    IRoutable source,
    RoutingStrategy routingStrategy
) : AsyncRoutedEvent<IRoutable>(source, routingStrategy)
{
    private readonly List<Restriction> _restrictions = [];

    /// <summary>
    /// Gets the list of restrictions.
    /// </summary>
    public IReadOnlyList<Restriction> Restrictions => _restrictions;

    /// <summary>
    /// Adds a restriction.
    /// </summary>
    /// <param name="restriction">The restriction.</param>
    public void AddRestriction(Restriction restriction)
    {
        _restrictions.Add(restriction);
    }

    /// <summary>
    /// Adds a restriction from source.
    /// </summary>
    /// <param name="source">Source of the restriction that has type <see cref="IRoutable"/>.</param>
    public void AddRestriction(IRoutable source)
    {
        _restrictions.Add(new Restriction(source));
    }

    /// <summary>
    /// Adds many restrictions.
    /// </summary>
    /// <param name="restrictions">Restrictions to add.</param>
    public void AddRestrictions(IEnumerable<Restriction> restrictions)
    {
        _restrictions.AddRange(restrictions);
    }

    /// <summary>
    /// Add many restrictions from sources.
    /// </summary>
    /// <param name="sources">Sources of the restrictions to add that have type <see cref="IRoutable"/>.</param>
    public void AddRestrictions(IEnumerable<IRoutable> sources)
    {
        _restrictions.AddRange(sources.Select(r => new Restriction(r)));
    }
}
