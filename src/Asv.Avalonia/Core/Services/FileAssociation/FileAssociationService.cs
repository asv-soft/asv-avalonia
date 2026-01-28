using System.Collections.Immutable;
using System.Composition;

namespace Asv.Avalonia;

[Export(typeof(IFileAssociationService))]
[Shared]
public class FileAssociationService : IFileAssociationService
{
    private readonly ImmutableArray<IFileHandler> _handlers;

    [ImportingConstructor]
    public FileAssociationService([ImportMany] IEnumerable<IFileHandler> handlers)
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

    public IEnumerable<FileTypeInfo> SupportedFiles => _handlers.SelectMany(x => x.SupportedFiles);

    public ValueTask Open(string path)
    {
        foreach (var handler in _handlers.Where(handler => handler.CanOpen(path)))
        {
            return handler.Open(path);
        }

        throw new NotSupportedException($"No handler found for file {path}");
    }

    public ValueTask Create(string path, FileTypeInfo type)
    {
        var handler = _handlers.FirstOrDefault(x => x.SupportedFiles.Any(y => y.Id == type.Id));
        if (handler == null)
        {
            throw new InvalidOperationException($"File type {type.Id} is not supported");
        }

        return handler.Create(path, type);
    }
}
