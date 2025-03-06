using System;
using System.Collections.Generic;

namespace Asv.Avalonia.Example;

public sealed class BrowserItemComparer : IComparer<IBrowserItem>
{
    public static readonly BrowserItemComparer Instance = new();

    private BrowserItemComparer() { }

    public int Compare(IBrowserItem? x, IBrowserItem? y)
    {
        switch (x)
        {
            case null when y == null:
                return 0;
            case null:
                return -1;
        }

        if (y == null)
        {
            return 1;
        }

        var pathComparison = string.CompareOrdinal(((IViewModel)x).Id.Id, ((IViewModel)y).Id.Id);
        if (pathComparison != 0)
        {
            return pathComparison;
        }

        var parentPathComparison = string.CompareOrdinal(x.ParentId.Id, y.ParentId.Id);
        if (parentPathComparison != 0)
        {
            return parentPathComparison;
        }

        if (x.Size.HasValue && y.Size.HasValue)
        {
            return x.Size.Value.CompareTo(y.Size.Value);
        }

        if (x.Size.HasValue)
        {
            return 1;
        }

        if (y.Size.HasValue)
        {
            return -1;
        }

        return 0;
    }
}
