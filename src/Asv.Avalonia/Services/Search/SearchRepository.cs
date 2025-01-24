using System.Reflection;
using FuzzySharp;

namespace Asv.Avalonia;

/// <inheritdoc cref="ISearchRepository" />
public sealed class SearchRepository : ISearchRepository
{
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<SearchRepository> _instance = new(() =>
    {
        var repository = new SearchRepository();
        repository.Initialize();
        return repository;
    });

    /// <inheritdoc cref="ISearchRepository.Instance" />
    public static SearchRepository Instance => _instance.Value;

    /// <inheritdoc cref="ISearchRepository.IsInstanceCreated" />
    public static bool IsInstanceCreated => _instance.IsValueCreated;

    private readonly List<SearchableItem> _items = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchRepository"/> class.
    /// Private constructor to enforce the singleton pattern.
    /// Automatically populates the repository from attributes in the executing assembly.
    /// </summary>
    private SearchRepository() { }

    /// <summary>
    /// Adds a searchable item to the repository.
    /// </summary>
    /// <param name="item">The searchable item to add.</param>
    private void AddItem(SearchableItem item)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(SearchRepository));
        _items.Add(item);
    }

    /// <inheritdoc cref="ISearchRepository.GetItems" />
    public IEnumerable<SearchableItem> GetItems()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(SearchRepository));
        return _items;
    }

    /// <inheritdoc cref="ISearchRepository.Clear" />
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(SearchRepository));
        _items.Clear();
    }

    /// <inheritdoc cref="ISearchRepository.Search" />
    public IEnumerable<SearchableItem> Search(string query, SearchableItemType? typeFilter = null)
    {
        var items = GetItems();

        if (typeFilter.HasValue)
        {
            items = items.Where(i => i.Type == typeFilter.Value);
        }

        return items
            .Select(item => new { Item = item, Score = Fuzz.PartialRatio(query, item.Name) })
            .Where(x => x.Score > 50)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Item);
    }

    /// <summary>
    /// Scans the specified assembly and adds items annotated with the SearchableAttribute.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    private void AddItemsFromAttributes(Assembly assembly)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(SearchRepository));
        try
        {
            var typesWithAttribute = assembly
                .GetTypes()
                .Where(t => t.IsClass && t.GetCustomAttribute<SearchableAttribute>() != null);

            foreach (var type in typesWithAttribute)
            {
                var attribute = type.GetCustomAttribute<SearchableAttribute>();

                if (attribute == null)
                {
                    continue;
                }

                AddItem(
                    new SearchableItem
                    {
                        Id = attribute.Id,
                        Name = attribute.Name,
                        Description = attribute.Description,
                        Type = attribute.Type,
                    }
                );

                // Debug: Print added item for verification
                Console.WriteLine($"Added item from type: {type.Name} (Id: {attribute.Id})");
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Handle types that couldn't be loaded (e.g., due to missing dependencies)
            Console.WriteLine(
                $"Failed to load types from assembly {assembly.FullName}: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// Scans all loaded assemblies and adds items annotated with the SearchableAttribute.
    /// </summary>
    private void Initialize()
    {
        // Get all assemblies in the current AppDomain
        var assemblies = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(assembly =>
                assembly is { IsDynamic: false, FullName: not null }
                && !assembly.FullName.StartsWith("System")
                && !assembly.FullName.StartsWith("Microsoft")
                && !assembly.FullName.StartsWith("Avalonia")
            )
            .ToList();

        foreach (var assembly in assemblies)
        {
            AddItemsFromAttributes(assembly);
            Console.WriteLine($"Assembly listed: {assembly.FullName}");
        }
    }

    // Flag to track whether the object has been disposed
    private bool _disposed;

    /// <inheritdoc cref="ISearchRepository.Dispose" />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }
}
