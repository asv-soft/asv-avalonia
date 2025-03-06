using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Mavlink;
using Avalonia.Remote.Protocol.Input;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

[ExportPage(PageId)]
public class FileBrowserViewModel : PageViewModel<FileBrowserViewModel>
{
    public const string PageId = "files.browser";
    public const MaterialIconKind PageIcon = MaterialIconKind.FolderEye;

    private readonly IDialogService _dialogs;
    private readonly ILogger<FileBrowserViewModel> _log;
    private readonly IFtpClient _client;
    private readonly FtpClientEx _clientEx;
    private readonly string _localRootPath;

    private readonly ObservableList<IBrowserItem> _localSource;
    private readonly ObservableList<IBrowserItem> _remoteSource;

    public FileBrowserViewModel()
        : base(string.Empty, DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
        _dialogs = null!;
        _log = null!;
        _client = null!;
        _clientEx = null!;
        _localRootPath = null!;
        LocalSelectedItem = null!;
        RemoteSelectedItem = null!;
        LocalSearchText = null!;
        RemoteSearchText = null!;
        Progress = null!;
        UploadCommand = null!;
        DownloadCommand = null!;
        CreateRemoteFolderCommand = null!;
        CreateLocalFolderCommand = null!;
        RefreshRemoteCommand = null!;
        RefreshLocalCommand = null!;
        RemoveLocalItemCommand = null!;
        RemoveRemoteItemCommand = null!;
        SetInEditModeCommand = null!;
        ClearLocalSearchBoxCommand = null!;
        ClearRemoteSearchBoxCommand = null!;
        CompareSelectedItemsCommand = null!;
        FindFileOnLocalCommand = null!;
        CalculateLocalCrc32Command = null!;
        CalculateRemoteCrc32Command = null!;
        _sub1 = null!;
        _sub2 = null!;
        _localSource =
        [
            new DirectoryItem("0", NavigationId.Empty, "/", "test"),
            new DirectoryItem("1", "0", "/", "test"),
            new DirectoryItem("2", "0", "/", "test"),
            new DirectoryItem("0_1", NavigationId.Empty, "/", "test"),
            new DirectoryItem("0_2", NavigationId.Empty, "/", "test"),
        ];
        _remoteSource =
        [
            new DirectoryItem("0", NavigationId.Empty, "/", "test"),
            new DirectoryItem("1", "0", "/", "test"),
            new DirectoryItem("2", "0", "/", "test"),
            new DirectoryItem("0_1", NavigationId.Empty, "/", "test"),
            new DirectoryItem("0_2", NavigationId.Empty, "/", "test"),
        ];

        LocalItemsView = new BrowserTree(_localSource);
        RemoteItemsView = new BrowserTree(_remoteSource);
    }

    [ImportingConstructor]
    public FileBrowserViewModel(
        ICommandService cmd,
        IDialogService dialogs,
        IFtpService svc,
        ILoggerFactory log
    )
        : base(PageId, cmd)
    {
        ArgumentNullException.ThrowIfNull(svc.Client);

        Title.Value = $"Files {svc.Client.Id}"; // TODO: localization

        _client = svc.Client;
        _clientEx = new FtpClientEx(_client);
        _localRootPath = AppHost.Instance.GetService<IAppPath>().UserDataFolder;
        _dialogs = dialogs;
        _log = log.CreateLogger<FileBrowserViewModel>();

        _localSource = [];
        _remoteSource = [];

        LocalSearchText = new BindableReactiveProperty<string>();
        RemoteSearchText = new BindableReactiveProperty<string>();
        LocalSelectedItem = new BindableReactiveProperty<BrowserNode?>(null);
        RemoteSelectedItem = new BindableReactiveProperty<BrowserNode?>(null);
        Progress = new BindableReactiveProperty<double>(0);
        IsDownloadPopupOpen = new BindableReactiveProperty<bool>(false);

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

        ShowDownloadPopupCommand = CanDownload.ToReactiveCommand(_ =>
            IsDownloadPopupOpen.OnNext(!IsDownloadPopupOpen.Value)
        );
        UploadCommand = CanUpload.ToReactiveCommand<Unit>(UploadImpl);
        DownloadCommand = CanDownload.ToReactiveCommand<Unit>(DownloadImpl);
        BurstDownloadCommand = CanDownload.ToReactiveCommand<Unit>(BurstDownloadImpl);
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
        SetInEditModeCommand = CanEdit.ToReactiveCommand<BrowserNode>(SetEditModeImpl);
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
        RefreshRemoteCommand.SubscribeOnUIThreadDispatcher();
        RefreshRemoteCommand.SubscribeOnUIThreadDispatcher();

        RefreshLocalCommand.Execute(Unit.Default);
        RefreshRemoteCommand.Execute(Unit.Default);

        LocalItemsView = new BrowserTree(_localSource);
        RemoteItemsView = new BrowserTree(_remoteSource);
    }

