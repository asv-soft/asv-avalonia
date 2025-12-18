using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class RttBoxViewModel : RoutableViewModel
{
    private readonly TimeProvider _timeProvider;
    private long _lastUpdate;

    public RttBoxViewModel()
        : base(DesignTime.Id, DesignTime.LoggerFactory)
    {
        _timeProvider = TimeProvider.System;
        DesignTime.ThrowIfNotDesignMode();
    }

    public RttBoxViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory)
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

    public AsvColorKind Status
    {
        get;
        set => SetField(ref field, value);
    }

    public double Progress
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind? ProgressStatus
    {
        get;
        set => SetField(ref field, value);
    }

    public bool? IsNetworkError
    {
        get;
        set => SetField(ref field, value);
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }
}
