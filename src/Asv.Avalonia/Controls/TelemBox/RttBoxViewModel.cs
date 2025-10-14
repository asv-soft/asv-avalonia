using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public enum RttBoxStatus
{
    Normal,
    Unknown,
    Success,
    Warning,
    Error,
    Info1,
    Info2,
    Info3,
    Info4,
}

public class RttBoxViewModel : RoutableViewModel
{
    private readonly TimeProvider _timeProvider;
    private long _lastUpdate;

    public RttBoxViewModel()
        : base(DesignTime.Id, NullLayoutService.Instance, DesignTime.LoggerFactory)
    {
        _timeProvider = TimeProvider.System;
        DesignTime.ThrowIfNotDesignMode();
    }

    public RttBoxViewModel(
        NavigationId id,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, layoutService, loggerFactory)
    {
        _timeProvider = TimeProvider.System;
        if (networkErrorTimeout != null)
        {
            _timeProvider.CreateTimer(
                CheckNetworkTimeout,
                networkErrorTimeout,
                networkErrorTimeout.Value,
                networkErrorTimeout.Value
            );
        }
    }

    private void CheckNetworkTimeout(object? state)
    {
        var timeout = (TimeSpan)state!;
        IsNetworkError = _timeProvider.GetElapsedTime(_lastUpdate) > timeout;
    }

    public bool? IsUpdated
    {
        get;
        private set => SetField(ref field, value);
    }

    public void Updated()
    {
        IsNetworkError = false;
        IsUpdated = false;
        IsUpdated = true;
        _lastUpdate = _timeProvider.GetTimestamp();
    }

    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Header
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ShortHeader
    {
        get;
        set => SetField(ref field, value);
    }

    public RttBoxStatus Status
    {
        get;
        set => SetField(ref field, value);
    }

    public double Progress
    {
        get;
        set => SetField(ref field, value);
    }

    public RttBoxStatus? ProgressStatus
    {
        get;
        set => SetField(ref field, value);
    }

    public bool? IsNetworkError
    {
        get;
        set => SetField(ref field, value);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