    public BrowserTree LocalItemsView { get; }
    public BrowserTree RemoteItemsView { get; }
    public BindableReactiveProperty<BrowserNode?> LocalSelectedItem { get; set; }
    public BindableReactiveProperty<BrowserNode?> RemoteSelectedItem { get; set; }
    public BindableReactiveProperty<string> LocalSearchText { get; set; }
    public BindableReactiveProperty<string> RemoteSearchText { get; set; }
    public BindableReactiveProperty<double> Progress { get; set; }
    public BindableReactiveProperty<bool> IsDownloadPopupOpen { get; set; }

    #region Commands

    public ReactiveCommand<Unit> ShowDownloadPopupCommand { get; set; }
    public ReactiveCommand<Unit> UploadCommand { get; set; }
    public ReactiveCommand<Unit> DownloadCommand { get; set; }
    public ReactiveCommand<Unit> BurstDownloadCommand { get; set; }
    public ReactiveCommand<Unit> CreateRemoteFolderCommand { get; set; }
    public ReactiveCommand<Unit> CreateLocalFolderCommand { get; set; }
    public ReactiveCommand<Unit> RefreshRemoteCommand { get; set; }
    public ReactiveCommand<Unit> RefreshLocalCommand { get; set; }
    public ReactiveCommand<Unit> RemoveLocalItemCommand { get; set; }
    public ReactiveCommand<Unit> RemoveRemoteItemCommand { get; set; }
    public ReactiveCommand<BrowserNode> SetInEditModeCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> ClearLocalSearchBoxCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> ClearRemoteSearchBoxCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> CompareSelectedItemsCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> FindFileOnLocalCommand { get; set; } // TODO: implement
    public ReactiveCommand<Unit> CalculateLocalCrc32Command { get; set; } // TODO: implement
    public ReactiveCommand<Unit> CalculateRemoteCrc32Command { get; set; } // TODO: implement

    private Observable<bool> CanUpload =>
        LocalSelectedItem.Select(x => x?.Base is { FtpEntryType: FtpEntryType.File });

    private Observable<bool> CanDownload =>
        RemoteSelectedItem.Select(x => x?.Base is { FtpEntryType: FtpEntryType.File });

    private Observable<bool> CanEdit =>
        LocalSelectedItem.Select(x => x?.Base is { IsInEditMode.Value: false });

    private Observable<bool> CanFindFileOnLocal =>
        RemoteSelectedItem.Select(x => x?.Base is { IsInEditMode.Value: false });

    private Observable<bool> CanCompareSelectedItems =>
        LocalSelectedItem.CombineLatest(
            RemoteSelectedItem,
            (local, remote) =>
                local?.Base is { IsInEditMode.Value: false, FtpEntryType: FtpEntryType.File }
                && remote?.Base is { IsInEditMode.Value: false, FtpEntryType: FtpEntryType.File }
        );

    private Observable<bool> CanCalculateRemoteCrc32 =>
        RemoteSelectedItem.Select(x =>
            x?.Base is { IsInEditMode.Value: false, FtpEntryType: FtpEntryType.File }
        );

