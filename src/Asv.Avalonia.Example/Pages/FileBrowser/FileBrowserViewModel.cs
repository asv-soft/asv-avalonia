using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Mavlink;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

[ExportPage(PageId)]
public class FileBrowserViewModel : PageViewModel<FileBrowserViewModel>
{
    public const string PageId = "files.browser";
    public const MaterialIconKind PageIcon = MaterialIconKind.FolderEye;

    private readonly IFtpClient _client;
    private readonly FtpClientEx _clientEx;
    private readonly string _localRootPath;

    private readonly ObservableList<IBrowserItem> _localSource;
    private readonly ObservableList<IBrowserItem> _remoteSource;

    public FileBrowserViewModel()
        : base(string.Empty, DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
        _localSource =
        [
            new DirectoryItem("0", NavigationId.Empty, "test"),
            new DirectoryItem("1", "0", "test"),
            new DirectoryItem("2", "0", "test"),
            new DirectoryItem("0_1", NavigationId.Empty, "test"),
            new DirectoryItem("0_2", NavigationId.Empty, "test"),
        ];
        _remoteSource =
        [
            new DirectoryItem("0", NavigationId.Empty, "test"),
            new DirectoryItem("1", "0", "test"),
            new DirectoryItem("2", "0", "test"),
            new DirectoryItem("0_1", NavigationId.Empty, "test"),
            new DirectoryItem("0_2", NavigationId.Empty, "test"),
        ];

        LocalItemsView = new BrowserTree(_localSource);
        RemoteItemsView = new BrowserTree(_remoteSource);
    }

    [ImportingConstructor]
    public FileBrowserViewModel(ICommandService cmd, IFtpService svc)
        : base(PageId, cmd)
    {
        ArgumentNullException.ThrowIfNull(svc.Client);
        _client = svc.Client;
        _clientEx = new FtpClientEx(_client);
        _localRootPath = AppHost.Instance.GetService<IAppPath>().UserDataFolder;

        _localSource = [];
        _remoteSource = [];

        LocalSearchText = new BindableReactiveProperty<string>();
        RemoteSearchText = new BindableReactiveProperty<string>();
        LocalSelectedItem = new BindableReactiveProperty<BrowserItem?>();
        RemoteSelectedItem = new BindableReactiveProperty<BrowserItem?>();

        /*_sub1 = LocalSearchText
            .Debounce(TimeSpan.FromMicroseconds(500))
            .Subscribe(search =>
            {
                _localView.AttachFilter(s => MatchesSearch(s, search));
                var match = FindFirstMatchingItem(_localView, search);
                if (match == null)
                {
                    return;
                }

                ExpandParents(match);
                LocalSelectedItem.OnNext(match);
            });

        _sub2 = RemoteSearchText
            .Debounce(TimeSpan.FromMicroseconds(500))
            .Subscribe(search =>
            {
                _remoteView.AttachFilter(s => MatchesSearch(s.Name, search));
                var match = FindFirstMatchingItem(_remoteView, search);
                if (match == null)
                {
                    return;
                }

                ExpandParents(match);
                RemoteSelectedItem.OnNext(match);
            });*/

        UploadCommand = CanUpload.ToReactiveCommand<Unit>(UploadImpl);
        DownloadCommand = CanDownload.ToReactiveCommand<Unit>(DownloadImpl);
        CreateRemoteFolderCommand = new ReactiveCommand<Unit>(CreateRemoteFolderImpl);
        CreateLocalFolderCommand = new ReactiveCommand<Unit>(CreateLocalFolderImpl);
        RefreshRemoteCommand = new ReactiveCommand<Unit>(RefreshRemoteImpl);
        RefreshRemoteCommand.IgnoreOnErrorResume(e =>
        {
            if (e is not FtpNackEndOfFileException)
            {
                throw e;
            }
        });
        RefreshLocalCommand = new ReactiveCommand<Unit>(RefreshLocalImpl);
        RemoveLocalItemCommand = new ReactiveCommand<Unit>(RemoveLocalItemImpl);
        RemoveRemoteItemCommand = new ReactiveCommand<Unit>(RemoveRemoteItemImpl);
        SetInEditModeCommand = CanEdit.ToReactiveCommand<BrowserItem>(SetEditModeImpl);
        ClearLocalSearchBoxCommand = new ReactiveCommand<Unit>(ClearLocalSearchBoxImpl);
        ClearRemoteSearchBoxCommand = new ReactiveCommand<Unit>(ClearRemoteSearchBoxImpl);
        FindFileOnLocalCommand = CanFindFileOnLocal.ToReactiveCommand<Unit>(FindFileOnLocalImpl);
        CompareSelectedItemsCommand = CanCompareSelectedItems.ToReactiveCommand<Unit>(
            CompareSelectedItemsImpl
        );
        CalculateLocalCrc32Command = CanCalculateLocalCrc32.ToReactiveCommand<Unit>(
            CalculateLocalCrc32Impl
        );
        CalculateRemoteCrc32Command = CanCalculateRemoteCrc32.ToReactiveCommand<Unit>(
            CalculateRemoteCrc32Impl
        );

        RefreshLocalCommand.Execute(Unit.Default);
        RefreshRemoteCommand.Execute(Unit.Default);

        LocalItemsView = new BrowserTree(_localSource);
        RemoteItemsView = new BrowserTree(_remoteSource);
    }

