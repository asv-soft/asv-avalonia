using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

[ExportPage(PageId)]
public class FileBrowserViewModel : DevicePageViewModel<FileBrowserViewModel>
{
    public const string PageId = "files.browser";
    public const MaterialIconKind PageIcon = MaterialIconKind.FolderEye;

    private readonly IDialogService _dialogs;
    private readonly IFtpService _svc;
    private readonly ILogger<FileBrowserViewModel> _log;
    private readonly INavigationService _navigation;
    private readonly string _localRootPath;

    private readonly ObservableList<IBrowserItem> _localSource;
    private readonly ObservableList<IBrowserItem> _remoteSource;

    public FileBrowserViewModel()
        : this(DesignTime.CommandService, null!, null!, null!, null!, null!, DesignTime.Navigation)
    {
        DesignTime.ThrowIfNotDesignMode();

        _localSource =
        [
            new DirectoryItem("0", string.Empty, "/", "test"),
            new DirectoryItem("1", "0", "/", "test"),
            new DirectoryItem("2", "0", "/", "test"),
            new DirectoryItem("0_1", string.Empty, "/", "test"),
            new DirectoryItem("0_2", string.Empty, "/", "test"),
        ];
        _remoteSource =
        [
            new DirectoryItem("0", string.Empty, "/", "test"),
            new DirectoryItem("1", "0", "/", "test"),
            new DirectoryItem("2", "0", "/", "test"),
            new DirectoryItem("0_1", string.Empty, "/", "test"),
            new DirectoryItem("0_2", string.Empty, "/", "test"),
        ];

        LocalItemsView = new BrowserTree(_localSource, _localRootPath);
        RemoteItemsView = new BrowserTree(
            _remoteSource,
            MavlinkFtpHelper.DirectorySeparator.ToString()
        );
    }

    [ImportingConstructor]
    public FileBrowserViewModel(
        ICommandService cmd,
        IDeviceManager devices,
        IDialogService dialogs,
        IFtpService svc,
        IAppPath appPath,
        ILoggerFactory log,
        INavigationService navigation
    )
        : base(PageId, devices, cmd)
    {
        _localRootPath = appPath.UserDataFolder;
        _dialogs = dialogs;
        _svc = svc;
        _log = log.CreateLogger<FileBrowserViewModel>();
        _navigation = navigation;

        _localSource = [];
        _remoteSource = [];

        LocalItemsView = new BrowserTree(_localSource, _localRootPath);
        RemoteItemsView = new BrowserTree(
            _remoteSource,
            MavlinkFtpHelper.DirectorySeparator.ToString()
        );

        LocalSearchText = new BindableReactiveProperty<string>();
        RemoteSearchText = new BindableReactiveProperty<string>();
        LocalSelectedItem = new BindableReactiveProperty<BrowserNode?>(null);
        RemoteSelectedItem = new BindableReactiveProperty<BrowserNode?>(null);
        Progress = new BindableReactiveProperty<double>(0);
        IsDownloadPopupOpen = new BindableReactiveProperty<bool>(false);
    }

    public IFtpClient? Client { get; private set; }
    public IFtpClientEx ClientEx { get; private set; }
    public BrowserTree LocalItemsView { get; }
    public BrowserTree RemoteItemsView { get; }
    public BindableReactiveProperty<BrowserNode?> LocalSelectedItem { get; set; }
    public BindableReactiveProperty<BrowserNode?> RemoteSelectedItem { get; set; }
    public BindableReactiveProperty<string> LocalSearchText { get; set; }
    public BindableReactiveProperty<string> RemoteSearchText { get; set; }
    public BindableReactiveProperty<double> Progress { get; set; }
    public BindableReactiveProperty<bool> IsDownloadPopupOpen { get; set; }

    #region Commands

