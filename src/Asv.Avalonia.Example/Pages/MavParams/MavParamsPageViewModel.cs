using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.IO;
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

internal class ParamsKvpComparer : IComparer<KeyValuePair<string, ParamItem>>
{
    public static ParamsKvpComparer Instance { get; } = new();

    public int Compare(KeyValuePair<string, ParamItem> x, KeyValuePair<string, ParamItem> y)
    {
        return string.CompareOrdinal(x.Value.Name, y.Value.Name);
    }
}

public class ParamsConfig
{
    public List<ParamItemViewModelConfig> Params { get; set; } = [];
}

public interface IMavParamsPageViewModel : IDevicePage { }

[ExportPage(PageId)]
public class MavParamsPageViewModel
    : DevicePageViewModel<IMavParamsPageViewModel>,
        IMavParamsPageViewModel
{
    public const string PageId = "mav-params";
    public const MaterialIconKind PageIcon = MaterialIconKind.CogTransferOutline;

    private DeviceId _deviceId;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _log;
    private readonly INavigationService _nav;
    private readonly IConfiguration _cfg;
    private CancellationTokenSource _cancellationTokenSource;
    private ISynchronizedView<KeyValuePair<string, ParamItem>, ParamItemViewModel> _view;
    private readonly ObservableList<ParamItemViewModel> _viewedParamsList;
    private readonly ObservableList<KeyValuePair<string, ParamItem>> _params;
    private readonly Subject<bool> _canClearSearchText = new();
    private readonly ParamsConfig _config;

    public MavParamsPageViewModel()
        : this(
            NullDeviceManager.Instance,
            NullCommandService.Instance,
            NullLoggerFactory.Instance,
            new InMemoryConfiguration(),
            NullNavigationService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();

        DeviceName.Value = "Params Device";
        var list = new ObservableList<ParamItemViewModel>()
        {
            new() { DisplayName = "Param 1" },
            new() { DisplayName = "Param 2" },
            new() { DisplayName = "Param 3" },
        };
        var viewedList = new ObservableList<ParamItemViewModel>() { list[0] };

        AllParams = list.ToNotifyCollectionChangedSlim();
        ViewedParams = viewedList.ToNotifyCollectionChangedSlim();
    }

    [ImportingConstructor]
    public MavParamsPageViewModel(
        IDeviceManager devices,
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IConfiguration cfg,
        INavigationService nav
    )
        : base(PageId, devices, cmd)
    {
        ArgumentNullException.ThrowIfNull(devices);
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(cfg);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(nav);

        Title.Value = "Params";
        Icon.Value = PageIcon; // TODO: choose icon from the device type

        _loggerFactory = loggerFactory;
        _log = loggerFactory.CreateLogger<MavParamsPageViewModel>();
        _cfg = cfg;
        _config = _cfg.Get<ParamsConfig>();
        _nav = nav;
        _params = [];
        _viewedParamsList = [];

        ViewedParams = _viewedParamsList.ToNotifyCollectionChangedSlim();

        _cancellationTokenSource = new CancellationTokenSource();

        SearchText = new BindableReactiveProperty<string>();
        ShowStaredOnly = new BindableReactiveProperty<bool>();
        IsRefreshing = new BindableReactiveProperty<bool>();
        DeviceName = new BindableReactiveProperty<string>();
        SelectedItem = new BindableReactiveProperty<ParamItemViewModel?>();
        Progress = new BindableReactiveProperty<double>();

        _sub1 = SelectedItem.Subscribe(value =>
        {
            var itemsToDelete = _viewedParamsList.Where(_ => !_.IsPinned.Value).ToArray();

            foreach (var item in itemsToDelete)
            {
                _viewedParamsList.Remove(item);
            }

            if (value is null)
            {
                return;
            }

            if (!_viewedParamsList.Contains(value))
            {
                _viewedParamsList.Add(value);
            }
        });

        _sub2 = SearchText.Subscribe(txt =>
            _canClearSearchText.OnNext(!string.IsNullOrWhiteSpace(txt))
        );

        Clear = _canClearSearchText.ToReactiveCommand(_ => SearchText.Value = string.Empty);
    }

    private void InternalInit(IParamsClientEx paramsIfc)
    {
        Total = paramsIfc.RemoteCount.ToReadOnlyBindableReactiveProperty();

        _sub3 = paramsIfc.Items.OnAddOrRemove<
            KeyValuePair<string, ParamItem>,
            KeyValuePair<string, ParamItem>
        >(
            addItem =>
            {
                _params.Add(addItem);
                _params.Sort(ParamsKvpComparer.Instance);
            },
            removeItem =>
            {
                _params.Remove(removeItem);
                _params.Sort(ParamsKvpComparer.Instance);
            }
        );
        _sub4 = paramsIfc
            .Items.ObserveReplace()
            .Subscribe(eve =>
            {
                _params.Remove(eve.OldValue);
                _params.Add(eve.NewValue);
                _params.Sort(ParamsKvpComparer.Instance);
            });
        _sub5 = paramsIfc.Items.ObserveClear().Subscribe(_ => _params.Clear());
        _sub6 = paramsIfc.Items.ObserveReverse().Subscribe(_ => _params.Reverse());

        _view = _params.CreateView(kvp => new ParamItemViewModel(
            kvp.Key,
            kvp.Value,
            _loggerFactory,
            _config
        ));
        _sub7 = _view.DisposeRemovedViewItems();

        _sub8 = SearchText
            .ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                if (x.IsNullOrWhiteSpace() && !ShowStaredOnly.Value)
                {
                    _view.ResetFilter();
                }
                else
                {
                    _view.AttachFilter(
                        new SynchronizedViewFilter<
                            KeyValuePair<string, ParamItem>,
                            ParamItemViewModel
                        >((_, model) => model.Filter(x, ShowStaredOnly.Value))
                    );
                }
            });

        _sub9 = ShowStaredOnly
            .ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                if (!x && SearchText.Value.IsNullOrWhiteSpace())
                {
                    _view.ResetFilter();
                }
                else
                {
                    _view.AttachFilter(
                        new SynchronizedViewFilter<
                            KeyValuePair<string, ParamItem>,
                            ParamItemViewModel
                        >((_, model) => model.Filter(SearchText.Value, x))
                    );
                }
            });

        _sub10 = _view
            .ObserveChanged()
            .Subscribe(e =>
            {
                foreach (var item in _config.Params)
                {
                    if (e.NewItem.View.Name == item.Name)
                    {
                        e.NewItem.View.SetConfig(item);
                    }
                }
            });

        AllParams = _view.ToNotifyCollectionChanged();

        UpdateParams = new ReactiveCommand(
            async (_, __) =>
            {
                IsRefreshing.Value = true;
                _cancellationTokenSource = new CancellationTokenSource();
                var viewed = _viewedParamsList.Select(item => item.GetConfig()).ToArray();
                _viewedParamsList.Clear();
                try
                {
                    await paramsIfc.ReadAll(
                        new Progress<double>(i => Progress.Value = i),
                        cancel: _cancellationTokenSource.Token
                    );
                }
                catch (TaskCanceledException)
                {
                    _log.LogInformation("User canceled updating params");
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error to read all params items");
                }
                finally
                {
                    foreach (var item in viewed)
                    {
                        var existItem = _view.FirstOrDefault(currentItem =>
                            currentItem.Name == item.Name
                        );
                        if (existItem is null)
                        {
                            continue;
                        }

                        existItem.SetConfig(item);
                        _viewedParamsList.Add(existItem);
                    }

                    _cancellationTokenSource.Dispose();
                    IsRefreshing.Value = false;
                }
            }
        );

        StopUpdateParams = new ReactiveCommand(_ => _cancellationTokenSource.Cancel());

        RemoveAllPins = new ReactiveCommand(_ =>
        {
            foreach (var item in _viewedParamsList)
            {
                item.IsPinned.Value = false;
            }
        });
    }

    protected override void AfterDeviceInitialized(IClientDevice device) // TODO: Add icon init from the device
    {
        ParamsClient?.Dispose();
        Title.Value = $"Params[{device.Id}]";
        ParamsClient =
            device.GetMicroservice<IParamsClientEx>()
            ?? throw new Exception($"Service of type {nameof(IParamsClientEx)} was not found");
        DeviceName.Value = device.Name.CurrentValue ?? "Unknown";
        _deviceId = device.Id;
        InternalInit(ParamsClient);
    }

    public IParamsClientEx? ParamsClient { get; private set; }
    public BindableReactiveProperty<bool> IsRefreshing { get; }
    public BindableReactiveProperty<bool> ShowStaredOnly { get; }
    public BindableReactiveProperty<double> Progress { get; }
    public ReactiveCommand Clear { get; }
    public ReactiveCommand UpdateParams { get; private set; }
    public ReactiveCommand StopUpdateParams { get; private set; }
    public ReactiveCommand RemoveAllPins { get; private set; }

    public INotifyCollectionChangedSynchronizedViewList<ParamItemViewModel> AllParams
    {
        get;
        private set;
    }

    public INotifyCollectionChangedSynchronizedViewList<ParamItemViewModel> ViewedParams { get; }

    public BindableReactiveProperty<string> DeviceName { get; }
    public BindableReactiveProperty<string> SearchText { get; }
    public IReadOnlyBindableReactiveProperty<int> Total { get; private set; }
    public BindableReactiveProperty<ParamItemViewModel?> SelectedItem { get; }

    protected override async ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is PageCloseAttemptEvent)
        {
            var isCloseReady = await TryCloseWithApproval();
            e.IsHandled = !isCloseReady;
        }

        await base.InternalCatchEvent(e);
    }

    private async Task<bool> TryCloseWithApproval(CancellationToken cancel = default)
    {
        _config.Params = _config.Params.Where(_ => _.IsStarred).ToList();
        _cfg.Set(_config);

        var notSyncedParams = _viewedParamsList.Where(param => !param.IsSynced.Value).ToArray();

        if (notSyncedParams.Length == 0)
        {
            return true;
        }

        using var vm = new TryCloseWithApprovalDialogViewModel();
        var dialog = new ContentDialog(_nav)
        {
            Title = RS.ParamPageViewModel_DataLossDialog_Title,
            Content = vm,
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
                await param.WriteParamData(cancel);
                param.IsSynced.Value = true;
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

        return true;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var item in _view)
        {
            yield return item;
        }

        foreach (var param in _viewedParamsList)
        {
            yield return param;
        }
    }

    protected override void AfterLoadExtensions()
    {
        Id.ChangeArgs(_deviceId.AsString());
    }

    public override IExportInfo Source => SystemModule.Instance;

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private IDisposable _sub3;
    private IDisposable _sub4;
    private IDisposable _sub5;
    private IDisposable _sub6;
    private IDisposable _sub7;
    private IDisposable _sub8;
    private IDisposable _sub9;
    private IDisposable _sub10;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _config.Params = _config.Params.Where(_ => _.IsStarred).ToList();
            _cfg.Set(_config);

            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
            _sub4.Dispose();
            _sub5.Dispose();
            _sub6.Dispose();
            _sub7.Dispose();
            _sub8.Dispose();
            _sub9.Dispose();
            _sub10.Dispose();
            Total.Dispose();
            DeviceName.Dispose();
            ParamsClient?.Dispose();
            _cancellationTokenSource.Dispose();
            SearchText.Dispose();
            IsRefreshing.Dispose();
            ShowStaredOnly.Dispose();
            Progress.Dispose();
            Clear.Dispose();
            UpdateParams.Dispose();
            StopUpdateParams.Dispose();
            RemoveAllPins.Dispose();
            AllParams.Dispose();
            ViewedParams.Dispose();
            Total.Dispose();
            SelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
