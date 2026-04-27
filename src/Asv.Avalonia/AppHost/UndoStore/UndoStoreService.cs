using Asv.Modeling;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text;

namespace Asv.Avalonia;

public class UndoStoreServiceOptions
{
    public const string SectionName = "Undo";
    public string Folder { get; set; } = "undo";
}

public class UndoStoreService : IUndoStoreService
{
    private readonly string _baseFolder;

    public UndoStoreService(IOptions<UndoStoreServiceOptions> options, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);

        var folder = options.Value.Folder;
        if (string.IsNullOrWhiteSpace(folder))
        {
            folder = UndoStoreServiceOptions.SectionName.ToLowerInvariant();
        }

        _baseFolder = Path.Combine(environment.ContentRootPath, folder);
        Directory.CreateDirectory(_baseFolder);
    }

    public IUndoHistoryStore CreateUndoHistoryStore(NavId ownerId)
    {
        var ownerFolder = Path.Combine(_baseFolder, CreateFolderName(ownerId));
        Directory.CreateDirectory(ownerFolder);
        return new JsonUndoHistoryStore(ownerFolder);
    }

    internal static string CreateFolderName(NavId ownerId)
    {
        return $"nav_{EscapePathSegment(ownerId.ToString().ToLowerInvariant())}";
    }

    internal static string EscapePathSegment(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length == 0)
        {
            return "_empty";
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (
                (ch >= 'a' && ch <= 'z')
                || (ch >= '0' && ch <= '9')
                || ch is '.' or '-'
            )
            {
                builder.Append(ch);
                continue;
            }

            builder.Append('_');
            builder.Append(((int)ch).ToString("X4"));
        }

        return builder.ToString();
    }
}
