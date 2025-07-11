﻿using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class DirectoryItem : BrowserItem
{
    public DirectoryItem(
        NavigationId id,
        string? parentPath,
        string path,
        string? name,
        ILoggerFactory loggerFactory
    )
        : base(id, parentPath, path, loggerFactory)
    {
        HasChildren = true;
        Header = name;
        FtpEntryType = FtpEntryType.Directory;
    }
}
