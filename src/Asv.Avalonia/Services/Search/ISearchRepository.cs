namespace Asv.Avalonia;

/// <summary>
/// Defines the contract for a repository that manages searchable items.
/// This repository provides functionality to store, retrieve, and manage items
/// that can be used in search operations within the application.
/// </summary>
public interface ISearchRepository : IDisposable
{
    /// <summary>
    /// Gets the singleton instance of the repository.
    /// </summary>
    public static ISearchRepository Instance;

    /// <summary>
    /// Gets a value indicating whether checks if the repository instance has been created.
    /// Useful for diagnostics or advanced scenarios.
    /// </summary>
    public static bool IsInstanceCreated;

    /// <summary>
    /// Retrieves all searchable items in the repository.
    /// </summary>
    /// <returns>All searchable items.</returns>
    public IEnumerable<SearchableItem> GetItems();

    /// <summary>
    /// Clears all items in the repository.
    /// </summary>
    public void Clear();

    /// <summary>
    /// Searches for items in the repository based on specific criteria.
    /// The search can be filtered or sorted, depending on the implementation.
    /// </summary>
    /// <param name="query">
    /// The search query string used to match items.
    /// This may include partial matches or fuzzy search depending on the implementation.
    /// </param>
    /// <param name="typeFilter">
    /// An optional filter to restrict the search to a specific <see cref="SearchableItemType"/>.
    /// If null, the search includes all item types.
    /// </param>
    /// <returns>
    /// A collection of <see cref="SearchableItem"/> objects that match the search criteria.
    /// </returns>
    public IEnumerable<SearchableItem> Search(string query, SearchableItemType? typeFilter = null);

    /// <summary>
    /// Releases all resources used by the repository.
    /// </summary>
    public void Dispose();
}
