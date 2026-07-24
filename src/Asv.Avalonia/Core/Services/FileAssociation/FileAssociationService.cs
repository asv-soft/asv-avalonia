using System.Collections.Immutable;

namespace Asv.Avalonia;

/// <summary>
/// Provides the default application-level dispatcher for registered <see cref="IFileHandler"/>
/// implementations.
/// </summary>
/// <remarks>
/// Handlers are evaluated in ascending <see cref="IFileHandler.Priority"/> order when opening a
/// file. File type identifiers must be unique across all registered handlers.
/// </remarks>
public class FileAssociationService : IFileAssociationService
{
    private readonly ImmutableArray<IFileHandler> _handlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAssociationService"/> class.
    /// </summary>
    /// <param name="handlers">The handlers registered in the dependency injection container.</param>
    /// <exception cref="InvalidOperationException">
    /// Two or more handlers advertise the same <see cref="FileTypeInfo.Id"/>.
    /// </exception>
    public FileAssociationService(IEnumerable<IFileHandler> handlers)
    {
        _handlers = [.. handlers.OrderBy(x => x.Priority)];

        // check file id is unique
        var differentId = _handlers
            .SelectMany(x => x.SupportedFiles)
            .GroupBy(x => x.Id)
            .FirstOrDefault(x => x.Count() > 1);
        if (differentId != null)
        {
            throw new InvalidOperationException(
                $"File handlers have non-unique id {differentId.Key}"
            );
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// File types are returned in ascending handler priority order.
    /// </remarks>
    public IEnumerable<FileTypeInfo> SupportedFiles => _handlers.SelectMany(x => x.SupportedFiles);

    /// <inheritdoc />
    /// <remarks>
    /// Handlers are evaluated in ascending priority order. The first handler for which
    /// <see cref="IFileHandler.CanOpen"/> returns <see langword="true"/> opens the file.
    /// </remarks>
    /// <exception cref="NotSupportedException">No registered handler can open the path.</exception>
    public ValueTask Open(string path, CancellationToken cancel = default)
    {
        foreach (var handler in _handlers.Where(handler => handler.CanOpen(path)))
        {
            return handler.Open(path, cancel);
        }

        throw new NotSupportedException($"No handler found for file {path}");
    }

    /// <inheritdoc />
    /// <remarks>
    /// The handler is selected by matching <see cref="FileTypeInfo.Id"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// No registered handler supports the specified file type identifier.
    /// </exception>
    public ValueTask Create(string path, FileTypeInfo type, CancellationToken cancel = default)
    {
        var handler = _handlers.FirstOrDefault(x => x.SupportedFiles.Any(y => y.Id == type.Id));
        if (handler == null)
        {
            throw new InvalidOperationException($"File type {type.Id} is not supported");
        }

        return handler.Create(path, type, cancel);
    }
}
