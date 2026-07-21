using Material.Icons;

namespace Asv.Avalonia;

/// <summary>
/// Describes a file type supported by an <see cref="IFileHandler"/>.
/// </summary>
public record FileTypeInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileTypeInfo"/> class.
    /// </summary>
    /// <param name="id">The identifier of the file type.</param>
    /// <param name="title">The human-readable title of the file type.</param>
    /// <param name="extension">
    /// The file name extension, typically without a leading period, for example <c>json</c>.
    /// </param>
    /// <param name="canOpen">
    /// A value indicating whether files of this type can be opened.
    /// </param>
    /// <param name="canCreate">
    /// A value indicating whether files of this type can be created.
    /// </param>
    /// <param name="icon">The optional icon associated with the file type.</param>
    public FileTypeInfo(
        string id,
        string title,
        string extension,
        bool canOpen,
        bool canCreate,
        MaterialIconKind? icon
    )
    {
        Id = id;
        Title = title;
        Extension = extension;
        CanOpen = canOpen;
        CanCreate = canCreate;
        Icon = icon;
    }

    /// <summary>
    /// Gets the identifier of the file type.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the human-readable title of the file type.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the file name extension, typically without a leading period.
    /// </summary>
    public string Extension { get; }

    /// <summary>
    /// Gets a value indicating whether files of this type can be opened.
    /// </summary>
    public bool CanOpen { get; }

    /// <summary>
    /// Gets a value indicating whether files of this type can be created.
    /// </summary>
    public bool CanCreate { get; }

    /// <summary>
    /// Gets the optional icon associated with the file type.
    /// </summary>
    public MaterialIconKind? Icon { get; }
}

/// <summary>
/// Defines an application-specific handler that can open and create one or more file types.
/// </summary>
public interface IFileHandler
{
    /// <summary>
    /// Gets the handler priority.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gets the file types supported by this handler.
    /// </summary>
    IEnumerable<FileTypeInfo> SupportedFiles { get; }

    /// <summary>
    /// Determines whether this handler can open the specified path.
    /// </summary>
    /// <param name="path">The file system path to inspect.</param>
    /// <returns>
    /// <see langword="true"/> when this handler can open the path; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    bool CanOpen(string path);

    /// <summary>
    /// Opens the specified file.
    /// </summary>
    /// <param name="path">The path of the file to open.</param>
    /// <param name="cancel">A token that cancels the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Open(string path, CancellationToken cancel = default);

    /// <summary>
    /// Creates a file of the specified type.
    /// </summary>
    /// <param name="path">The path at which to create the file.</param>
    /// <param name="type">The file type to create.</param>
    /// <param name="cancel">A token that cancels the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Create(string path, FileTypeInfo type, CancellationToken cancel = default);
}

/// <summary>
/// Defines application-level operations for querying supported file types and opening or creating files.
/// </summary>
public interface IFileAssociationService
{
    /// <summary>
    /// Gets the file types supported by the service.
    /// </summary>
    IEnumerable<FileTypeInfo> SupportedFiles { get; }

    /// <summary>
    /// Opens the specified file.
    /// </summary>
    /// <param name="path">The path of the file to open.</param>
    /// <param name="cancel">A token that cancels the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Open(string path, CancellationToken cancel = default);

    /// <summary>
    /// Creates a file of the specified type.
    /// </summary>
    /// <param name="path">The path at which to create the file.</param>
    /// <param name="type">The file type to create.</param>
    /// <param name="cancel">A token that cancels the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Create(string path, FileTypeInfo type, CancellationToken cancel = default);
}
