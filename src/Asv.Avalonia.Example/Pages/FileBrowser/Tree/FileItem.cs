using Asv.Mavlink;

namespace Asv.Avalonia.Example;

public class FileItem : BrowserItem
{
    public FileItem(NavigationId id, NavigationId parentId, string path, string? name, long size)
        : base(id, parentId, path)
    {
        HasChildren = false;
        Header = name;
        Size = new FileSize(size);
        FtpEntryType = FtpEntryType.File;
    }
}
