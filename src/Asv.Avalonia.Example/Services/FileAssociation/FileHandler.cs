using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Material.Icons;

namespace Asv.Avalonia.Example;

[Export(typeof(IFileHandler))]
[Shared]
public class FileHandler : IFileHandler
{
    private static readonly FileTypeInfo[] StaticTypes =
    [
        new("test", "Test file", "json", true, true, MaterialIconKind.AboutCircle),
        new("test2", "Record file", "rec", true, true, MaterialIconKind.Record),
    ];

    [ImportingConstructor]
    public FileHandler() { }

    public int Priority => 0;
    public IEnumerable<FileTypeInfo> SupportedFiles => StaticTypes;

    public bool CanOpen(string path)
    {
        return true;
    }

    public ValueTask Open(string path)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Create(string path, FileTypeInfo type)
    {
        return ValueTask.CompletedTask;
    }
}
