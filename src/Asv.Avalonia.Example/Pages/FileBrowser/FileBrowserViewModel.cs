using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
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

        LocalItemsView = new BrowserTree(_localSource);
        RemoteItemsView = new BrowserTree(_remoteSource);

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
    public ReactiveCommand UploadCommand { get; set; }
    public ReactiveCommand DownloadCommand { get; set; }
    public ReactiveCommand BurstDownloadCommand { get; set; }
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
        ShowDownloadPopupCommand = CanDownload.ToReactiveCommand(_ =>
            IsDownloadPopupOpen.OnNext(!IsDownloadPopupOpen.Value)
        );
        UploadCommand = CanUpload.ToReactiveCommand(_ => UploadImpl());
        DownloadCommand = CanDownload.ToReactiveCommand(_ => DownloadImpl());
        BurstDownloadCommand = CanDownload.ToReactiveCommand(_ => BurstDownloadImpl());
        CreateRemoteFolderCommand = new ReactiveCommand(_ => CreateRemoteFolderImpl());
        CreateLocalFolderCommand = new ReactiveCommand(_ => CreateLocalFolderImpl());
        RefreshRemoteCommand = new ReactiveCommand(async _ => await RefreshRemoteImpl());
        RefreshLocalCommand = new ReactiveCommand(_ => RefreshLocalImpl());
        RemoveLocalItemCommand = new ReactiveCommand(_ => RemoveLocalItemImpl());
        RemoveRemoteItemCommand = new ReactiveCommand(_ => RemoveRemoteItemImpl());
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

        RefreshRemoteCommand.IgnoreOnErrorResume(e =>
        {
            if (e is not FtpNackEndOfFileException)
            {
                throw e;
            }
        });

        RefreshRemoteCommand.SubscribeOnUIThreadDispatcher();
        RefreshLocalCommand.SubscribeOnUIThreadDispatcher();

        _sub1 = RemoveRemoteItemCommand.Subscribe(async _ => await RefreshRemoteImpl());
        _sub2 = RemoveLocalItemCommand.Subscribe(_ => RefreshLocalImpl());

        RefreshLocalCommand.Execute(Unit.Default);
        RefreshRemoteCommand.Execute(Unit.Default);
    }

    private async void UploadImpl()
    {
        var item = LocalSelectedItem.Value;
        if (item is null)
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
                    ? Path.Combine(
                        MavlinkFtpHelper.DirectorySeparator.ToString(),
                        item.Base.Header ?? "unknown"
                    )
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
                await ClientEx.UploadFile(path, stream, progress);
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning($"File uploading was canceled: {item.Base.Header}"); //TODO: localization
            }

            await RefreshRemoteImpl();
        }
    }

    private async void DownloadImpl()
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
                    $"File downloading was canceled: {RemoteSelectedItem.Value?.Base.Header}" //TODO: localization
                );
            }
        }

        RefreshLocalImpl();
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
        var process = await res.ApplyDialog();

        if (process)
        {
            MemoryStream stream = new();
            var progress = new Progress<double>();
            EventHandler<double> progressHandler = (_, value) => Progress.OnNext(value);
            progress.ProgressChanged += (_, value) => progressHandler.Invoke(_, value);

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
                    $"File downloaded successfully: {RemoteSelectedItem.Value?.Base.Header}" //TODO: localization
                );
            }
            catch (OperationCanceledException)
            {
                await File.WriteAllBytesAsync(path, stream.ToArray());
                _log.LogWarning(
                    $"File downloading was canceled: {RemoteSelectedItem.Value?.Base.Header}" //TODO: localization
                );
            }
            finally
            {
                progress.ProgressChanged -= progressHandler;
            }
        }

        RefreshLocalImpl();
    }

    private void CalculateLocalCrc32Impl() { }

    private void CalculateRemoteCrc32Impl() { }

    private void CompareSelectedItemsImpl() { }

    private void FindFileOnLocalImpl() { }

    private void ClearLocalSearchBoxImpl() => LocalSearchText.OnNext(string.Empty);

    private void ClearRemoteSearchBoxImpl() => RemoteSearchText.OnNext(string.Empty);

    private void RemoveLocalItemImpl()
    {
        if (LocalSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.Directory })
        {
            Directory.Delete(LocalSelectedItem.Value.Base.Path, true);
        }

        if (LocalSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.File })
        {
            File.Delete(LocalSelectedItem.Value.Base.Path);
        }
    }

    private async void RemoveRemoteItemImpl()
    {
        if (RemoteSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.Directory })
        {
            await ClientEx.Base.RemoveDirectory(RemoteSelectedItem.Value.Base.Path);
        }

        if (RemoteSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.File })
        {
            await ClientEx.Base.RemoveFile(RemoteSelectedItem.Value.Base.Path);
        }
    }

    private void SetEditModeImpl(BrowserNode item) { }

    private ValueTask CommitEdit(BrowserNode item)
    {
        return ValueTask.CompletedTask;
    }

    private async void CreateRemoteFolderImpl()
    {
        await RefreshRemoteImpl();
        await CreateFolder(1);
        await RefreshRemoteImpl();
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
                                        Path.DirectorySeparatorChar
                                    )
                                ],
                                name
                            );

                    ClientEx
                        .Entries.FirstOrDefault(x => x.Key == path)
                        .Deconstruct(out var k, out _);
                    if (ClientEx.Entries.FirstOrDefault(x => x.Key == path).Equals(null))
                    {
                        n++;
                        continue;
                    }

                    await ClientEx.Base.CreateDirectory(path);
                }
                else
                {
                    var path = $"{MavlinkFtpHelper.DirectorySeparator}{name}";

                    ClientEx
                        .Entries.FirstOrDefault(x => x.Value.Name == name)
                        .Deconstruct(out var k, out _);
                    if (!string.IsNullOrEmpty(k))
                    {
                        n++;
                        continue;
                    }

                    await ClientEx.Base.CreateDirectory(path);
                }

                break;
            }
        }
    }

    private void CreateLocalFolderImpl()
    {
        CreateFolder(1);
        RefreshLocalImpl();
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
                                        Path.DirectorySeparatorChar
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

    private async ValueTask RefreshRemoteImpl()
    {
        await ClientEx.Refresh(MavlinkFtpHelper.DirectorySeparator.ToString());

        _remoteSource.Clear();
        _remoteSource.AddRange(LoadRemoteItems());
    }

    private void RefreshLocalImpl() // TODO: this method takes more and more time every time it's called
    {
        _localSource.Clear();
        _localSource.AddRange(LoadLocalItems());
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
                var parentRelative = Path.GetRelativePath(_localRootPath, parentPath);
                var parentId = NavigationId.NormalizeTypeId(parentRelative);
                var name = new DirectoryInfo(dir).Name;

                lock (items)
                {
                    items.Add(new DirectoryItem(id, parentId, dir, name));
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
                var parentRelative = Path.GetRelativePath(_localRootPath, parentPath);
                var parentId = NavigationId.NormalizeTypeId(parentRelative);
                var fileInfo = new FileInfo(file);

                lock (items)
                {
                    items.Add(new FileItem(id, parentId, file, fileInfo.Name, fileInfo.Length));
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
    private IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();

            Client?.Dispose();
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
