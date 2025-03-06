using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Mavlink;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

public class FileBrowserViewModel : RoutableViewModel
{
    public const string ViewModelId = "files.browser";

    private readonly ILogService _log;
    private readonly IFtpClient _client;
    private readonly FtpClientEx _clientEx;
    private readonly string _localRootPath;

    private readonly ObservableList<string> _localSource;
    private readonly ObservableList<IFtpEntry> _remoteSource;
    private readonly ISynchronizedView<string, FileSystemItemViewModel> _localView;
    private readonly ISynchronizedView<IFtpEntry, FileSystemItemViewModel> _remoteView;

    public FileBrowserViewModel()
        : base(string.Empty)
    {
        DesignTime.ThrowIfNotDesignMode();
        _client = null!;
        _log = null!;
        _localRootPath = string.Empty;
    }

    [ImportingConstructor]
    public FileBrowserViewModel(IFtpClient client, ILogService log)
        : base($"{ViewModelId}.{client.Id}")
    {
        _client = client;
        _clientEx = new FtpClientEx(client);
        _log = log;
        _localRootPath = AppHost.Instance.GetService<IAppPath>().UserDataFolder;

        _localSource = new ObservableList<string>();
        _remoteSource = new ObservableList<IFtpEntry>();

        _localView = _localSource.CreateView(x => new FileSystemItemViewModel(
            x,
            Directory.Exists(x)
        ));
        _remoteView = _remoteSource.CreateView(x => new FileSystemItemViewModel(
            new ReactiveNode<IFtpEntry>(x)
        ));

        LocalSearchText = new BindableReactiveProperty<string>();
        RemoteSearchText = new BindableReactiveProperty<string>();
        LocalSelectedItem = new BindableReactiveProperty<FileSystemItemViewModel?>();
        RemoteSelectedItem = new BindableReactiveProperty<FileSystemItemViewModel?>();

        _sub1 = LocalSearchText
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
            });

        LocalItems = _localView.ToNotifyCollectionChanged();
        LocalItems = _remoteView.ToNotifyCollectionChanged();

        UploadCommand = CanUpload.ToReactiveCommand<Unit>(UploadImpl);
        DownloadCommand = CanDownload.ToReactiveCommand<Unit>(DownloadImpl);
        CreateRemoteFolderCommand = new ReactiveCommand<Unit>(CreateRemoteFolderImpl);
        CreateLocalFolderCommand = new ReactiveCommand<Unit>(CreateLocalFolderImpl);
        RefreshRemoteCommand = new ReactiveCommand<Unit>(RefreshRemoteImpl);
        RefreshLocalCommand = new ReactiveCommand<Unit>(RefreshLocalImpl);
        RemoveLocalItemCommand = new ReactiveCommand<Unit>(RemoveLocalItemImpl);
        RemoveRemoteItemCommand = new ReactiveCommand<Unit>(RemoveRemoteItemImpl);
        SetInEditModeCommand = CanEdit.ToReactiveCommand<FileSystemItemViewModel>(SetEditModeImpl);
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

        LoadLocalItems(_localRootPath);
        LoadRemoteItems();
    }

    public NotifyCollectionChangedSynchronizedViewList<FileSystemItemViewModel> LocalItems { get; set; }
    public NotifyCollectionChangedSynchronizedViewList<FileSystemItemViewModel> RemoteItems { get; set; }
    public BindableReactiveProperty<FileSystemItemViewModel?> LocalSelectedItem { get; set; }
    public BindableReactiveProperty<FileSystemItemViewModel?> RemoteSelectedItem { get; set; }
    public BindableReactiveProperty<string> LocalSearchText { get; set; }
    public BindableReactiveProperty<string> RemoteSearchText { get; set; }

    #region Commands

    public ReactiveCommand<Unit> UploadCommand { get; set; }
    public ReactiveCommand<Unit> DownloadCommand { get; set; }
    public ReactiveCommand<Unit> CreateRemoteFolderCommand { get; set; }
    public ReactiveCommand<Unit> CreateLocalFolderCommand { get; set; }
    public ReactiveCommand<Unit> RefreshRemoteCommand { get; set; }
    public ReactiveCommand<Unit> RefreshLocalCommand { get; set; }
    public ReactiveCommand<Unit> RemoveLocalItemCommand { get; set; }
    public ReactiveCommand<Unit> RemoveRemoteItemCommand { get; set; }
    public ReactiveCommand<FileSystemItemViewModel> SetInEditModeCommand { get; set; }
    public ReactiveCommand<Unit> ClearLocalSearchBoxCommand { get; set; }
    public ReactiveCommand<Unit> ClearRemoteSearchBoxCommand { get; set; }
    public ReactiveCommand<Unit> CompareSelectedItemsCommand { get; set; }
    public ReactiveCommand<Unit> FindFileOnLocalCommand { get; set; }
    public ReactiveCommand<Unit> CalculateLocalCrc32Command { get; set; }
    public ReactiveCommand<Unit> CalculateRemoteCrc32Command { get; set; }

    private Observable<bool> CanUpload =>
        LocalSelectedItem.Select(x => x is { IsDirectory: false });

    private Observable<bool> CanDownload =>
        RemoteSelectedItem.Select(x => x is { IsDirectory: false });

    private Observable<bool> CanEdit =>
        LocalSelectedItem.Select(x => x is { IsInEditMode.Value: false });

    private Observable<bool> CanFindFileOnLocal =>
        RemoteSelectedItem.Select(x => x is { IsInEditMode.Value: false });

    private Observable<bool> CanCompareSelectedItems =>
        LocalSelectedItem.CombineLatest(
            RemoteSelectedItem,
            (local, remote) =>
                local is { IsInEditMode.Value: false, IsFile: true }
                && remote is { IsInEditMode.Value: false, IsFile: true }
        );

    private Observable<bool> CanCalculateRemoteCrc32 =>
        RemoteSelectedItem.Select(x => x is { IsInEditMode.Value: false, IsFile: true });

    private Observable<bool> CanCalculateLocalCrc32 =>
        LocalSelectedItem.Select(x => x is { IsInEditMode.Value: false, IsFile: true });

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
        var path = await File.ReadAllBytesAsync(LocalSelectedItem.Value!.Path, token);
        var crc32 = Crc32Mavlink.Accumulate(path);
        var hexCrc32 = Crc32ToHex(crc32);
        LocalSelectedItem.Value.Crc32Hex.OnNext(hexCrc32);

        LocalSelectedItem.Value.Crc32Color.OnNext(
            hexCrc32 == "00000000"
                ? FileSystemItemViewModel.BadCrcColor
                : FileSystemItemViewModel.DefaultColor
        );
    }

    private async ValueTask CalculateRemoteCrc32Impl(Unit unit, CancellationToken token)
    {
        var crc32 = await _clientEx.Base.CalcFileCrc32(RemoteSelectedItem.Value!.Path, token);
        var hexCrc32 = Crc32ToHex(crc32);
        RemoteSelectedItem.Value.Crc32Hex.OnNext(hexCrc32);

        RemoteSelectedItem.Value.Crc32Hex.OnNext(hexCrc32);

        RemoteSelectedItem.Value.Crc32Color.OnNext(
            hexCrc32 == "00000000"
                ? FileSystemItemViewModel.BadCrcColor
                : FileSystemItemViewModel.DefaultColor
        );
    }

    private async ValueTask CompareSelectedItemsImpl(Unit unit, CancellationToken token)
    {
        var localFileCrc32 = Crc32Mavlink.Accumulate(
            await File.ReadAllBytesAsync(LocalSelectedItem.Value!.Path, token)
        );
        var remoteFileCrc32 = await _clientEx.Base.CalcFileCrc32(
            RemoteSelectedItem.Value!.Path,
            token
        );

        LocalSelectedItem.Value.Crc32Hex.OnNext(Crc32ToHex(localFileCrc32));
        RemoteSelectedItem.Value.Crc32Hex.OnNext(Crc32ToHex(remoteFileCrc32));

        if (localFileCrc32 == remoteFileCrc32 && (localFileCrc32 != 0 || remoteFileCrc32 != 0))
        {
            LocalSelectedItem.Value.Crc32Color.OnNext(FileSystemItemViewModel.GoodCrcColor);
            RemoteSelectedItem.Value.Crc32Color.OnNext(FileSystemItemViewModel.GoodCrcColor);
        }
        else
        {
            LocalSelectedItem.Value.Crc32Color.OnNext(FileSystemItemViewModel.BadCrcColor);
            RemoteSelectedItem.Value.Crc32Color.OnNext(FileSystemItemViewModel.BadCrcColor);
        }
    }

    private async ValueTask FindFileOnLocalImpl(Unit unit, CancellationToken token)
    {
        var file = FindFirstMatchingItem(_localView, RemoteSelectedItem.Value!.Name);
        if (file == null)
        {
            return;
        }

        await ExpandParents(file);
        await Task.Delay(200, token);
        LocalSelectedItem.OnNext(file);
    }

    private void ClearLocalSearchBoxImpl(Unit unit) => LocalSearchText.OnNext(string.Empty);

    private void ClearRemoteSearchBoxImpl(Unit unit) => RemoteSearchText.OnNext(string.Empty);

    private async ValueTask RemoveLocalItemImpl(Unit unit, CancellationToken token)
    {
        if (LocalSelectedItem.Value is { IsDirectory: true })
        {
            Directory.Delete(LocalSelectedItem.Value.Path, true);
        }

        if (LocalSelectedItem.Value is { IsFile: true })
        {
            File.Delete(LocalSelectedItem.Value.Path);
        }

        await RefreshLocalImpl(unit, token);
    }

    private async ValueTask RemoveRemoteItemImpl(Unit unit, CancellationToken token)
    {
        if (RemoteSelectedItem.Value is { IsDirectory: true })
        {
            await _clientEx.Base.RemoveDirectory(RemoteSelectedItem.Value.Path, token);
        }

        if (RemoteSelectedItem.Value is { IsFile: true })
        {
            await _clientEx.Base.RemoveFile(RemoteSelectedItem.Value.Path, token);
        }

        await RefreshRemoteImpl(unit, token);
    }

    private async ValueTask SetEditModeImpl(FileSystemItemViewModel item, CancellationToken token)
    {
        item.IsInEditMode.OnNext(true);
        item.EditedName.OnNext(item.Name);
        await CommitEdit(item, token);
    }

    private async ValueTask CommitEdit(FileSystemItemViewModel item, CancellationToken token)
    {
        await Task.Run(
            () =>
            {
                while (item.IsInEditMode.Value) { }
            },
            token
        );
        if (!item.IsEditingSuccess)
        {
            return;
        }

        var oldPath = item.Path;
        var oldName = item.Name;
        var directoryPath = Path.GetDirectoryName(oldPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        var newPath = Path.Combine(directoryPath, item.EditedName.Value);

        if (oldPath == newPath)
        {
            item.IsInEditMode.OnNext(false);
            item.IsEditingSuccess = false;
        }

        try
        {
            if (item.IsFile)
            {
                File.Move(oldPath, newPath);
            }

            if (item.IsDirectory)
            {
                Directory.Move(oldPath, newPath);
            }

            item.Name = item.EditedName.Value;
            item.Path = newPath;

            await RefreshLocalImpl(Unit.Default, token);
            _log.Info(nameof(FileBrowserViewModel), $"File renamed from {oldName} to {item.Name}"); // TODO: localization

            item.IsInEditMode.OnNext(false);
            item.IsEditingSuccess = false;
        }
        catch (Exception ex)
        {
            _log.Error(
                nameof(FileBrowserViewModel),
                $"Failed to rename file from {oldPath} to {newPath}", // TODO: localization
                ex
            );
        }
    }

    private async ValueTask CreateRemoteFolderImpl(Unit unit, CancellationToken token)
    {
        CreateFolder(1);
        await RefreshRemoteImpl(unit, token);
        return;

        async void CreateFolder(int n)
        {
            var tree = new List<FileSystemItemViewModel>();
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
            }
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
                        LocalSelectedItem.Value!.IsDirectory
                            ? Path.Combine(LocalSelectedItem.Value.Path, name)
                            : Path.Combine(
                                LocalSelectedItem.Value.Path[
                                    ..LocalSelectedItem.Value.Path.LastIndexOf('\\')
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
            await _clientEx.Refresh(RemoteSelectedItem.Value!.Path, cancel: token);
        }

        LoadRemoteItems();
    }

    private ValueTask RefreshLocalImpl(Unit unit, CancellationToken token)
    {
        LoadLocalItems(_localRootPath);
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Load items

    private void LoadLocalItems(string path)
    {
        _localSource.AddRange(Directory.GetFileSystemEntries(path));
    }

    private void LoadRemoteItems()
    {
        _clientEx
            .Entries.TransformToTree(e => e.Path, e => e.ParentPath, d => d.Keys.FirstOrDefault())
            .ForEachAsync(x =>
            {
                if (x != null)
                {
                    _remoteSource.Add(x.Item.Value);
                }
            });
    }

    private static Task ExpandParents(FileSystemItemViewModel item)
    {
        var parent = item.Parent;
        while (parent != null)
        {
            parent.IsExpanded.OnNext(true);
            parent = parent.Parent;
        }

        return Task.CompletedTask;
    }

    private static bool MatchesSearch(string item, string? search)
    {
        return search == null || item.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private static FileSystemItemViewModel? FindFirstMatchingItem(
        IEnumerable<FileSystemItemViewModel> items,
        string? search
    )
    {
        if (search == null)
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
        }

        return null;
    }

    public static string ConvertBytesToReadableSize(long fileSizeInBytes)
    {
        string[] sizes =
        [
            RS.Unit_Byte_Abbreviation,
            RS.Unit_Kilobyte_Abbreviation,
            RS.Unit_Megabyte_Abbreviation,
            RS.Unit_Gigabyte_Abbreviation,
            RS.Unit_Terabyte_Abbreviation,
        ];
        double len = fileSizeInBytes;
        var order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    #endregion

    private static string Crc32ToHex(uint crc32) => crc32.ToString("X8");

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

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
            _localView.Dispose();
            _remoteView.Dispose();

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
