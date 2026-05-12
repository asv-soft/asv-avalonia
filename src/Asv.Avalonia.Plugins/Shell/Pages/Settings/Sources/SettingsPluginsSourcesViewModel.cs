using System.Text.Json;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

public class SettingsPluginsSourcesViewModel : SettingsSubPage
{
    public const string PageId = "plugins.sources";

    private readonly IPluginManager _pluginManager;
    private readonly IShellHost _shellHost;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ObservableList<IPluginServerInfo> _sources;
    private readonly ISynchronizedView<IPluginServerInfo, PluginsSourceViewModel> _view;
    private readonly ILogger<SettingsPluginsSourcesViewModel> _logger;
    private readonly YesOrNoDialogPrefab? _yesOrNoDialogPrefab;
    private readonly IUndoChangeSink<ValueUndoChange<string>> _undoSink;

    public SettingsPluginsSourcesViewModel()
        : this(
            NullTreeSubPageContext<SettingsPageViewModel>.Instance,
            NullPluginManager.Instance,
            NullDialogService.Instance,
            NullShellHost.Instance,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SettingsPluginsSourcesViewModel(
        ITreeSubPageContext<ISettingsPage> context,
        IPluginManager pluginManager,
        IDialogService dialogService,
        IShellHost shellHost,
        ILoggerFactory loggerFactory
    )
        : base(PageId, context)
    {
        ArgumentNullException.ThrowIfNull(pluginManager);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(shellHost);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _logger = loggerFactory.CreateLogger<SettingsPluginsSourcesViewModel>();
        _pluginManager = pluginManager;
        _shellHost = shellHost;
        _loggerFactory = loggerFactory;
        _yesOrNoDialogPrefab = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _undoSink = Undo.CreateValueChange<string>(
                "default",
                ApplySourcesSnapshot,
                ApplySourcesSnapshot
            )
            .DisposeItWith(Disposable);

        SelectedItem = new BindableReactiveProperty<PluginsSourceViewModel?>().DisposeItWith(
            Disposable
        );

        _sources = new ObservableList<IPluginServerInfo>(_pluginManager.Servers);
        _view = _sources
            .CreateView(info => new PluginsSourceViewModel(
                info,
                EditSourceAsync,
                RemoveSourceAsync
            ))
            .DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        _view.DisposeMany().DisposeItWith(Disposable);
        Items = _view
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        var add = new MenuItem("add", RS.SettingsPluginsSourcesViewModel_MenuItem_Add_Title)
        {
            Order = 0,
            Icon = MaterialIconKind.Add,
            Command = new ReactiveCommand(AddSourceAsync).DisposeItWith(Disposable),
        };

        var refresh = new MenuItem("refresh", string.Empty)
        {
            Order = 1,
            Icon = MaterialIconKind.Refresh,
            Command = new ReactiveCommand(_ => Refresh()).DisposeItWith(Disposable),
        };
        Menu.Add(add);
        Menu.Add(refresh);
    }

    public NotifyCollectionChangedSynchronizedViewList<PluginsSourceViewModel> Items { get; }
    public BindableReactiveProperty<PluginsSourceViewModel?> SelectedItem { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        foreach (var child in base.GetChildren())
        {
            yield return child;
        }

        foreach (var view in _view)
        {
            yield return view;
        }
    }

    internal void Refresh()
    {
        try
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                _sources.RemoveAll();
                _sources.AddRange(_pluginManager.Servers);
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error to update info about plugin sources");
        }
    }

    private async ValueTask AddSourceAsync(Unit unit, CancellationToken cancel)
    {
        var oldSnapshot = CaptureSourcesSnapshot();
        var server = await ShowSourceDialogAsync(
            new SourceDialogViewModel(_loggerFactory),
            RS.SettingsPluginsSourcesViewModel_AddDialog_Title,
            RS.SettingsPluginsSourcesViewModel_AddDialog_PrimaryButtonText
        );

        if (server is null)
        {
            return;
        }

        var newSnapshot = AddSourceSnapshot(oldSnapshot, ToSnapshot(server));
        ApplySourcesSnapshot(newSnapshot);
        _undoSink.Publish(oldSnapshot, newSnapshot);
    }