    public BrowserTree LocalItemsView { get; }
    public BrowserTree RemoteItemsView { get; }
    public BindableReactiveProperty<BrowserItem?> LocalSelectedItem { get; set; }
    public BindableReactiveProperty<BrowserItem?> RemoteSelectedItem { get; set; }
    public BindableReactiveProperty<string> LocalSearchText { get; set; }
    public BindableReactiveProperty<string> RemoteSearchText { get; set; }

    #region Commands

    public ReactiveCommand<Unit> UploadCommand { get; set; }
    public ReactiveCommand<Unit> DownloadCommand { get; set; }
    public ReactiveCommand<Unit> CreateRemoteFolderCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> CreateLocalFolderCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> RefreshRemoteCommand { get; set; }
    public ReactiveCommand<Unit> RefreshLocalCommand { get; set; }
    public ReactiveCommand<Unit> RemoveLocalItemCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> RemoveRemoteItemCommand { get; set; } // TODO: implement
    public ReactiveCommand<BrowserItem> SetInEditModeCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> ClearLocalSearchBoxCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> ClearRemoteSearchBoxCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> CompareSelectedItemsCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> FindFileOnLocalCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> CalculateLocalCrc32Command { get; set; } // TODO: implement
    public ReactiveCommand<Unit> CalculateRemoteCrc32Command { get; set; } // TODO: implement

    private Observable<bool> CanUpload =>
        LocalSelectedItem.Select(x => x is { FtpEntryType: FtpEntryType.File });

    private Observable<bool> CanDownload =>
        RemoteSelectedItem.Select(x => x is { FtpEntryType: FtpEntryType.File });

    private Observable<bool> CanEdit =>
        LocalSelectedItem.Select(x => x is { IsInEditMode.Value: false });

    private Observable<bool> CanFindFileOnLocal =>
        RemoteSelectedItem.Select(x => x is { IsInEditMode.Value: false });

    private Observable<bool> CanCompareSelectedItems =>
        LocalSelectedItem.CombineLatest(
            RemoteSelectedItem,
            (local, remote) =>
                local is { IsInEditMode.Value: false, FtpEntryType: FtpEntryType.File }
                && remote is { IsInEditMode.Value: false, FtpEntryType: FtpEntryType.File }
        );

    private Observable<bool> CanCalculateRemoteCrc32 =>
        RemoteSelectedItem.Select(x =>
            x is { IsInEditMode.Value: false, FtpEntryType: FtpEntryType.File }
        );

    private Observable<bool> CanCalculateLocalCrc32 =>
        LocalSelectedItem.Select(x =>
            x is { IsInEditMode.Value: false, FtpEntryType: FtpEntryType.File }
        );

    #endregion

    #region Commands implementation

    private async ValueTask UploadImpl(Unit unit, CancellationToken token)
    {
        /*var dialog = new ContentDialog
        {
            Title = RS.FileBrowserViewModel_UploadDialog_Title,
            PrimaryButtonText = RS.FileBrowserViewModel_UploadDialog_PrimaryButtonText,
        };

        using var viewModel = new UploadFileDialogViewModel(
            _log,
            _clientEx,
            RemoteSelectedItem,
            LocalSelectedItem!
        );
        dialog.Content = viewModel;
        viewModel.ApplyDialog(dialog);
        await dialog.ShowAsync();

        await RefreshRemoteImpl(unit, token);*/
    }

    private async ValueTask DownloadImpl(Unit unit, CancellationToken token)
    {
        /*var dialog = new ContentDialog
        {
            Title = RS.FileBrowserViewModel_DownloadDialog_Title,
            PrimaryButtonText = RS.FileBrowserViewModel_DownloadDialog_SecondaryButtonText,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = RS.FileBrowserViewModel_DownloadDialog_PrimaryButtonText,
        };

        using var viewModel = new DownloadFileDialogViewModel(
            _log,
            _clientEx,
            _localRootPath,
            RemoteSelectedItem!,
            LocalSelectedItem
        );
        dialog.Content = viewModel;
        viewModel.ApplyDialog(dialog);
        await dialog.ShowAsync();

        await RefreshLocalImpl(unit, token);*/
    }

