using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class MapInteractionController : IMapInteractionController, IDisposable
{
    private readonly BindableReactiveProperty<string?> _status = new();
    private readonly BindableReactiveProperty<AsvColorKind?> _accent = new();
    private readonly CompositeDisposable _disposable = new();

    private MapItemsControl? _map;
    private TopLevel? _topLevel;
    private MapInteractionSession? _session;

    public MapInteractionController()
    {
        _status.AddTo(_disposable);
        _accent.AddTo(_disposable);
    }

    public IReadOnlyBindableReactiveProperty<string?> Status => _status;

    public IReadOnlyBindableReactiveProperty<AsvColorKind?> Accent => _accent;

    public bool IsBusy => _session is not null;

    public bool TryBegin([MaybeNullWhen(false)] out IMapInteractionSession session)
    {
        Dispatcher.UIThread.VerifyAccess();

        if (_map is null)
        {
            session = null;
            return false;
        }

        End();
        var fresh = new MapInteractionSession(this);
        _session = fresh;
        session = fresh;
        return true;
    }

    public void AttachMap(MapItemsControl map)
    {
        DetachMap();

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

    public void DetachMap()
    {
        End();

        if (_map is not null)
        {
            _map.RemoveHandler(MapItemsControl.MapClickEvent, OnMapClick);
            _map.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        }

        _topLevel?.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        _topLevel = null;
        _map = null;
    }

    internal void SetStatus(MapInteractionSession session, string? value)
    {
        if (ReferenceEquals(_session, session))
        {
            _status.Value = value;
        }
    }

    internal void SetAccent(MapInteractionSession session, AsvColorKind? value)
    {
        if (ReferenceEquals(_session, session))
        {
            _accent.Value = value;
        }
    }

    internal void End(MapInteractionSession session)
    {
        if (ReferenceEquals(_session, session))
        {
            End();
        }
    }

    private void End()
    {
        if (_session is null)
        {
            return;
        }

        var ending = _session;
        _session = null;
        _status.Value = null;
        _accent.Value = null;
        ending.Complete();
    }

    private void OnMapClick(object? sender, MapClickEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        _session.PushClick(e.Point);
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_session is null || _map is null)
        {
            return;
        }

        _session.PushCursor(_map.CursorPosition);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_session is not null && e.Key == Key.Escape)
        {
            End();
            e.Handled = true;
        }
    }

    public void Dispose()
    {
        DetachMap();
        _disposable.Dispose();
    }
}