    public ReactiveCommand ShowDownloadPopupCommand { get; set; }
    public ReactiveCommand<Unit> UploadCommand { get; set; }
    public ReactiveCommand<Unit> DownloadCommand { get; set; }
    public ReactiveCommand<Unit> BurstDownloadCommand { get; set; }
    public ReactiveCommand CreateRemoteFolderCommand { get; set; }
    public ReactiveCommand CreateLocalFolderCommand { get; set; }
    public ReactiveCommand RefreshRemoteCommand { get; set; }
    public ReactiveCommand RefreshLocalCommand { get; set; }
    public ReactiveCommand RemoveLocalItemCommand { get; set; }
    public ReactiveCommand RemoveRemoteItemCommand { get; set; }
    public ReactiveCommand<BrowserNode> SetInEditModeCommand { get; set; } // TODO: implement
    public ReactiveCommand ClearLocalSearchBoxCommand { get; set; } // TODO: implement
    public ReactiveCommand ClearRemoteSearchBoxCommand { get; set; } // TODO: implement
    public ReactiveCommand CompareSelectedItemsCommand { get; set; } // TODO: implement
    public ReactiveCommand FindFileOnLocalCommand { get; set; } // TODO: implement
    public ReactiveCommand CalculateLocalCrc32Command { get; set; } // TODO: implement
    public ReactiveCommand CalculateRemoteCrc32Command { get; set; } // TODO: implement

    private Observable<bool> CanUpload =>
        LocalSelectedItem.Select(x => x?.Base is { FtpEntryType: FtpEntryType.File });

    private Observable<bool> CanDownload =>
        RemoteSelectedItem.Select(x => x?.Base is { FtpEntryType: FtpEntryType.File });

    private Observable<bool> CanEdit =>
        LocalSelectedItem.Select(x => x?.Base is { IsInEditMode: false });

    private Observable<bool> CanFindFileOnLocal =>
        RemoteSelectedItem.Select(x => x?.Base is { IsInEditMode: false });

    private Observable<bool> CanCompareSelectedItems =>
        LocalSelectedItem.CombineLatest(
            RemoteSelectedItem,
            (local, remote) =>
                local?.Base is { IsInEditMode: false, FtpEntryType: FtpEntryType.File }
                && remote?.Base is { IsInEditMode: false, FtpEntryType: FtpEntryType.File }
        );

    private Observable<bool> CanCalculateRemoteCrc32 =>
        RemoteSelectedItem.Select(x =>
            x?.Base is { IsInEditMode: false, FtpEntryType: FtpEntryType.File }
        );

    private Observable<bool> CanCalculateLocalCrc32 =>
        LocalSelectedItem.Select(x =>
            x?.Base is { IsInEditMode: false, FtpEntryType: FtpEntryType.File }
        );

    #endregion

    #region Commands implementation