    private async ValueTask CalculateLocalCrc32Impl(Unit unit, CancellationToken token)
    {
        var path = await File.ReadAllBytesAsync(LocalSelectedItem.Value!.Id.Id, token);
        var crc32 = Crc32Mavlink.Accumulate(path);
        var hexCrc32 = Crc32ToHex(crc32);
        LocalSelectedItem.Value.Crc32Hex = hexCrc32;

        /*LocalSelectedItem.Value.Crc32Color =
            hexCrc32 == "00000000"
                ? FileSystemItemViewModel.BadCrcColor
                : FileSystemItemViewModel.DefaultColor;*/
    }

    private async ValueTask CalculateRemoteCrc32Impl(Unit unit, CancellationToken token)
    {
        var crc32 = await _clientEx.Base.CalcFileCrc32(RemoteSelectedItem.Value!.Id.Id, token);
        var hexCrc32 = Crc32ToHex(crc32);
        RemoteSelectedItem.Value.Crc32Hex = hexCrc32;

        RemoteSelectedItem.Value.Crc32Hex = hexCrc32;

        /*RemoteSelectedItem.Value.Crc32Color =
            hexCrc32 == "00000000"
                ? FileSystemItemViewModel.BadCrcColor
                : FileSystemItemViewModel.DefaultColor;*/
    }

    private async ValueTask CompareSelectedItemsImpl(Unit unit, CancellationToken token)
    {
        var localFileCrc32 = Crc32Mavlink.Accumulate(
            await File.ReadAllBytesAsync(LocalSelectedItem.Value!.Id.Id, token)
        );
        var remoteFileCrc32 = await _clientEx.Base.CalcFileCrc32(
            RemoteSelectedItem.Value!.Id.Id,
            token
        );

        LocalSelectedItem.Value.Crc32Hex = Crc32ToHex(localFileCrc32);
        RemoteSelectedItem.Value.Crc32Hex = Crc32ToHex(remoteFileCrc32);

        /*if (localFileCrc32 == remoteFileCrc32 && (localFileCrc32 != 0 || remoteFileCrc32 != 0))
        {
            LocalSelectedItem.Value.Crc32Color.OnNext(FileSystemItemViewModel.GoodCrcColor);
            RemoteSelectedItem.Value.Crc32Color.OnNext(FileSystemItemViewModel.GoodCrcColor);
        }
        else
        {
            LocalSelectedItem.Value.Crc32Color.OnNext(FileSystemItemViewModel.BadCrcColor);
            RemoteSelectedItem.Value.Crc32Color.OnNext(FileSystemItemViewModel.BadCrcColor);
        }*/
    }

    private async ValueTask FindFileOnLocalImpl(Unit unit, CancellationToken token)
    {
        /*var file = FindFirstMatchingItem(_localView, RemoteSelectedItem.Value!.Header);
        if (file == null)
        {
            return;
        }

        await ExpandParents(file);
        await Task.Delay(200, token);
        LocalSelectedItem.OnNext(file);*/
    }

    private void ClearLocalSearchBoxImpl(Unit unit) => LocalSearchText.OnNext(string.Empty);

    private void ClearRemoteSearchBoxImpl(Unit unit) => RemoteSearchText.OnNext(string.Empty);

    private async ValueTask RemoveLocalItemImpl(Unit unit, CancellationToken token)
    {
        if (LocalSelectedItem.Value is { FtpEntryType: FtpEntryType.Directory })
        {
            Directory.Delete(LocalSelectedItem.Value.Id.Id, true);
        }

        if (LocalSelectedItem.Value is { FtpEntryType: FtpEntryType.File })
        {
            File.Delete(LocalSelectedItem.Value.Id.Id);
        }

        await RefreshLocalImpl(unit, token);
    }

    private async ValueTask RemoveRemoteItemImpl(Unit unit, CancellationToken token)
    {
        if (RemoteSelectedItem.Value is { FtpEntryType: FtpEntryType.Directory })
        {
            await _clientEx.Base.RemoveDirectory(RemoteSelectedItem.Value.Id.Id, token);
        }

        if (RemoteSelectedItem.Value is { FtpEntryType: FtpEntryType.File })
        {
            await _clientEx.Base.RemoveFile(RemoteSelectedItem.Value.Id.Id, token);
        }

        await RefreshRemoteImpl(unit, token);
    }

