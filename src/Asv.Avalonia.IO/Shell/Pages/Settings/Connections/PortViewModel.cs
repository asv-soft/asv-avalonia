using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

public class PortViewModel : RoutableViewModel, IPortViewModel
{
    private readonly List<INotifyDataErrorInfo> _validateProperties = new();
    private readonly BindableReactiveProperty<bool> _hasChanges;
    private readonly BindableReactiveProperty<bool> _hasValidationError;
    private readonly ObservableList<IProtocolEndpoint> _endpoints = [];
    private readonly IncrementalRateCounter _rxBytes;
    private readonly IncrementalRateCounter _txBytes;
    private readonly IncrementalRateCounter _rxPackets;
    private readonly IncrementalRateCounter _txPackets;

    public PortViewModel()
        : this(DesignTime.Id, DesignTime.LoggerFactory, TimeProvider.System)
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

    public PortViewModel(NavigationId id, ILoggerFactory loggerFactory, TimeProvider timeProvider)
        : base(id, loggerFactory)
    {
        LoggerFactory = loggerFactory;
        TimeProvider = timeProvider;
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
            TypeTag = new TagViewModel("type", loggerFactory) { TagType = TagType.Info, Key = null }
        );

        TagsSource.Add(
            ConfigTag = new TagViewModel("cfg", loggerFactory)
            {
                Icon = null,
                TagType = TagType.Success,
                Value = DataFormatter.ByteRate.Print(double.NaN),
            }
        );
        TagsSource.Add(
            RxTag = new TagViewModel("rx", loggerFactory)
            {
                Icon = MaterialIconKind.ArrowDownBold,
                TagType = TagType.Success,
                Value = DataFormatter.ByteRate.Print(double.NaN),
            }
        );
        TagsSource.Add(
            TxTag = new TagViewModel("tx", loggerFactory)
            {
                Icon = MaterialIconKind.ArrowUpBold,
                TagType = TagType.Success,
                Value = DataFormatter.ByteRate.Print(double.NaN),
            }
        );
    }

    protected ILoggerFactory LoggerFactory { get; }
    protected TimeProvider TimeProvider { get; }

    protected virtual EndpointViewModel EndpointFactory(IProtocolEndpoint arg)
    {
        return new EndpointViewModel(arg, LoggerFactory, TimeProvider);
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
            StatusMessage = "Disabled";
        }

        return ValueTask.CompletedTask;
    }

    public ReactiveCommand CancelChangesCommand { get; }

    private ValueTask CancelChanges(Unit arg1, CancellationToken arg2)
    {
        if (Port == null)
        {
            return ValueTask.CompletedTask;
        }

        InternalLoadChanges(Port.Config);
        ResetChanges();
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand SaveChangesCommand { get; }

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

    public ReactiveCommand RemovePortCommand { get; }

    private ValueTask RemovePort(Unit arg1, CancellationToken arg2)
    {
        Debug.Assert(Port != null, "Port should not be null when removing port");
        return PortCrudCommand.ExecuteRemove(this, Port.Id);
    }

    public IProtocolPort? Port { get; private set; }

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

    private void UpdatePortStatus(ProtocolPortStatus status)
    {
        Status = status;
        StatusMessage = Status switch
        {
            ProtocolPortStatus.Connected => "Connected",
            ProtocolPortStatus.Disconnected => "Disconnected",
            ProtocolPortStatus.InProgress => "Connecting...",
            ProtocolPortStatus.Error => Port?.Error.CurrentValue?.Message ?? "Error",
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
        RxTag.Value = $"{rxBytes} / {rxPackets} Hz";
        TxTag.Value = $"{txBytes} / {txPackets} Hz";
        EndpointsView.ForEach(x => x.UpdateStatistic());
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
        validateProperty.Subscribe(x => _hasChanges.Value = true).DisposeItWith(Disposable);
    }

    private void UpdateValidationStatus((object? sender, DataErrorsChangedEventArgs e) valueTuple)
    {
        _hasValidationError.Value = _validateProperties.Any(x => x.HasErrors);
    }

    public IReadOnlyBindableReactiveProperty<bool> HasValidationError => _hasValidationError;

    public IReadOnlyBindableReactiveProperty<bool> HasChanges => _hasChanges;

    private void ResetChanges()
    {
        _hasChanges.Value = false;
    }

    private Exception? ValidateName(string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            return new Exception("Port name is required");
        }

        if (arg.Length > 50)
        {
            return new Exception("Port name is too long. Max length is 50 characters");
        }

        return null;
    }

    public BindableReactiveProperty<string> Name { get; }

    public string? ConnectionString
    {
        get;
        set => SetField(ref field, value);
    }

    protected ObservableList<TagViewModel> TagsSource { get; } = [];

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

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    public IExportInfo Source => IoModule.Instance;
}
