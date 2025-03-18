using System;
using Asv.Mavlink;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.Example;

public class BrowserItem : HeadlinedViewModel, IBrowserItem
{
    private string _path = string.Empty;
    private NavigationId _parentId = NavigationId.Empty;
    private FileSize? _size;
    private bool _hasChildren;
    private BindableReactiveProperty<bool> _isExpanded = new(false);
    private BindableReactiveProperty<bool> _isSelected = new(false);
    private BindableReactiveProperty<bool> _isInEditMode = new(false);
    private string _editedName = null!;
    private string? _crc32Hex;
    private SolidColorBrush _crc32Color = null!;
    private FtpEntryType _ftpEntryType;

    public BrowserItem(NavigationId id, NavigationId parentId, string path)
        : base(id)
    {
        ParentId = parentId;
        Path = path;
        Order = 0;
    }

    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
    }

    public NavigationId ParentId
    {
        get => _parentId;
        set => SetField(ref _parentId, value);
    }

    public FileSize? Size
    {
        get => _size;
        set => SetField(ref _size, value);
    }

    public bool HasChildren
    {
        get => _hasChildren;
        set => SetField(ref _hasChildren, value);
    }

    public BindableReactiveProperty<bool> IsExpanded
    {
        get => _isExpanded;
        set => SetField(ref _isExpanded, value);
    }

    public BindableReactiveProperty<bool> IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public BindableReactiveProperty<bool> IsInEditMode
    {
        get => _isInEditMode;
        set => SetField(ref _isInEditMode, value);
    }

    public string EditedName
    {
        get => _editedName;
        set => SetField(ref _editedName, value);
    }

    public string? Crc32Hex
    {
        get => _crc32Hex;
        set => SetField(ref _crc32Hex, value);
    }

    public SolidColorBrush Crc32Color
    {
        get => _crc32Color;
        set => SetField(ref _crc32Color, value);
    }

    public FtpEntryType FtpEntryType
    {
        get => _ftpEntryType;
        set => SetField(ref _ftpEntryType, value);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            IsExpanded.Dispose();
            IsSelected.Dispose();
            IsInEditMode.Dispose();
        }

        base.Dispose(disposing);
    }
}