    private async ValueTask SetEditModeImpl(BrowserItem item, CancellationToken token)
    {
        item.IsInEditMode.OnNext(true);
        item.EditedName = item.Header ?? "Unknown";
        await CommitEdit(item, token);
    }

    private async ValueTask CommitEdit(BrowserItem item, CancellationToken token)
    {
        /*await Task.Run(
            () =>
            {
                while (item.IsInEditMode) { }
            },
            token
        );
        /*if (!item.IsEditingSuccess)
        {
            return;
        }#1#

        var oldPath = item.Id.Id;
        var oldName = item.Header;
        var directoryPath = Path.GetDirectoryName(oldPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        var newPath = Path.Combine(directoryPath, item.EditedName);

        if (oldPath == newPath)
        {
            item.IsInEditMode = false;

            // item.IsEditingSuccess = false;
        }

        try
        {
            switch (item.FtpEntryType)
            {
                case FtpEntryType.File:
                    File.Move(oldPath, newPath);
                    break;
                case FtpEntryType.Directory:
                    Directory.Move(oldPath, newPath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            item.Header = item.EditedName;
            item.Id = newPath;

            await RefreshLocalImpl(Unit.Default, token);
            _log.Info(
                nameof(FileBrowserViewModel),
                $"File renamed from {oldName} to {item.Header}"
            ); // TODO: localization

            item.IsInEditMode = false;

            // item.IsEditingSuccess = false;
        }
        catch (Exception ex)
        {
            _log.Error(
                nameof(FileBrowserViewModel),
                $"Failed to rename file from {oldPath} to {newPath}", // TODO: localization
                ex
            );
        }*/
    }

    private async ValueTask CreateRemoteFolderImpl(Unit unit, CancellationToken token)
    {
        CreateFolder(1);
        await RefreshRemoteImpl(unit, token);
        return;

        async void CreateFolder(int n)
        {
            /*var tree = new List<FileSystemItemViewModel>();
            await _clientEx
                .Entries.TransformToTree(
                    e => e.Path,
                    e => e.ParentPath,
                    d => d.Keys.FirstOrDefault()
                )
                .ForEachAsync(
                    x =>
                    {
                        if (x != null)
                        {
                            tree.Add(new FileSystemItemViewModel(x));
                        }
                    },
                    cancellationToken: token
                );
            while (true)
            {
                var name = $"Folder{n}";
                if (RemoteSelectedItem != null)
                {
                    var path = RemoteSelectedItem.Value!.IsDirectory
                        ? Path.Combine(RemoteSelectedItem.Value.Path, name)
                        : Path.Combine(
                            RemoteSelectedItem.Value.Path[
                                ..RemoteSelectedItem.Value.Path.LastIndexOf('/')
                            ],
                            name
                        );

                    await _clientEx.Base.CreateDirectory(path, token);
                }
                else
                {
                    var path = Path.Combine("/", name);
                    if (tree.FirstOrDefault(x => x.Path == path) != null)
                    {
                        n++;
                        continue;
                    }

                    await _clientEx.Base.CreateDirectory(path, token);
                }

                break;
            }*/
        }
    }

    private async ValueTask CreateLocalFolderImpl(Unit unit, CancellationToken token)
    {
        CreateFolder(1);
        await RefreshLocalImpl(unit, token);
        return;

        void CreateFolder(int n)
        {
            while (true)
            {
                var name = $"Folder{n}";
                if (LocalSelectedItem != null)
                {
                    Directory.CreateDirectory(
                        LocalSelectedItem.Value!.FtpEntryType == FtpEntryType.Directory
                            ? Path.Combine(LocalSelectedItem.Value.Id.Id, name)
                            : Path.Combine(
                                LocalSelectedItem.Value.Id.Id[
                                    ..LocalSelectedItem.Value.Id.Id.LastIndexOf('\\')
                                ],
                                name
                            )
                    );
                }
                else
                {
                    var path = Path.Combine(_localRootPath, name);
                    if (Directory.Exists(path))
                    {
                        n++;
                        continue;
                    }

                    Directory.CreateDirectory(path);
                }

                break;
            }
        }
    }

    private async ValueTask RefreshRemoteImpl(Unit unit, CancellationToken token)
    {
        if (RemoteSelectedItem.Value == null)
        {
            await _clientEx.Refresh("/", cancel: token);
        }
        else
        {
            await _clientEx.Refresh(RemoteSelectedItem.Value!.Id.Id, cancel: token);
        }

        _remoteSource.Clear();
        _remoteSource.AddRange(LoadRemoteItems());
    }

