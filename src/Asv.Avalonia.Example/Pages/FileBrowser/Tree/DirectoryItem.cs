using Asv.Mavlink;

namespace Asv.Avalonia.Example;

public class DirectoryItem : BrowserItem
{
    public DirectoryItem(NavigationId id, NavigationId parentId, string? name)
        : base(id, parentId)
    {
        HasChildren = true;
        Header = name;
        FtpEntryType = FtpEntryType.File;
    }
}
