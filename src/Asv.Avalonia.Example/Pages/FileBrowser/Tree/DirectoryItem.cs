using Asv.Mavlink;

namespace Asv.Avalonia.Example;

public class DirectoryItem : BrowserItem
{
    public DirectoryItem(NavigationId id, NavigationId parentId, string path, string? name)
        : base(id, parentId, path)
    {
        HasChildren = true;
        Header = name;
        FtpEntryType = FtpEntryType.Directory;
    }
}
