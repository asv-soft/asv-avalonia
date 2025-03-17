using Asv.Mavlink;

namespace Asv.Avalonia.Example;

public class FileItem : BrowserItem
{
    public FileItem(NavigationId id, NavigationId parentId, string? name, long size)
        : base(id, parentId)
    {
        HasChildren = false;
        Header = name;
        Size = new FileSize(size);
        FtpEntryType = FtpEntryType.File;
    }
}
