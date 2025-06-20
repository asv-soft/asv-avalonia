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
    private MaterialIconKind? _icon;
    private readonly List<INotifyDataErrorInfo> _validateProperties = new();
    private readonly BindableReactiveProperty<bool> _hasChanges;
    private readonly BindableReactiveProperty<bool> _hasValidationError;
    private readonly ObservableList<TagViewModel> _tagsSource = [];

    public PortViewModel()
        : this(DesignTime.Id, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        InitArgs(Guid.NewGuid().ToString());
        Icon = MaterialIconKind.Connection;
        TagsSource.Add(
            new TagViewModel("ip", DesignTime.LoggerFactory) { Key = "ip", Value = "127.0.0.1" }
        );
        TagsSource.Add(
            new TagViewModel("port", DesignTime.LoggerFactory) { Key = "port", Value = "7341" }
        );
    }

    public PortViewModel(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
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
    }

    private async ValueTask ChangeEnabled(bool isEnabled, CancellationToken cancel)
    {
        if (Port == null)
        {
            return;
        }

        if (Port.IsEnabled.CurrentValue == isEnabled)
        {
            return;
        }

        await Task.Factory.StartNew(
            () =>
            {
                if (isEnabled)
                {
                    Port.Enable();
                }
                else
                {
                    Port.Disable();
                }
            },
            cancel
        );
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
    }

    public virtual void Init(IProtocolPort protocolPort)
    {
        Port = protocolPort;
        Port.IsEnabled.Subscribe(IsEnabled.AsObserver()).DisposeItWith(Disposable);
        InitArgs(protocolPort.Id);
        InternalLoadChanges(protocolPort.Config);
        ResetChanges();
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

    protected void ResetChanges()
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

    protected ObservableList<TagViewModel> TagsSource => _tagsSource;

    public NotifyCollectionChangedSynchronizedViewList<TagViewModel> TagsView { get; }

    private void UpdateStatus(ProtocolPortStatus status)
    {
        switch (status)
        {
            case ProtocolPortStatus.Disconnected:

                break;
            case ProtocolPortStatus.InProgress:
                break;
            case ProtocolPortStatus.Connected:
                break;
            case ProtocolPortStatus.Error:
                IsError = true;
                IsSuccess = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    public bool IsError { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsInProgress { get; set; }

    public MaterialIconKind? Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    public BindableReactiveProperty<bool> IsEnabled { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public IExportInfo Source => IoModule.Instance;
}