    private Observable<bool> CanCalculateLocalCrc32 =>
        LocalSelectedItem.Select(x =>
            x?.Base is { IsInEditMode.Value: false, FtpEntryType: FtpEntryType.File }
        );

    #endregion

    #region Commands implementation

    private async ValueTask UploadImpl(Unit unit, CancellationToken token)
    {
        var item = LocalSelectedItem.Value;
        if (item == null)
        {
            return;
        }

        var res = await _dialogs.ShowYesNoDialog(
            "Uploading",
            $"Do you want to upload file \'{item.Base.Header}\' to device?" //TODO: localization
        );

        if (res)
        {
            var progress = new Progress<double>();
            progress.ProgressChanged += (_, value) => Progress.OnNext(value);

            var path =
                RemoteSelectedItem.Value == null
                    ? Path.Combine(Path.PathSeparator.ToString(), item.Base.Header ?? "unknown")
                    : Path.Combine(
                        RemoteSelectedItem.Value.Base.Path,
                        item.Base.Header ?? "unknown"
                    );

            try
            {
                var stream = new FileStream(
                    item.Base.Path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read
                );
                await _clientEx.UploadFile(path, stream, progress, token);
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning($"File uploading was canceled: {item.Base.Header}"); //TODO: localization
            }

            await RefreshRemoteImpl(unit, token);
        }
    }

    private async ValueTask DownloadImpl(Unit unit, CancellationToken token)
    {
        var item = RemoteSelectedItem.Value;
        if (item == null)
        {
            return;
        }

        var path = _localRootPath;
        if (LocalSelectedItem.Value != null)
        {
            path = Path.Combine(
                LocalSelectedItem.Value.Base.HasChildren
                    ? LocalSelectedItem.Value.Base.Path
                    : LocalSelectedItem.Value.Base.Path[
                        ..LocalSelectedItem.Value.Base.Path.LastIndexOf('\\')
                    ],
                RemoteSelectedItem.Value!.Base.Header!
            );
        }

        var res = await _dialogs.ShowYesNoDialog(
            "Download",
            $"Do you want to download file \'{item.Base.Header}\'?" //TODO: localization
        );

        if (res)
        {
            MemoryStream stream = new();
            try
            {
                var progress = new Progress<double>();
                progress.ProgressChanged += (_, value) => Progress.OnNext(value);
                await _clientEx.DownloadFile(item.Base.Path, stream, progress, cancel: token);
                await File.WriteAllBytesAsync(path, stream.ToArray(), token);
                _log.LogInformation(
                    $"File downloaded successfully: {RemoteSelectedItem.Value?.Base.Header}"
                );
            }
            catch (OperationCanceledException)
            {
                await File.WriteAllBytesAsync(path, stream.ToArray(), token);
                _log.LogWarning(
                    $"File downloading was canceled: {RemoteSelectedItem.Value?.Base.Header}" //TODO: localization
                );
            }
        }

        await RefreshLocalImpl(unit, token);
    }

    private async ValueTask BurstDownloadImpl(Unit unit, CancellationToken token)
    {
        var item = RemoteSelectedItem.Value;
        if (item == null)
        {
            return;
        }

        var path = _localRootPath;
        if (LocalSelectedItem.Value != null)
        {
            path = Path.Combine(
                LocalSelectedItem.Value.Base.HasChildren
                    ? LocalSelectedItem.Value.Base.Path
                    : LocalSelectedItem.Value.Base.Path[
                        ..LocalSelectedItem.Value.Base.Path.LastIndexOf('\\')
                    ],
                RemoteSelectedItem.Value!.Base.Header!
            );
        }

        var res = await _dialogs.ShowInputDialog(
            "Burst download",
            $"Do you want to burst-download file \'{item.Base.Header}\'?" //TODO: localization
        );

        if (byte.TryParse(res, out var burstSize) && burstSize is <= 239 and > 0)
        {
            MemoryStream stream = new();
            try
            {
                var progress = new Progress<double>();
                progress.ProgressChanged += (_, value) => Progress.OnNext(value);
                await _clientEx.BurstDownloadFile(
                    item.Base.Path,
                    stream,
                    progress,
                    burstSize,
                    token
                );
                await File.WriteAllBytesAsync(path, stream.ToArray(), token);
                _log.LogInformation(
                    $"File downloaded successfully: {RemoteSelectedItem.Value?.Base.Header}" //TODO: localization
                );
            }
            catch (OperationCanceledException)
            {
                await File.WriteAllBytesAsync(path, stream.ToArray(), token);
                _log.LogWarning(
                    $"File downloading was canceled: {RemoteSelectedItem.Value?.Base.Header}" //TODO: localization
                );
            }
        }

        await RefreshLocalImpl(unit, token);
    }