    private void InitCommands()
    {
        // Not implemented
        SetInEditModeCommand = CanEdit.ToReactiveCommand<BrowserNode>(SetEditModeImpl);
        ClearLocalSearchBoxCommand = new ReactiveCommand(_ => ClearLocalSearchBoxImpl());
        ClearRemoteSearchBoxCommand = new ReactiveCommand(_ => ClearRemoteSearchBoxImpl());
        FindFileOnLocalCommand = CanFindFileOnLocal.ToReactiveCommand(_ => FindFileOnLocalImpl());
        CompareSelectedItemsCommand = CanCompareSelectedItems.ToReactiveCommand(_ =>
            CompareSelectedItemsImpl()
        );
        CalculateLocalCrc32Command = CanCalculateLocalCrc32.ToReactiveCommand(_ =>
            CalculateLocalCrc32Impl()
        );
        CalculateRemoteCrc32Command = CanCalculateRemoteCrc32.ToReactiveCommand(_ =>
            CalculateRemoteCrc32Impl()
        );

        // Implemented
        RefreshRemoteCommand = new ReactiveCommand(async (_, ct) => await RefreshRemoteImpl(ct));
        RefreshLocalCommand = new ReactiveCommand(async (_, _) => await RefreshLocalImpl());

        UploadCommand = CanUpload.ToReactiveCommand<Unit>(
            async (_, ct) =>
            {
                await UploadImpl();
                await RefreshRemoteImpl(ct);
            },
            awaitOperation: AwaitOperation.Drop
        );
        DownloadCommand = CanDownload.ToReactiveCommand<Unit>(
            async (_, _) =>
            {
                await DownloadImpl();
                await RefreshLocalImpl();
            },
            awaitOperation: AwaitOperation.Drop
        );
        BurstDownloadCommand = CanDownload.ToReactiveCommand<Unit>(
            async (_, _) =>
            {
                await BurstDownloadImpl();
                await RefreshLocalImpl();
            },
            awaitOperation: AwaitOperation.Drop
        );
        CreateRemoteFolderCommand = new ReactiveCommand(
            async (_, ct) =>
            {
                await CreateRemoteFolderImpl();
                await RefreshRemoteImpl(ct);
            },
            awaitOperation: AwaitOperation.Drop
        );
        CreateLocalFolderCommand = new ReactiveCommand(
            async (_, _) =>
            {
                await CreateLocalFolderImpl();
                await RefreshLocalImpl();
            },
            awaitOperation: AwaitOperation.Drop
        );
        RemoveLocalItemCommand = new ReactiveCommand(
            async (_, _) =>
            {
                await RemoveLocalItemImpl();
                await RefreshLocalImpl();
            },
            awaitOperation: AwaitOperation.Drop
        );
        RemoveRemoteItemCommand = new ReactiveCommand(
            async (_, ct) =>
            {
                await RemoveRemoteItemImpl();
                await RefreshRemoteImpl(ct);
            },
            awaitOperation: AwaitOperation.Drop
        );

        RefreshRemoteCommand.IgnoreOnErrorResume(e =>
        {
            if (e is not FtpNackEndOfFileException)
            {
                throw e;
            }
        });

        ShowDownloadPopupCommand = CanDownload.ToReactiveCommand(_ =>
            IsDownloadPopupOpen.OnNext(!IsDownloadPopupOpen.Value)
        );

        _sub1 = CanDownload.Subscribe(b =>
        {
            if (!b)
            {
                IsDownloadPopupOpen.OnNext(false);
            }
        });

        RefreshRemoteCommand.SubscribeOnUIThreadDispatcher();
        RefreshLocalCommand.SubscribeOnUIThreadDispatcher();

        RefreshRemoteCommand.Execute(Unit.Default);
        RefreshLocalCommand.Execute(Unit.Default);
    }

