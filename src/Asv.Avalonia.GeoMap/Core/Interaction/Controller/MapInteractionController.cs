using System.Diagnostics.CodeAnalysis;
using Asv.Common;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class MapInteractionController
    : DisposableOnce,
        IMapInteractionController,
        IMapInteractionSessionOwner
{
    private readonly BindableReactiveProperty<string?> _status = new();
    private readonly BindableReactiveProperty<AsvColorKind?> _accent = new();
    private readonly Subject<GeoPoint> _clicked = new();
    private readonly Subject<GeoPoint> _cursorMoved = new();
    private MapItemsControl? _map;
    private TopLevel? _topLevel;
    private IMapInteractionSession? _session;

    public bool IsBusy => Dispatcher.UIThread.Invoke(() => _session is not null);

    public IReadOnlyBindableReactiveProperty<string?> Status => _status;

    public IReadOnlyBindableReactiveProperty<AsvColorKind?> Accent => _accent;

    public Observable<GeoPoint> Clicked => _clicked;

    public Observable<GeoPoint> CursorMoved => _cursorMoved;

    public void AttachMap(MapItemsControl map)
    {
        Dispatcher.UIThread.Invoke(() => AttachMapCore(map));
    }

    public void DetachMap()
    {
        Dispatcher.UIThread.Invoke(DetachMapCore);
    }

    public bool TryBegin(
        MapInteractionRequest request,
        [MaybeNullWhen(false)] out IMapInteractionSession session
    )
    {
        var result = Dispatcher.UIThread.Invoke(() =>
        {
            var isStarted = TryBeginCore(request, out var uiSession);
            return (isStarted, uiSession);
        });

        session = result.uiSession;
        return result.isStarted;
    }

    public void SetStatus(IMapInteractionSession session, string? value)
    {
        Dispatcher.UIThread.Invoke(() => SetStatusCore(session, value));
    }

    public void SetAccent(IMapInteractionSession session, AsvColorKind? value)
    {
        Dispatcher.UIThread.Invoke(() => SetAccentCore(session, value));
    }

    public void End(IMapInteractionSession session)
    {
        Dispatcher.UIThread.Invoke(() => EndCore(session));
    }

    private bool TryBeginCore(
        MapInteractionRequest request,
        [MaybeNullWhen(false)] out IMapInteractionSession session
    )
    {
        if (_map is null)
        {
            session = null;
            return false;
        }

        if (_session is not null)
        {
            if (_session.Lifecycle == MapInteractionLifecycle.Lock)
            {
                session = null;
                return false;
            }

            _session.Dispose();
        }

        IMapInteractionSession fresh = new MapInteractionSession(this, request.Lifecycle);
        _session = fresh;
        _status.Value = request.Status;
        _accent.Value = request.Accent;
        session = fresh;
        return true;
    }

    private void AttachMapCore(MapItemsControl map)
    {
        DetachMapCore();

        _map = map;
        map.AddHandler(MapItemsControl.MapClickEvent, OnMapClick);
        map.AddHandler(
            InputElement.PointerMovedEvent,
            OnPointerMoved,
            RoutingStrategies.Bubble,
            handledEventsToo: true
        );

        _topLevel = TopLevel.GetTopLevel(map);
        _topLevel?.AddHandler(
            InputElement.KeyDownEvent,
            OnKeyDown,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
            handledEventsToo: true
        );
    }

    private void DetachMapCore()
    {
        _session?.Dispose();

        if (_map is not null)
        {
            _map.RemoveHandler(MapItemsControl.MapClickEvent, OnMapClick);
            _map.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        }

        _topLevel?.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        _topLevel = null;
        _map = null;
    }

    private void SetStatusCore(IMapInteractionSession session, string? value)
    {
        if (ReferenceEquals(_session, session))
        {
            _status.Value = value;
        }
    }

    private void SetAccentCore(IMapInteractionSession session, AsvColorKind? value)
    {
        if (ReferenceEquals(_session, session))
        {
            _accent.Value = value;
        }
    }

    private void EndCore()
    {
        if (_session is null)
        {
            return;
        }

        _session = null;
        _status.Value = null;
        _accent.Value = null;
    }

    private void EndCore(IMapInteractionSession session)
    {
        if (ReferenceEquals(_session, session))
        {
            EndCore();
        }
    }

    private void OnMapClick(object? sender, MapClickEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        _clicked.OnNext(e.Point);
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_session is null || _map is null)
        {
            return;
        }

        _cursorMoved.OnNext(_map.CursorPosition);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_session is null || e.Key != Key.Escape)
        {
            return;
        }

        _session.Dispose();
        e.Handled = true;
    }

    protected override void InternalDisposeOnce()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Invoke(DetachMapCore);
        }
        else
        {
            DetachMapCore();
        }

        _clicked.Dispose();
        _cursorMoved.Dispose();
        _status.Dispose();
        _accent.Dispose();
    }
}
