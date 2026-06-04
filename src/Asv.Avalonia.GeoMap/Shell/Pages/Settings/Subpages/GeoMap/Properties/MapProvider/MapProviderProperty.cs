using Asv.Avalonia;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapProviderProperty : PropertyComboBoxViewModel
{
    public const string ViewModelId = "map-provider";

    private readonly IMapService _mapService;
    private readonly EditApiKeyDialogPrefab _editApiKeyDialog;
    private readonly MenuItem _editApiKeyMenuItem;

    public MapProviderProperty()
        : this(NullMapService.Instance, DesignTime.DialogService, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MapProviderProperty(
        IMapService mapService,
        IDialogService dialogService,
        ILoggerFactory loggerFactory
    )
        : base(ViewModelId)
    {
        _mapService = mapService;
        _editApiKeyDialog = dialogService.GetDialogPrefab<EditApiKeyDialogPrefab>();

        Header = RS.SettingsGeoMapView_TileProviderProperty_Title;
        Description = RS.SettingsGeoMapView_TileProviderProperty_Description;
        Icon = MaterialIconKind.MapOutline;
        IconColor = AsvColorKind.Info7;

        foreach (var provider in mapService.AvailableProviders.OrderBy(p => p.Info.Group.Id))
        {
            ItemsSource.Add(new TileProviderViewModel(provider));
        }

        CurrentProvider = new BindableReactiveProperty<TileProviderViewModel?>().DisposeItWith(
            Disposable
        );
        EditApiKeyCommand = new ReactiveCommand(EditApiKeyAsync).DisposeItWith(Disposable);
        IsEditApiKeyEnabled = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);

        _editApiKeyMenuItem = new MenuItem(
            "edit_provider_api_key",
            RS.SettingsGeoMapView_EditProviderApiKey_Tooltip
        )
        {
            Icon = MaterialIconKind.Pencil,
            Command = EditApiKeyCommand,
        };
        Menu.Add(_editApiKeyMenuItem);

        _mapService
            .CurrentProvider.ObserveOnUIThreadDispatcher()
            .Subscribe(provider =>
            {
                SetCurrentProvider(FindProvider(provider));
            })
            .DisposeItWith(Disposable);

        SetCurrentProvider(FindProvider(mapService.CurrentProvider.Value));
    }

    public BindableReactiveProperty<TileProviderViewModel?> CurrentProvider { get; }
    public BindableReactiveProperty<bool> IsEditApiKeyEnabled { get; }
    public ReactiveCommand EditApiKeyCommand { get; }

    protected override ValueTask ApplyFromUser(IHeadlinedViewModel item, CancellationToken cancel)
    {
        if (item is TileProviderViewModel provider)
        {
            _mapService.CurrentProvider.Value = provider.Provider;
        }

        return ValueTask.CompletedTask;
    }

    private TileProviderViewModel? FindProvider(ITileProvider provider)
    {
        return ItemsSource
            .OfType<TileProviderViewModel>()
            .FirstOrDefault(vm => vm.Provider == provider);
    }

    private void SetCurrentProvider(TileProviderViewModel? provider)
    {
        if (CurrentProvider.Value is not null)
        {
            CurrentProvider.Value.IsCurrent.Value = false;
        }

        CurrentProvider.Value = provider;

        if (CurrentProvider.Value is not null)
        {
            CurrentProvider.Value.IsCurrent.Value = true;
        }

        ApplyValueFromModel(provider);
        UpdateApiKeyActionState();
    }

    private void UpdateApiKeyActionState()
    {
        var isEnabled = CurrentProvider.Value?.Provider is IProtectedTileProvider;
        IsEditApiKeyEnabled.Value = isEnabled;
        _editApiKeyMenuItem.IsEnabled = isEnabled;
    }

    private async ValueTask EditApiKeyAsync(Unit unit, CancellationToken cancel)
    {
        if (CurrentProvider.Value is not { Provider: IProtectedTileProvider })
        {
            return;
        }

        var currentKey = _mapService.GetProviderApiKey(CurrentProvider.Value.Provider.Info.Id);
        var result = await _editApiKeyDialog.ShowDialogAsync(
            new EditApiKeyDialogPayload { CurrentApiKey = currentKey }
        );

        if (result != currentKey)
        {
            _mapService.SetProviderApiKey(CurrentProvider.Value.Provider.Info.Id, result);
        }
    }
}
