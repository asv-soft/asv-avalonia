using Material.Icons;

namespace Asv.Avalonia;

public record FileTypeInfo
{
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

    public string Id { get; }
    public string Title { get; }
    public string Extension { get; }
    public bool CanOpen { get; }
    public bool CanCreate { get; }
    public MaterialIconKind? Icon { get; }
}

public interface IFileHandler
{
    int Priority { get; }
    IEnumerable<FileTypeInfo> SupportedFiles { get; }
    bool CanOpen(string path);
    ValueTask Open(string path);
    ValueTask Create(string path, FileTypeInfo type);
}

public interface IFileAssociationService
{
    IEnumerable<FileTypeInfo> SupportedFiles { get; }
    ValueTask Open(string path);
    ValueTask Create(string path, FileTypeInfo type);
}