    private async ValueTask EditSourceAsync(PluginsSourceViewModel source)
    {
        var oldSnapshot = CaptureSourcesSnapshot();
        var server = await ShowSourceDialogAsync(
            new SourceDialogViewModel(_loggerFactory, source),
            RS.PluginsSourceViewModel_EditDialog_Title,
            RS.PluginsSourceViewModel_EditDialog_PrimaryButtonText
        );

        if (server is null)
        {
            return;
        }

        var newSnapshot = ReplaceSourceSnapshot(
            oldSnapshot,
            source.Model.SourceUri,
            ToSnapshot(server)
        );
        ApplySourcesSnapshot(newSnapshot);
        _undoSink.Publish(oldSnapshot, newSnapshot);
    }

    private async ValueTask RemoveSourceAsync(PluginsSourceViewModel source)
    {
        var payload = new YesOrNoDialogPayload
        {
            Title = RS.PluginsSourceViewModel_RemoveDialog_Title,
            Message = RS.PluginsSourceViewModel_RemoveDialog_Message,
        };

        if (_yesOrNoDialogPrefab is null || !await _yesOrNoDialogPrefab.ShowDialogAsync(payload))
        {
            return;
        }

        var oldSnapshot = CaptureSourcesSnapshot();
        var newSnapshot = RemoveSourceSnapshot(oldSnapshot, source.Model.SourceUri);
        ApplySourcesSnapshot(newSnapshot);
        _undoSink.Publish(oldSnapshot, newSnapshot);
    }

    private async Task<PluginServer?> ShowSourceDialogAsync(
        SourceDialogViewModel viewModel,
        string title,
        string primaryButtonText
    )
    {
        using (viewModel)
        {
            var dialog = new ContentDialog(viewModel)
            {
                Title = title,
                PrimaryButtonText = primaryButtonText,
                IsSecondaryButtonEnabled = true,
                CloseButtonText = Avalonia.RS.DialogButton_Cancel,
            };

            viewModel.ApplyDialog(dialog);

            var result = _shellHost.TopLevel is { } topLevel
                ? await dialog.ShowAsync(topLevel)
                : await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
                return null;
            }

            return new PluginServer(
                viewModel.Name.Value,
                viewModel.SourceUri.Value,
                viewModel.Username.Value,
                viewModel.Password.Value
            );
        }
    }

    private string CaptureSourcesSnapshot()
    {
        return Serialize(_pluginManager.Servers.Select(ToSnapshot));
    }

    private void ApplySourcesSnapshot(string snapshot)
    {
        var sources = Deserialize(snapshot);
        foreach (var server in _pluginManager.Servers.ToArray())
        {
            _pluginManager.RemoveServer(server);
        }

        foreach (var source in sources)
        {
            _pluginManager.AddServer(source.ToPluginServer());
        }

        Refresh();
    }

    private static string AddSourceSnapshot(string snapshot, PluginSourceSnapshot source)
    {
        var sources = Deserialize(snapshot).ToList();
        sources.RemoveAll(x => x.sourceUri == source.sourceUri);
        sources.Add(source);
        return Serialize(sources);
    }

    private static string ReplaceSourceSnapshot(
        string snapshot,
        string oldSourceUri,
        PluginSourceSnapshot source
    )
    {
        var sources = Deserialize(snapshot).ToList();
        var index = sources.FindIndex(x => x.sourceUri == oldSourceUri);
        if (index < 0)
        {
            sources.Add(source);
        }
        else
        {
            sources[index] = source;
        }

        return Serialize(sources);
    }

    private static string RemoveSourceSnapshot(string snapshot, string sourceUri)
    {
        var sources = Deserialize(snapshot).ToList();
        sources.RemoveAll(x => x.sourceUri == sourceUri);
        return Serialize(sources);
    }

    private static PluginSourceSnapshot ToSnapshot(IPluginServerInfo source)
    {
        return new PluginSourceSnapshot(source.Name, source.SourceUri, source.Username, null);
    }

    private static PluginSourceSnapshot ToSnapshot(PluginServer source)
    {
        return new PluginSourceSnapshot(
            source.Name,
            source.SourceUri,
            source.Username,
            source.Password
        );
    }

    private static string Serialize(IEnumerable<PluginSourceSnapshot> sources)
    {
        return JsonSerializer.Serialize(sources);
    }

    private static PluginSourceSnapshot[] Deserialize(string snapshot)
    {
        return JsonSerializer.Deserialize<PluginSourceSnapshot[]>(snapshot) ?? [];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SelectedItem.Value = null;
        }

        base.Dispose(disposing);
    }

    private sealed record PluginSourceSnapshot(
        string name,
        string sourceUri,
        string? username,
        string? password
    )
    {
        public PluginServer ToPluginServer()
        {
            return new PluginServer(name, sourceUri, username, password);
        }
    }
}