    private async Task UploadImpl()
    {
        var item = LocalSelectedItem.Value;
        if (item is null)
        {
            return;
        }

        var res = await _dialogs.ShowYesNoDialog(
            RS.FileBrowserViewModel_UploadingDialog_Title,
            string.Format(RS.FileBrowserViewModel_UploadingDialog_Message, item.Base.Header)
        );

        var stream = new FileStream(item.Base.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var progress = new Progress<double>();
        EventHandler<double> progressHandler = (_, value) => Progress.OnNext(value);
        progress.ProgressChanged += (o, value) => progressHandler.Invoke(o, value);

        if (res)
        {
            var path = $"{MavlinkFtpHelper.DirectorySeparator}unknown";
            if (RemoteSelectedItem.Value != null)
            {
                path = Path.Combine(
                    RemoteSelectedItem.Value.Base.HasChildren
                        ? RemoteSelectedItem.Value.Base.Path
                        : RemoteSelectedItem.Value.Base.Path[
                            ..RemoteSelectedItem.Value.Base.Path.LastIndexOf(
                                MavlinkFtpHelper.DirectorySeparator
                            )
                        ],
                    LocalSelectedItem.Value?.Base.Header ?? "unknown"
                );
            }

            try
            {
                await ClientEx.UploadFile(path, stream, progress);
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning($"File uploading was canceled: {item.Base.Header}");
            }
            finally
            {
                progress.ProgressChanged -= progressHandler;
                await stream.DisposeAsync();
            }
        }
    }

    private async Task DownloadImpl()
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
                        ..LocalSelectedItem.Value.Base.Path.LastIndexOf(Path.DirectorySeparatorChar)
                    ],
                RemoteSelectedItem.Value!.Base.Header!
            );
        }
        else
        {
            path = Path.Combine(path, RemoteSelectedItem.Value!.Base.Header!);
        }

        var res = await _dialogs.ShowYesNoDialog(
            RS.FileBrowserViewModel_DownloadDialog_Title,
            string.Format(RS.FileBrowserViewModel_DownloadDialog_Message, item.Base.Header)
        );

        if (res)
        {
            MemoryStream stream = new();
            var progress = new Progress<double>();
            EventHandler<double> progressHandler = (_, value) => Progress.OnNext(value);
            progress.ProgressChanged += (o, value) => progressHandler.Invoke(o, value);
            try
            {
                await ClientEx.DownloadFile(item.Base.Path, stream, progress);
                await File.WriteAllBytesAsync(path, stream.ToArray());
                _log.LogInformation(
                    $"File downloaded successfully: {RemoteSelectedItem.Value?.Base.Header}"
                );
            }
            catch (OperationCanceledException)
            {
                await File.WriteAllBytesAsync(path, stream.ToArray());
                _log.LogWarning(
                    $"File downloading was canceled: {RemoteSelectedItem.Value?.Base.Header}"
                );
            }
            finally
            {
                progress.ProgressChanged -= progressHandler;
                await stream.DisposeAsync();
            }
        }
    }

    private async Task BurstDownloadImpl()
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
                        ..LocalSelectedItem.Value.Base.Path.LastIndexOf(Path.DirectorySeparatorChar)
                    ],
                RemoteSelectedItem.Value!.Base.Header!
            );
        }
        else
        {
            path = Path.Combine(path, RemoteSelectedItem.Value!.Base.Header!);
        }

        var res = new BurstDownloadDialogViewModel("burst.dialog", _svc, _navigation);
        await res.ApplyDialog();

        if (res.CanDownload)
        {
            MemoryStream stream = new();
            var progress = new Progress<double>();
            EventHandler<double> progressHandler = (_, value) => Progress.OnNext(value);
            progress.ProgressChanged += (o, value) => progressHandler.Invoke(o, value);

            try
            {
                await ClientEx.BurstDownloadFile(
                    item.Base.Path,
                    stream,
                    progress,
                    _svc.BurstDownloadPacketSize
                );
                await File.WriteAllBytesAsync(path, stream.ToArray());
                _log.LogInformation(
                    $"File downloaded successfully: {RemoteSelectedItem.Value?.Base.Header}"
                );
            }
            catch (OperationCanceledException)
            {
                await File.WriteAllBytesAsync(path, stream.ToArray());
                _log.LogWarning(
                    $"File downloading was canceled: {RemoteSelectedItem.Value?.Base.Header}"
                );
            }
            finally
            {
                progress.ProgressChanged -= progressHandler;
                await stream.DisposeAsync();
            }
        }
    }

    private void CalculateLocalCrc32Impl() { }

    private void CalculateRemoteCrc32Impl() { }

    private void CompareSelectedItemsImpl() { }

    private void FindFileOnLocalImpl() { }

    private void ClearLocalSearchBoxImpl() => LocalSearchText.OnNext(string.Empty);

    private void ClearRemoteSearchBoxImpl() => RemoteSearchText.OnNext(string.Empty);

    private async Task RemoveLocalItemImpl()
    {
        var res = await _dialogs.ShowYesNoDialog(
            RS.FileBrowserViewModel_RemoveDialog_Title,
            RS.FileBrowserViewModel_RemoveDialog_Message
        );
        if (!res)
        {
            return;
        }

        if (LocalSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.Directory })
        {
            Directory.Delete(LocalSelectedItem.Value.Base.Path, true);
        }

        if (LocalSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.File })
        {
            File.Delete(LocalSelectedItem.Value.Base.Path);
        }
    }

    private async Task RemoveRemoteItemImpl()
    {
        var res = await _dialogs.ShowYesNoDialog(
            RS.FileBrowserViewModel_RemoveDialog_Title,
            RS.FileBrowserViewModel_RemoveDialog_Message
        );
        if (!res)
        {
            return;
        }

        if (RemoteSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.Directory })
        {
            await RemoveDirectoryRecursive(RemoteSelectedItem.Value.Base.Path);
        }

        if (RemoteSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.File })
        {
            await ClientEx.Base.RemoveFile(RemoteSelectedItem.Value.Base.Path);
        }

        return;

        async Task RemoveDirectoryRecursive(string directoryPath)
        {
            var itemsInDir = ClientEx
                .Entries.Where(x => x.Value.ParentPath == directoryPath)
                .ToList();

            foreach (var item in itemsInDir)
            {
                switch (item.Value.Type)
                {
                    case FtpEntryType.Directory:
                        await RemoveDirectoryRecursive(item.Key);
                        break;
                    case FtpEntryType.File:
                        await ClientEx.Base.RemoveFile(item.Key);
                        break;
                    default:
                        _log.LogError($"Unknown FTP entry type: ({item.Value.Type})");
                        break;
                }
            }

            await ClientEx.Base.RemoveDirectory(directoryPath);
        }
    }

    private void SetEditModeImpl(BrowserNode item) { }

    private async Task CreateRemoteFolderImpl()
    {
        await CreateFolder(1);
        return;

        async Task CreateFolder(int n)
        {
            while (true)
            {
                var name = $"Folder{n}{MavlinkFtpHelper.DirectorySeparator}";
                string path;
                if (RemoteSelectedItem.Value != null)
                {
                    path =
                        RemoteSelectedItem.Value!.Base.FtpEntryType == FtpEntryType.Directory
                            ? Path.Combine(RemoteSelectedItem.Value.Base.Path, name)
                            : Path.Combine(
                                RemoteSelectedItem.Value.Base.Path[
                                    ..RemoteSelectedItem.Value.Base.Path.LastIndexOf(
                                        MavlinkFtpHelper.DirectorySeparator
                                    )
                                ],
                                name
                            );
                }
                else
                {
                    path = $"{MavlinkFtpHelper.DirectorySeparator}{name}";
                }

                ClientEx.Entries.FirstOrDefault(x => x.Key == path).Deconstruct(out var k, out _);
                if (!string.IsNullOrEmpty(k))
                {
                    n++;
                    continue;
                }

                await ClientEx.Base.CreateDirectory(path);

                break;
            }
        }
    }

    private Task CreateLocalFolderImpl()
    {
        CreateFolder(1);
        return Task.CompletedTask;

        void CreateFolder(int n)
        {
            while (true)
            {
                var name = $"Folder{n}";
                string path;
                if (LocalSelectedItem.Value != null)
                {
                    path =
                        LocalSelectedItem.Value.Base.FtpEntryType == FtpEntryType.Directory
                            ? Path.Combine(LocalSelectedItem.Value.Base.Path, name)
                            : Path.Combine(
                                LocalSelectedItem.Value.Base.Path[
                                    ..LocalSelectedItem.Value.Base.Path.LastIndexOf(
                                        Path.DirectorySeparatorChar
                                    )
                                ],
                                name
                            );
                }
                else
                {
                    path = Path.Combine(_localRootPath, name);
                }

                if (Directory.Exists(path))
                {
                    n++;
                    continue;
                }

                Directory.CreateDirectory(path);

                break;
            }
        }
    }

    private async Task RefreshRemoteImpl(CancellationToken ct)
    {
        await ClientEx.Refresh(MavlinkFtpHelper.DirectorySeparator.ToString(), cancel: ct);
        var newItems = LoadRemoteItems();

        var toRemove = _remoteSource.Where(rs => newItems.All(n => n.Path != rs.Path)).ToList();
        foreach (var item in toRemove)
        {
            _remoteSource.Remove(item);
        }

        var toAdd = newItems.Where(n => _remoteSource.All(rs => rs.Path != n.Path)).ToList();
        _remoteSource.AddRange(toAdd);
    }

    private Task RefreshLocalImpl()
    {
        var newItems = LoadLocalItems();

        var toRemove = _localSource.Where(ls => newItems.All(n => n.Path != ls.Path)).ToList();
        foreach (var item in toRemove)
        {
            _localSource.Remove(item);
        }

        var toAdd = newItems.Where(n => _localSource.All(ls => ls.Path != n.Path)).ToList();
        _localSource.AddRange(toAdd);

        return Task.CompletedTask;
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
        var directories = Directory.EnumerateDirectories(directoryPath).ToList();
        var files = Directory.EnumerateFiles(directoryPath).ToList();

        Parallel.ForEach(
            directories,
            dir =>
            {
                var relativeDir = Path.GetRelativePath(_localRootPath, dir);
                var id = NavigationId.NormalizeTypeId(relativeDir);
                var parentPath = Directory.GetParent(dir)?.FullName ?? _localRootPath;
                var name = new DirectoryInfo(dir).Name;

                lock (items)
                {
                    items.Add(new DirectoryItem(id, parentPath, dir, name));
                }

                ProcessDirectory(dir, items);
            }
        );

        Parallel.ForEach(
            files,
            file =>
            {
                var relativeFile = Path.GetRelativePath(_localRootPath, file);
                var id = NavigationId.NormalizeTypeId(relativeFile);
                var parentPath = Directory.GetParent(file)?.FullName ?? _localRootPath;
                var fileInfo = new FileInfo(file);

                lock (items)
                {
                    items.Add(new FileItem(id, parentPath, file, fileInfo.Name, fileInfo.Length));
                }
            }
        );
    }

    private ObservableList<IBrowserItem> LoadRemoteItems()
    {
        var items = new ObservableList<IBrowserItem>();

        ClientEx.Entries.ForEach(e =>
        {
            if (e.Value.Path == MavlinkFtpHelper.DirectorySeparator.ToString())
            {
                var root = new DirectoryItem(
                    "_",
                    string.Empty,
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
                    e.Value.ParentPath,
                    e.Key,
                    e.Value.Name
                ),
                FtpEntryType.File => new FileItem(
                    NavigationId.NormalizeTypeId(e.Value.Path),
                    e.Value.ParentPath,
                    e.Key,
                    e.Value.Name,
                    ((FtpFile)e.Value).Size
                ),
                _ => new BrowserItem(
                    NavigationId.NormalizeTypeId(e.Value.Path),
                    e.Value.ParentPath,
                    e.Key
                ),
            };

            items.Add(item);
        });

        return items;
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

    protected override void AfterDeviceInitialized(IClientDevice device)
    {
        Title.Value = $"Browser[{device.Id}]";
        Client = device.GetMicroservice<IFtpClient>();
        ArgumentNullException.ThrowIfNull(Client);
        ClientEx = device.GetMicroservice<IFtpClientEx>() ?? new FtpClientEx(Client);
        InitCommands();
    }

    public override IExportInfo Source => SystemModule.Instance;

    #region Dispose

    private IDisposable _sub1;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();

            _localSource.Clear();
            _remoteSource.Clear();

            LocalItemsView.Dispose();
            RemoteItemsView.Dispose();

            ShowDownloadPopupCommand.Dispose();
            UploadCommand.Dispose();
            DownloadCommand.Dispose();
            BurstDownloadCommand.Dispose();
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
            CalculateLocalCrc32Command.Dispose();
            CalculateRemoteCrc32Command.Dispose();

            Progress.Dispose();
            IsDownloadPopupOpen.Dispose();
            LocalSearchText.Dispose();
            RemoteSearchText.Dispose();
            LocalSelectedItem.Dispose();
            RemoteSelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