    private async ValueTask CalculateLocalCrc32Impl(Unit unit, CancellationToken token)
    {
        var path = await File.ReadAllBytesAsync(LocalSelectedItem.Value!.Base.Id.Id, token);
        var crc32 = Crc32Mavlink.Accumulate(path);
        var hexCrc32 = Crc32ToHex(crc32);
        LocalSelectedItem.Value.Base.Crc32Hex = hexCrc32;

        /*LocalSelectedItem.Value.Crc32Color =
            hexCrc32 == "00000000"
                ? FileSystemItemViewModel.BadCrcColor
                : FileSystemItemViewModel.DefaultColor;*/
    }

    private async ValueTask CalculateRemoteCrc32Impl(Unit unit, CancellationToken token)
    {
        var crc32 = await _clientEx.Base.CalcFileCrc32(RemoteSelectedItem.Value!.Base.Id.Id, token);
        var hexCrc32 = Crc32ToHex(crc32);
        RemoteSelectedItem.Value.Base.Crc32Hex = hexCrc32;

        RemoteSelectedItem.Value.Base.Crc32Hex = hexCrc32;

        /*RemoteSelectedItem.Value.Crc32Color =
            hexCrc32 == "00000000"
                ? FileSystemItemViewModel.BadCrcColor
                : FileSystemItemViewModel.DefaultColor;*/
    }

    private async ValueTask CompareSelectedItemsImpl(Unit unit, CancellationToken token)
    {
        var localFileCrc32 = Crc32Mavlink.Accumulate(
            await File.ReadAllBytesAsync(LocalSelectedItem.Value!.Base.Id.Id, token)
        );
        var remoteFileCrc32 = await _clientEx.Base.CalcFileCrc32(
            RemoteSelectedItem.Value!.Base.Id.Id,
            token
        );

        LocalSelectedItem.Value.Base.Crc32Hex = Crc32ToHex(localFileCrc32);
        RemoteSelectedItem.Value.Base.Crc32Hex = Crc32ToHex(remoteFileCrc32);

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
        if (LocalSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.Directory })
        {
            Directory.Delete(LocalSelectedItem.Value.Base.Path, true);
            await RefreshLocalImpl(Unit.Default, token);
        }

