using System.ComponentModel;
using System.Diagnostics;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

public class PortViewModel : RoutableViewModel, IPortViewModel
{
    private readonly IUnitService _unitService;
    private readonly List<INotifyDataErrorInfo> _validateProperties = new();
    private readonly BindableReactiveProperty<bool> _hasChanges;
    private readonly BindableReactiveProperty<bool> _hasValidationError;
    private readonly ObservableList<IProtocolEndpoint> _endpoints = [];
    private readonly IncrementalRateCounter _rxBytes;
    private readonly IncrementalRateCounter _txBytes;
    private readonly IncrementalRateCounter _rxPackets;
    private readonly IncrementalRateCounter _txPackets;
    private readonly IUnit _frequencyUnit;

    private const int MaxPortNameLength = 50;

    public PortViewModel()
        : this(DesignTime.Id, DesignTime.UnitService, DesignTime.LoggerFactory, TimeProvider.System)
    {
        DesignTime.ThrowIfNotDesignMode();
        InitArgs(Guid.NewGuid().ToString());
        Icon = MaterialIconKind.Connection;
        var index = 0;
        Observable
            .Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                index++;
                Status = (index % 4) switch
                {
                    0 => ProtocolPortStatus.Disconnected,
                    1 => ProtocolPortStatus.InProgress,
                    2 => ProtocolPortStatus.Error,
                    _ => ProtocolPortStatus.Connected,
                };
            })
            .DisposeItWith(Disposable);
        TagsSource.Add(
            new TagViewModel("ip", DesignTime.LoggerFactory) { Key = "ip", Value = "127.0.0.1" }
        );
        TagsSource.Add(
            new TagViewModel("port", DesignTime.LoggerFactory) { Key = "port", Value = "7341" }
        );
        TagsSource.Add(
            new TagViewModel("rx", DesignTime.LoggerFactory)
            {
                Icon = MaterialIconKind.ArrowDownBold,
                Value = "12kb",
            }
        );
        TagsSource.Add(
            new TagViewModel("tx", DesignTime.LoggerFactory)
            {
                Icon = MaterialIconKind.ArrowUpBold,
                Value = "38kb",
            }
        );

        var source = new ObservableList<EndpointViewModel>
        {
            new EndpointViewModel(),
            new EndpointViewModel(),
            new EndpointViewModel(),
        };
        EndpointsView = source.ToNotifyCollectionChangedSlim();
    }

    public PortViewModel(
        NavigationId id,
        IUnitService unitService,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : base(id, loggerFactory)
    {
        LoggerFactory = loggerFactory;
        TimeProvider = timeProvider;
        _unitService = unitService;
        _frequencyUnit =
            unitService.Units[FrequencyBase.Id]
            ?? throw new UnitException($"Unit {FrequencyBase.Id} was not found");
        _rxBytes = new IncrementalRateCounter(5, timeProvider);
        _txBytes = new IncrementalRateCounter(5, timeProvider);
        _rxPackets = new IncrementalRateCounter(5, timeProvider);
        _txPackets = new IncrementalRateCounter(5, timeProvider);

        var view = _endpoints.CreateView(EndpointFactory).DisposeItWith(Disposable);
        view.DisposeMany().DisposeItWith(Disposable);
        view.SetRoutableParent(this).DisposeItWith(Disposable);
        EndpointsView = view.ToNotifyCollectionChanged(
                SynchronizationContextCollectionEventDispatcher.Current
            )
            .DisposeItWith(Disposable);

        TagsView = TagsSource.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        _hasValidationError = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        _hasChanges = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        SaveChangesCommand = new ReactiveCommand(_ =>
            Task.Factory.StartNew(SaveChanges, null, TaskCreationOptions.LongRunning)
        ).DisposeItWith(Disposable);
        _hasValidationError
            .Subscribe(x => SaveChangesCommand.ChangeCanExecute(!x))
            .DisposeItWith(Disposable);
        CancelChangesCommand = new ReactiveCommand(CancelChanges).DisposeItWith(Disposable);
        IsEnabled = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        IsEnabled
            .SubscribeAwait(ChangeEnabled, AwaitOperation.Drop, false)
            .DisposeItWith(Disposable);
        AddToValidation(Name = new BindableReactiveProperty<string>(), ValidateName);

        RemovePortCommand = new ReactiveCommand(RemovePort).DisposeItWith(Disposable);

        TagsSource.Add(
            TypeTag = new TagViewModel("type", loggerFactory)
            {
                Color = AsvColorKind.Info1,
                Key = null,
            }
        );

        TagsSource.Add(
            ConfigTag = new TagViewModel("cfg", loggerFactory)
            {
                Icon = null,
                Color = AsvColorKind.Success,
                Value = DataFormatter.ByteRate.Print(double.NaN),
            }
        );
        TagsSource.Add(
            RxTag = new TagViewModel("rx", loggerFactory)
            {
                Icon = MaterialIconKind.ArrowDownBold,
                Color = AsvColorKind.Success,
                Value = DataFormatter.ByteRate.Print(double.NaN),
            }
        );
        TagsSource.Add(
            TxTag = new TagViewModel("tx", loggerFactory)
            {
                Icon = MaterialIconKind.ArrowUpBold,
                Color = AsvColorKind.Success,
                Value = DataFormatter.ByteRate.Print(double.NaN),
            }
        );
    }

    public IReadOnlyBindableReactiveProperty<bool> HasValidationError => _hasValidationError;

    public IReadOnlyBindableReactiveProperty<bool> HasChanges => _hasChanges;

    public BindableReactiveProperty<string> Name { get; }

    public string? ConnectionString
    {
        get;
        set => SetField(ref field, value);
    }

    public NotifyCollectionChangedSynchronizedViewList<TagViewModel> TagsView { get; }

    public ProtocolPortStatus Status
    {
        get;
        set => SetField(ref field, value);
    }

    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public BindableReactiveProperty<bool> IsEnabled { get; }

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    public NotifyCollectionChangedSynchronizedViewList<EndpointViewModel> EndpointsView { get; }

    #region Default tags

    public TagViewModel TypeTag { get; }
    public TagViewModel ConfigTag { get; }
    public TagViewModel RxTag { get; }
    public TagViewModel TxTag { get; }

    #endregion

    public string? StatusMessage
    {
        get;
        set => SetField(ref field, value);
    }

    public ReactiveCommand CancelChangesCommand { get; }
    public ReactiveCommand SaveChangesCommand { get; }
    public ReactiveCommand RemovePortCommand { get; }
    public IProtocolPort? Port { get; private set; }

    protected ILoggerFactory LoggerFactory { get; }
    protected TimeProvider TimeProvider { get; }
    protected ObservableList<TagViewModel> TagsSource { get; } = [];

    public virtual void Init(IProtocolPort protocolPort)
    {
        Port = protocolPort;
        Port.IsEnabled.Subscribe(IsEnabled.AsObserver()).DisposeItWith(Disposable);
        Port.Status.Subscribe(UpdatePortStatus).DisposeItWith(Disposable);

        Port.Error.Subscribe(x => StatusMessage = x?.Message).DisposeItWith(Disposable);
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(UpdateStatistic)
            .DisposeItWith(Disposable);
        InitArgs(protocolPort.Id);
        InternalLoadChanges(protocolPort.Config);
        ResetChanges();

        Port.EndpointAdded.Subscribe(x => _endpoints.Add(x)).DisposeItWith(Disposable);
        Port.EndpointRemoved.Subscribe(x => _endpoints.Remove(x)).DisposeItWith(Disposable);
        _endpoints.AddRange(_endpoints);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected virtual EndpointViewModel EndpointFactory(IProtocolEndpoint arg)
    {
        return new EndpointViewModel(arg, _unitService, LoggerFactory, TimeProvider);
    }

    protected virtual void InternalSaveChanges(ProtocolPortConfig config)
    {
        ConnectionString = config.AsUri().ToString();
        config.IsEnabled = IsEnabled.Value;
        config.Name = Name.Value;
    }

    protected virtual void InternalLoadChanges(ProtocolPortConfig config)
    {
        IsEnabled.Value = config.IsEnabled;
        if (config.Name != null)
        {
            Name.Value = config.Name;
        }
        ConnectionString = config.AsUri().ToString();
    }

    protected void AddToValidation<T>(
        BindableReactiveProperty<T> validateProperty,
        Func<T, Exception?> validator
    )
    {
        _validateProperties.Add(validateProperty);
        validateProperty.EnableValidation(validator);
        validateProperty.DisposeItWith(Disposable);
        Observable
            .FromEventHandler<DataErrorsChangedEventArgs>(
                h => validateProperty.ErrorsChanged += h,
                h => validateProperty.ErrorsChanged -= h
            )
            .Subscribe(UpdateValidationStatus)
            .DisposeItWith(Disposable);
        validateProperty.Subscribe(_ => _hasChanges.Value = true).DisposeItWith(Disposable);
    }

    private void UpdatePortStatus(ProtocolPortStatus status)
    {
        Status = status;
        StatusMessage = Status switch
        {
            ProtocolPortStatus.Connected => RS.PortViewModel_ProtocolPortStatus_Connected,
            ProtocolPortStatus.Disconnected => RS.PortViewModel_ProtocolPortStatus_Disconected,
            ProtocolPortStatus.InProgress => RS.PortViewModel_ProtocolPortStatus_InProgress,
            ProtocolPortStatus.Error => Port?.Error.CurrentValue?.Message is null
                ? RS.PortViewModel_ProtocolPortStatus_Error
                : $"{RS.PortViewModel_ProtocolPortStatus_Error}: {Port?.Error.CurrentValue?.Message}",
            _ => null,
        };
    }

    private void UpdateStatistic(Unit unit)
    {
        var rxBytes = DataFormatter.ByteRate.Print(
            _rxBytes.Calculate(Port?.Statistic.RxBytes ?? 0)
        );
        var txBytes = DataFormatter.ByteRate.Print(
            _txBytes.Calculate(Port?.Statistic.TxBytes ?? 0)
        );
        var rxPackets = _rxPackets.Calculate(Port?.Statistic.RxMessages ?? 0).ToString("F1");
        var txPackets = _txPackets.Calculate(Port?.Statistic.TxMessages ?? 0).ToString("F1");
        var gzUnitSymbol = _frequencyUnit.AvailableUnits[HertzFrequencyUnit.Id].Symbol;
        RxTag.Value = $"{rxBytes} / {rxPackets} {gzUnitSymbol}";
        TxTag.Value = $"{txBytes} / {txPackets} {gzUnitSymbol}";
        EndpointsView.ForEach(x => x.UpdateStatistic());
    }

    private void UpdateValidationStatus((object? sender, DataErrorsChangedEventArgs e) valueTuple)
    {
        _hasValidationError.Value = _validateProperties.Any(x => x.HasErrors);
    }

    private void ResetChanges()
    {
        _hasChanges.Value = false;
    }

    private Exception? ValidateName(string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            return new Exception(RS.PortViewModel_ValidationException_IsNullOrWhiteSpace);
        }

        if (arg.Length > MaxPortNameLength)
        {
            return new Exception(
                string.Format(RS.PortViewModel_ValidationException_TooLong, MaxPortNameLength)
            );
        }

        return null;
    }

    private ValueTask ChangeEnabled(bool isEnabled, CancellationToken cancel)
    {
        if (isEnabled)
        {
            Port?.Enable();
            StatusMessage = null;
        }
        else
        {
            Port?.Disable();
            StatusMessage = RS.PortViewModel_StatusMessage_Disabled;
        }

        return ValueTask.CompletedTask;
    }

    private ValueTask CancelChanges(Unit arg1, CancellationToken arg2)
    {
        if (Port is null)
        {
            return ValueTask.CompletedTask;
        }

        InternalLoadChanges(Port.Config);
        ResetChanges();
        return ValueTask.CompletedTask;
    }

    private async void SaveChanges(object? state)
    {
        try
        {
            Debug.Assert(Port != null, "Port should not be null when saving changes");
            var cfg = (ProtocolPortConfig)Port.Config.Clone();
            InternalSaveChanges(cfg);
            await PortCrudCommand.ExecuteChange(this, Port.Id, cfg);
        }
        catch (Exception e)
        {
            // TODO handle exception
        }
    }

    private ValueTask RemovePort(Unit arg1, CancellationToken arg2)
    {
        Debug.Assert(Port != null, "Port should not be null when removing port");
        return PortCrudCommand.ExecuteRemove(this, Port.Id);
    }

    public IExportInfo Source => IoModule.Instance;
}
