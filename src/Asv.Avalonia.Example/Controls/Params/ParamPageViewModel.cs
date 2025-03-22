using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Cfg;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

public class ParamsConfig
{
    public List<ParamItemViewModelConfig> Params { get; set; } = [];
}

public class ParamPageViewModel : PageViewModel<ParamPageViewModel>
{
    public const string PageIdUnique = "params";

    private readonly IMavlinkConnectionService _svc;
    private readonly ILogger _log;
    private readonly INavigationService _nav;
    private readonly IConfiguration _cfg;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly ISynchronizedView<ParamItemViewModel, ParamItemViewModel> _view;
    private readonly ObservableList<ParamItemViewModel> _paramsList;
    private ParamItemViewModel _selectedItem;
    private readonly Subject<bool> _canClearSearchText = new();
    private readonly ParamsConfig _config;
    private IParamsClientEx _paramsIfc;

    public ParamPageViewModel()
        : this(
            Guid.NewGuid().ToString(),
            null!,
            NullNavigationService.Instance,
            NullCommandService.Instance,
            NullLoggerFactory.Instance,
            new InMemoryConfiguration()
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        DeviceName.Value = "Params";
    }

    protected ParamPageViewModel(
        string deviceId,
        IMavlinkConnectionService svc,
        INavigationService nav,
        ICommandService cmd,
        ILoggerFactory log,
        IConfiguration cfg
    )
        : base(new NavigationId(PageIdUnique, deviceId), cmd)
    {
        ArgumentNullException.ThrowIfNull(svc);
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(cfg);
        ArgumentNullException.ThrowIfNull(log);
        ArgumentNullException.ThrowIfNull(nav);

        _svc = svc;
        _log = log.CreateLogger<ParamPageViewModel>();
        _cfg = cfg;
        _config = _cfg.Get<ParamsConfig>();
        _nav = nav;

        _cancellationTokenSource = new CancellationTokenSource();

        Icon.Value = MaterialIconKind.CardsDiamond; // TODO: сделать выбор иконки в зависимости от типа устройства
        SearchText = new BindableReactiveProperty<string>();
        ShowStaredOnly = new BindableReactiveProperty<bool>();

        _paramsList = new ObservableList<ParamItemViewModel>();
        _view = _paramsList.CreateView(vm => vm);
        ViewedParams = _paramsList.ToNotifyCollectionChanged();

        SearchText
            .ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                if (x.IsNullOrWhiteSpace())
                {
                    _view.ResetFilter();
                }
                else
                {
                    _view.AttachFilter(
                        new SynchronizedViewFilter<ParamItemViewModel, ParamItemViewModel>(
                            (_, model) => model.Filter(x, ShowStaredOnly.Value)
                        )
                    );
                }
            });

        SearchText.Subscribe(txt => _canClearSearchText.OnNext(!string.IsNullOrWhiteSpace(txt)));

        Clear = _canClearSearchText.ToReactiveCommand(_ => SearchText.Value = string.Empty);

        Disposable.AddAction(() =>
        {
            _config.Params = _config.Params.Where(_ => _.IsStarred).ToList();
            _cfg.Set(_config);
        });
    }

    // public override void SetArgs(NameValueCollection args)
    // {
    //     base.SetArgs(args);
    //     if (ushort.TryParse(args["id"], out var id) == false) return;
    //     if (Enum.TryParse<DeviceClass>(args["class"], true, out var deviceClass) == false) return;
    //     var ifc = GetParamsClient(_svc, id, deviceClass);
    //     if (ifc == null) return;
    //
    //     Icon.Value = MavlinkHelper.GetIcon(deviceClass);
    //
    //     switch (deviceClass)
    //     {
    //         case DeviceClass.Plane:
    //             var plane = _svc.GetVehicleByFullId(id);
    //             if (plane == null) break;
    //             DeviceName = plane.Name.Value;
    //             break;
    //         case DeviceClass.Copter:
    //             var copter = _svc.GetVehicleByFullId(id);
    //             if (copter == null) break;
    //             DeviceName = copter.Name.Value;
    //             break;
    //         case DeviceClass.GbsRtk:
    //             var gbs = _svc.GetGbsByFullId(id);
    //             if (gbs == null) break;
    //             DeviceName = gbs.Name.Value;
    //             break;
    //         case DeviceClass.SdrPayload:
    //             var sdr = _svc.GetPayloadsByFullId(id);
    //             if (sdr == null) break;
    //             DeviceName = sdr.Name.Value;
    //             break;
    //         case DeviceClass.Adsb:
    //             var adsb = _svc.GetAdsbVehicleByFullId(id);
    //             if (adsb == null) break;
    //             DeviceName = adsb.Name.Value;
    //             break;
    //     }
    //
    //     InternalInit(ifc);
    // }
    public virtual IParamsClientEx? GetParamsClient(
        IMavlinkDevicesService svc,
        ushort fullId,
        DeviceClass @class
    )
    {
        return null;
    }

    protected void InternalInit(IParamsClientEx paramsIfc)
    {
        @paramsIfc
            .RemoteCount.Where(_ => _.HasValue)
            .Subscribe(_ => Total = _.Value)
            .DisposeItWith(Disposable);
        var inputPipe = @paramsIfc
            .Items.Transform(_ => new ParamItemViewModel(Id, _, _log))
            .DisposeMany()
            .RefCount();
        inputPipe
            .Bind(out var allItems)
            .Subscribe(_ =>
            {
                foreach (var item in _config.Params)
                {
                    var existItem = _.FirstOrDefault(_ => _.Current.Name == item.Name);
                    if (existItem == null)
                        continue;
                    existItem.Current?.SetConfig(item);
                }
            })
            .DisposeItWith(Disposable);
        inputPipe
            .Filter(FilterPipe)
            .SortBy(_ => _.Name)
            .AutoRefresh(v => v.IsSynced)
            .Bind(out var leftItems)
            .Subscribe()
            .DisposeItWith(Disposable);
        inputPipe
            .AutoRefresh(_ => _.IsStarred)
            .Filter(_ => _.IsStarred)
            .Subscribe(_ =>
            {
                foreach (var item in _)
                {
                    var existItem = _config.Params.FirstOrDefault(__ =>
                        __.Name == item.Current.Name
                    );

                    if (existItem != null)
                        _config.Params.Remove(existItem);

                    _config.Params.Add(
                        new ParamItemViewModelConfig
                        {
                            IsStarred = item.Current.IsStarred,
                            Name = item.Current.Name,
                        }
                    );
                }
            })
            .DisposeItWith(Disposable);

        Params = leftItems;

        UpdateParams = ReactiveCommand
            .CreateFromTask(async cancel =>
            {
                _cancellationTokenSource = new CancellationTokenSource().DisposeItWith(Disposable);
                var viewed = _paramsList.Items.Select(_ => _.GetConfig()).ToArray();
                _paramsList.Clear();
                try
                {
                    await paramsIfc.ReadAll(
                        new Progress<double>(_ => Progress = _),
                        _cancellationTokenSource.Token
                    );
                }
                catch (TaskCanceledException)
                {
                    _log.Info("User", "Canceled updating params");
                }
                finally
                {
                    foreach (var item in viewed)
                    {
                        var existItem = allItems.FirstOrDefault(_ => _.Name == item.Name);
                        if (existItem == null)
                            continue;
                        existItem.SetConfig(item);
                        _paramsList.Add(existItem);
                    }
                }
            })
            .DisposeItWith(Disposable);
        UpdateParams
            .IsExecuting.ToProperty(this, _ => _.IsRefreshing, out IsRefreshing)
            .DisposeItWith(Disposable);
        UpdateParams.ThrownExceptions.Subscribe(OnRefreshError).DisposeItWith(Disposable);
        StopUpdateParams = ReactiveCommand.Create(() =>
        {
            _cancellationTokenSource.Cancel();
        });
        RemoveAllPins = ReactiveCommand
            .Create(() =>
            {
                _paramsList.Edit(_ =>
                {
                    foreach (var item in _)
                    {
                        item.IsPinned = false;
                    }
                });
            })
            .DisposeItWith(Disposable);
    }

    public override async Task<bool> TryClose()
    {
        var notSyncedParams = _paramsList.Items.Where(_ => !_.IsSynced).ToArray();

        if (notSyncedParams.Any())
        {
            var dialog = new ContentDialog()
            {
                Title = RS.ParamPageViewModel_DataLossDialog_Title,
                Content = RS.ParamPageViewModel_DataLossDialog_Content,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = RS.ParamPageViewModel_DataLossDialog_PrimaryButtonText,
                SecondaryButtonText = RS.ParamPageViewModel_DataLossDialog_SecondaryButtonText,
                CloseButtonText = RS.ParamPageViewModel_DataLossDialog_CloseButtonText,
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                foreach (var param in notSyncedParams)
                {
                    param.WriteParamData();
                    param.IsSynced = true;
                }

                return true;
            }

            if (result == ContentDialogResult.Secondary)
            {
                return true;
            }

            if (result == ContentDialogResult.None)
            {
                return false;
            }
        }

        return true;
    }

    private void OnRefreshError(Exception? ex)
    {
        _log.Error("Params view", "Error to read all params items", ex);
    }

    public BindableReactiveProperty<bool> IsRefreshing { get; }
    public BindableReactiveProperty<bool> ShowStaredOnly { get; }
    public BindableReactiveProperty<double> Progress { get; }
    public ReactiveCommand Clear { get; }
    public ReactiveCommand UpdateParams { get; }
    public ReactiveCommand StopUpdateParams { get; }
    public ReactiveCommand RemoveAllPins { get; }
    public IReadOnlyObservableList<ParamItemViewModel> Params { get; }
    public INotifyCollectionChangedSynchronizedViewList<ParamItemViewModel> ViewedParams;
    public BindableReactiveProperty<string> DeviceName { get; }
    public BindableReactiveProperty<string> SearchText { get; }
    public BindableReactiveProperty<int> Total { get; }
    public BindableReactiveProperty<int> Loaded { get; }

    public ParamItemViewModel SelectedItem
    {
        get => _selectedItem;
        set
        {
            var itemsToDelete = _paramsList.Items.Where(_ => _.IsPinned == false).ToArray();
            _paramsList.RemoveMany(itemsToDelete);
            this.RaiseAndSetIfChanged(ref _selectedItem, value);
            if (value != null)
            {
                if (_paramsList.Items.Contains(value) == false)
                {
                    _paramsList.Add(value);
                }
            }
        }
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        throw new NotImplementedException();
    }

    protected override void AfterLoadExtensions()
    {
        // ignore
    }

    public override IExportInfo Source => SystemModule.Instance;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _paramsList.DisposeRemovedItems().Dispose();
            SearchText.Dispose();
        }

        base.Dispose(disposing);
    }
}