        if (LocalSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.File })
        {
            File.Delete(LocalSelectedItem.Value.Base.Path);
            await RefreshLocalImpl(Unit.Default, token);
        }
    }

    private async ValueTask RemoveRemoteItemImpl(Unit unit, CancellationToken token)
    {
        if (RemoteSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.Directory })
        {
            await _clientEx.Base.RemoveDirectory(RemoteSelectedItem.Value.Base.Path, token);
            await RefreshRemoteImpl(Unit.Default, token);
        }

        if (RemoteSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.File })
        {
            await _clientEx.Base.RemoveFile(RemoteSelectedItem.Value.Base.Path, token);
            await RefreshRemoteImpl(Unit.Default, token);
        }
    }

    private async ValueTask SetEditModeImpl(BrowserNode item, CancellationToken token)
    {
        item.Base.IsInEditMode.OnNext(true);
        item.Base.EditedName = item.Base.Header ?? "Unknown";
        await CommitEdit(item, token);
    }

    private async ValueTask CommitEdit(BrowserNode item, CancellationToken token)
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
        await RefreshRemoteImpl(Unit.Default, token);
        await CreateFolder(1);
        await RefreshRemoteImpl(Unit.Default, token);
        return;

        async Task CreateFolder(int n)
        {
            while (true)
            {
                var name = $"Folder{n}{MavlinkFtpHelper.DirectorySeparator}";
                if (RemoteSelectedItem.Value != null)
                {
                    var path =
                        RemoteSelectedItem.Value!.Base.FtpEntryType == FtpEntryType.Directory
                            ? Path.Combine(RemoteSelectedItem.Value.Base.Path, name)
                            : Path.Combine(
                                RemoteSelectedItem.Value.Base.Path[
                                    ..RemoteSelectedItem.Value.Base.Path.LastIndexOf(
                                        Path.PathSeparator
                                    )
                                ],
                                name
                            );

                    _clientEx
                        .Entries.FirstOrDefault(x => x.Key == path)
                        .Deconstruct(out var k, out _);
                    if (_clientEx.Entries.FirstOrDefault(x => x.Key == path).Equals(default))
                    {
                        n++;
                        continue;
                    }

                    await _clientEx.Base.CreateDirectory(path, token);
                }
                else
                {
                    var path = $"{MavlinkFtpHelper.DirectorySeparator}{name}";

                    _clientEx
                        .Entries.FirstOrDefault(x => x.Value.Name == name)
                        .Deconstruct(out var k, out _);
                    if (!string.IsNullOrEmpty(k))
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
        await RefreshLocalImpl(Unit.Default, token);
        return;

        void CreateFolder(int n)
        {
            while (true)
            {
                var name = $"Folder{n}";
                if (LocalSelectedItem.Value != null)
                {
                    var path =
                        LocalSelectedItem.Value.Base.FtpEntryType == FtpEntryType.Directory
                            ? Path.Combine(LocalSelectedItem.Value.Base.Path, name)
                            : Path.Combine(
                                LocalSelectedItem.Value.Base.Path[
                                    ..LocalSelectedItem.Value.Base.Path.LastIndexOf(
                                        Path.PathSeparator
                                    )
                                ],
                                name
                            );

                    if (Directory.Exists(path))
                    {
                        n++;
                        continue;
                    }

                    Directory.CreateDirectory(path);
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
        await _clientEx.Refresh(MavlinkFtpHelper.DirectorySeparator.ToString(), cancel: token);

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

            items.Add(new DirectoryItem(id, parentId, dir, name));
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

            items.Add(new FileItem(id, parentId, file, name, size));
        }
    }

    private ObservableList<IBrowserItem> LoadRemoteItems()
    {
        var items = new ObservableList<IBrowserItem>();

        _clientEx.Entries.ForEach(e =>
        {
            if (e.Value.Path == MavlinkFtpHelper.DirectorySeparator.ToString())
            {
                var root = new DirectoryItem(
                    "_",
                    NavigationId.Empty,
                    MavlinkFtpHelper.DirectorySeparator.ToString(),
                    "_"
                );
                items.Add(root);
                return;
            }

            var item = e.Value.Type switch
            {
                FtpEntryType.Directory => new DirectoryItem(
                    NavigationId.NormalizeTypeId(e.Value.Path),
                    NavigationId.NormalizeTypeId(e.Value.ParentPath),
                    e.Key,
                    e.Value.Name
                ),
                FtpEntryType.File => new FileItem(
                    NavigationId.NormalizeTypeId(e.Value.Path),
                    NavigationId.NormalizeTypeId(e.Value.ParentPath),
                    e.Key,
                    e.Value.Name,
                    ((FtpFile)e.Value).Size
                ),
                _ => new BrowserItem(
                    NavigationId.NormalizeTypeId(e.Value.Path),
                    NavigationId.NormalizeTypeId(e.Value.ParentPath),
                    e.Key
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
