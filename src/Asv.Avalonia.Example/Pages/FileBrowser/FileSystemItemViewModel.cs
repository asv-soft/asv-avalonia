/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Asv.Common;
using Asv.Mavlink;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

public class FileSystemItemViewModel : ObservableTree<IFtpEntry, string>
{
    public const string ViewModelId = "files.browser.item";

    public FileSystemItemViewModel()
        : base(ViewModelId)
    {
        DesignTime.ThrowIfNotDesignMode();
        IsExpanded = new BindableReactiveProperty<bool>();
        IsSelected = new BindableReactiveProperty<bool>();
        IsInEditMode = new BindableReactiveProperty<bool>();
        EditedName = new BindableReactiveProperty<string>();
        Crc32Hex = new BindableReactiveProperty<string?>();
        Crc32Color = new BindableReactiveProperty<SolidColorBrush>();
        Name = string.Empty;
        Path = string.Empty;
    }

    public FileSystemItemViewModel(
        string entry,
        bool isDirectory,
        FileSystemItemViewModel? parent = null
    )
        : base(ViewModelId)
    {
        IsExpanded = new BindableReactiveProperty<bool>();
        IsSelected = new BindableReactiveProperty<bool>();
        IsInEditMode = new BindableReactiveProperty<bool>();
        EditedName = new BindableReactiveProperty<string>();
        Crc32Hex = new BindableReactiveProperty<string?>();
        Crc32Color = new BindableReactiveProperty<SolidColorBrush>();

        Parent = parent;

        if (isDirectory)
        {
            Children = LoadItems(entry, this);
            IsDirectory = true;
        }
        else
        {
            Children = new ObservableList<FileSystemItemViewModel>([]);
            IsFile = true;
            Size = FileBrowserViewModel.ConvertBytesToReadableSize(new FileInfo(entry).Length);
        }

        Name = System.IO.Path.GetFileName(entry);
        EditedName = new BindableReactiveProperty<string>(Name);
        Path = entry;

        _sub1 = IsSelected.Where(b => !b).Subscribe(_ => IsInEditMode.OnNext(false));
        _sub2 = IsExpanded.Where(expanded => expanded).Subscribe(_ => OnExpanded());
        EndEditCommand = new ReactiveCommand(EndEditImpl);
        Crc32Color = new BindableReactiveProperty<SolidColorBrush>(DefaultColor);
    }

    public FileSystemItemViewModel(
        ObservableTreeNode<IFtpEntry, string> node,
        FileSystemItemViewModel? parent = null
    )
        : base(ViewModelId)
    {
        IsExpanded = new BindableReactiveProperty<bool>();
        IsSelected = new BindableReactiveProperty<bool>();
        IsInEditMode = new BindableReactiveProperty<bool>();
        EditedName = new BindableReactiveProperty<string>();
        Crc32Hex = new BindableReactiveProperty<string?>();
        Crc32Color = new BindableReactiveProperty<SolidColorBrush>();

        Parent = parent;
        node.Items.ForEach(n => Children.Add(new FileSystemItemViewModel(n, this)));
        Name = node.Base.Name;
        EditedName = new BindableReactiveProperty<string>(Name);
        Path = node.Base.Path;
        IsDirectory = node.Base.Type is FtpEntryType.Directory;
        IsFile = node.Base.Type is FtpEntryType.File;

        if (IsFile)
        {
            Size = FileBrowserViewModel.ConvertBytesToReadableSize(((FtpFile)node.Base).Size);
        }

        _sub1 = IsSelected.Where(b => !b).Subscribe(_ => IsInEditMode.OnNext(false));
        _sub2 = IsExpanded.Where(expanded => expanded).Subscribe(_ => OnExpanded());
        EndEditCommand = new ReactiveCommand(EndEditImpl);
        Crc32Color = new BindableReactiveProperty<SolidColorBrush>(DefaultColor);
    }

    public ReactiveCommand EndEditCommand { get; set; } = null!;
    public ObservableList<FileSystemItemViewModel> Children { get; } = [];
    public FileSystemItemViewModel? Parent { get; }
    public string Name { get; set; }
    public string Path { get; set; }
    public bool IsDirectory { get; set; }
    public bool IsFile { get; set; }
    public string? Size { get; set; }
    public bool IsEditingSuccess { get; set; }

    public BindableReactiveProperty<bool> IsExpanded { get; set; }

    public BindableReactiveProperty<bool> IsSelected { get; set; }

    public BindableReactiveProperty<bool> IsInEditMode { get; set; }

    public BindableReactiveProperty<string> EditedName { get; set; }

    public BindableReactiveProperty<string?> Crc32Hex { get; set; }

    public BindableReactiveProperty<SolidColorBrush> Crc32Color { get; set; }

    public static SolidColorBrush DefaultColor =>
        Application.Current?.FindResource("TextControlBackgroundPointerOver") as SolidColorBrush
        ?? SolidColorBrush.Parse("#7E8C7E");
    public static SolidColorBrush BadCrcColor =>
        Application.Current?.FindResource("SystemAccentColorDark3") as SolidColorBrush
        ?? SolidColorBrush.Parse("#8B0000");
    public static SolidColorBrush GoodCrcColor =>
        Application.Current?.FindResource("SystemAccentColorLight1") as SolidColorBrush
        ?? SolidColorBrush.Parse("#006400");

    private void OnExpanded()
    {
        IsSelected.OnNext(true);
    }

    private static ObservableList<FileSystemItemViewModel> LoadItems(
        string path,
        FileSystemItemViewModel? parent
    )
    {
        var items = new ObservableList<FileSystemItemViewModel>();

        foreach (var directory in Directory.GetDirectories(path))
        {
            items.Add(new FileSystemItemViewModel(directory, true, parent));
        }

        foreach (var file in Directory.GetFiles(path))
        {
            items.Add(new FileSystemItemViewModel(file, false, parent));
        }

        return new ObservableList<FileSystemItemViewModel>(items);
    }

    private void EndEditImpl(Unit unit)
    {
        IsInEditMode = new BindableReactiveProperty<bool>(false);
        if (Name != EditedName.Value)
        {
            IsEditingSuccess = true;
        }
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();

            Parent?.Dispose();
            Children.Clear();
            EndEditCommand.Dispose();

            IsExpanded.Dispose();
            IsSelected.Dispose();
            IsInEditMode.Dispose();
            EditedName.Dispose();
            Crc32Hex.Dispose();
            Crc32Color.Dispose();
        }

        base.Dispose(isDisposing);
    }

    #endregion
}
*/
