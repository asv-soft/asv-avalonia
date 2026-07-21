namespace Asv.Avalonia;

public class NullFileAssociationService : IFileAssociationService
{
    public static IFileAssociationService Instance { get; } = new NullFileAssociationService();
    public IEnumerable<FileTypeInfo> SupportedFiles { get; } = [];

    public ValueTask Open(string path, CancellationToken cancel = default)
    {
        // do nothing
        return ValueTask.CompletedTask;
    }

    public ValueTask Create(string path, FileTypeInfo type, CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }
}