    private ValueTask RefreshLocalImpl(Unit unit, CancellationToken token)
    {
        _localSource.Clear();
        _localSource.AddRange(LoadLocalItems());
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Load items

    private ObservableList<IBrowserItem> LoadLocalItems()
    {
        var items = new ObservableList<IBrowserItem>();
        ProcessDirectory(_localRootPath, items);
        return items;
    }

    private void ProcessDirectory(string directoryPath, ObservableList<IBrowserItem> items)
    {
        var directories = Directory.GetDirectories(directoryPath);
        foreach (var dir in directories)
        {
            var id = NavigationId.NormalizeTypeId(Path.GetRelativePath(_localRootPath, dir));
            var parentId = NavigationId.NormalizeTypeId(
                Path.GetRelativePath(
                    _localRootPath,
                    Directory.GetParent(dir)?.FullName ?? _localRootPath
                )
            );
            var name = new DirectoryInfo(dir).Name;

            items.Add(new DirectoryItem(id, parentId, name));
            ProcessDirectory(dir, items);
        }

        var files = Directory.GetFiles(directoryPath);
        foreach (var file in files)
        {
            var id = NavigationId.NormalizeTypeId(Path.GetRelativePath(_localRootPath, file));
            var parentId = NavigationId.NormalizeTypeId(
                Path.GetRelativePath(
                    _localRootPath,
                    Directory.GetParent(file)?.FullName ?? _localRootPath
                )
            );
            var fileInfo = new FileInfo(file);
            var name = fileInfo.Name;
            var size = fileInfo.Length;

            items.Add(new FileItem(id, parentId, name, size));
        }
    }

    private ObservableList<IBrowserItem> LoadRemoteItems()
    {
        var items = new ObservableList<IBrowserItem>();

        _clientEx.Entries.ForEach(e =>
        {
            if (e.Value.Path == "/")
            {
                var root = new DirectoryItem("_", NavigationId.Empty, "_");
                items.Add(root);
                return;
            }

            var item = e.Value.Type switch
            {
                FtpEntryType.Directory => new DirectoryItem(
                    NavigationId.NormalizeTypeId(e.Value.Path),
                    NavigationId.NormalizeTypeId(e.Value.ParentPath),
                    e.Value.Name
                ),
                FtpEntryType.File => new FileItem(
                    NavigationId.NormalizeTypeId(e.Value.Path),
                    NavigationId.NormalizeTypeId(e.Value.ParentPath),
                    e.Value.Name,
                    ((FtpFile)e.Value).Size
                ),
                _ => new BrowserItem(
                    NavigationId.NormalizeTypeId(e.Value.Path),
                    NavigationId.NormalizeTypeId(e.Value.ParentPath)
                ),
            };

            items.Add(item);
        });

        return items;
    }

    private static Task ExpandParents(BrowserItem item)
    {
        /*var parent = item.Parent;
        while (parent != null)
        {
            parent.IsExpanded.OnNext(true);
            parent = parent.Parent;
        }*/

        return Task.CompletedTask;
    }

    private static bool MatchesSearch(string item, string? search)
    {
        return search == null || item.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private static BrowserItem? FindFirstMatchingItem(
        IEnumerable<BrowserItem> items,
        string? search
    )
    {
        /*if (search == null)
        {
            return null;
        }

        foreach (var item in items)
        {
            if (item.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            var match = FindFirstMatchingItem(item.Children, search);
            if (match != null)
            {
                return match;
            }
        }*/

        return null;
    }

    #endregion

    private static string Crc32ToHex(uint crc32) => crc32.ToString("X8");

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();

            _client.Dispose();
            _localSource.Clear();
            _remoteSource.Clear();

            LocalItemsView.Dispose();
            RemoteItemsView.Dispose();

            UploadCommand.Dispose();
            DownloadCommand.Dispose();
            CreateRemoteFolderCommand.Dispose();
            CreateLocalFolderCommand.Dispose();
            RefreshRemoteCommand.Dispose();
            RefreshLocalCommand.Dispose();
            RemoveLocalItemCommand.Dispose();
            RemoveRemoteItemCommand.Dispose();
            SetInEditModeCommand.Dispose();
            ClearLocalSearchBoxCommand.Dispose();
            ClearRemoteSearchBoxCommand.Dispose();
            FindFileOnLocalCommand.Dispose();
            CompareSelectedItemsCommand.Dispose();

            LocalSearchText.Dispose();
            RemoteSearchText.Dispose();
            LocalSelectedItem.Dispose();
            RemoteSelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
