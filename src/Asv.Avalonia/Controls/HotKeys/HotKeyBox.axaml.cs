using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Asv.Avalonia;

public sealed partial class HotKeyBox : TemplatedControl
{
    private readonly HashSet<Key> _pressedSymbols = [];
    private readonly TimeSpan _chordTimeout = TimeSpan.FromSeconds(1);
    private IDisposable? _sub1;

    private Key? _firstKey;
    private KeyModifiers _firstMods;
    private bool _firstKeyReleased;
    private Key? _secondKey;
    private DispatcherTimer? _timer;
    private TextBlock? _textBlock;

    public HotKeyBox()
    {
        Focusable = true;
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, OnKeyUp, RoutingStrategies.Tunnel);

        DetachedFromVisualTree += (_, _) =>
        {
            _timer?.Stop();
            _sub1?.Dispose();
        };
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _textBlock =
            e.NameScope.Get<TextBlock>("PART_Text")
            ?? throw new InvalidOperationException("Text block should not be null");
        _textBlock.Focus();

        _sub1 = this.GetObservable(HotKeyProperty)
            .Subscribe(hk => _textBlock.Text = hk?.ToString() ?? string.Empty);

        _textBlock.Text = HotKey?.ToString() ?? string.Empty;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (AutoFocus && !IsFocused)
        {
            // focus after building hit-points
            Dispatcher.UIThread.Post(() => Focus(), DispatcherPriority.Input);
        }
    }

    #region Key processing
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
            {
                if (_firstKey is not null)
                {
                    Commit(final: true);
                }
                return;
            }
            case Key.Delete
            or Key.Back:
                StopTimer();
                HotKey = null;
                ResetCore();
                e.Handled = true;
                return;
            case Key.Escape:
                StopTimer();
                ResetCore();
                return;
        }

        if (IsModifier(e.Key))
        {
            return;
        }

        _pressedSymbols.Add(e.Key);

        // first key (Ctrl+A)
        if (_firstKey is null)
        {
            _firstKey = e.Key;
            _firstMods = e.KeyModifiers;
            _firstKeyReleased = false;
            e.Handled = true;
            return;
        }

        // second key (Ctrl+A+S)
        if (
            !_firstKeyReleased
            && _pressedSymbols.Count == 2
            && _firstMods != KeyModifiers.None
            && e.Key != _firstKey
        )
        {
            _secondKey = e.Key;
            Commit(final: true);
            e.Handled = true;
            return;
        }

        // chord key (Ctrl+A ; S)
        if (_firstKeyReleased && _secondKey is null)
        {
            _secondKey = e.Key;
            Commit(final: true);
            e.Handled = true;
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        _pressedSymbols.Remove(e.Key);

        // check if it's a chord
        if (_firstKey is not null && e.Key == _firstKey && !_firstKeyReleased)
        {
            _firstKeyReleased = true;

            Commit(final: false);
            StartTimer();
            e.Handled = true;
        }
    }
    #endregion

    #region Helpers
    private void Commit(bool final)
    {
        if (_firstKey is null)
        {
            throw new Exception("First key should not be null");
        }

        HotKey = new HotKeyInfo(new KeyGesture(_firstKey.Value, _firstMods), _secondKey);

        if (final)
        {
            ResetCore();
        }
    }

    private void StartTimer()
    {
        StopTimer();
        _timer = new DispatcherTimer { Interval = _chordTimeout };
        _timer.Tick += (_, _) =>
        {
            StopTimer(); // it's a simple gesture
            ResetCore();
        };
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
    }

    private void ResetCore()
    {
        StopTimer();
        _firstKey = null;
        _secondKey = null;
        _firstMods = KeyModifiers.None;
        _firstKeyReleased = false;
        _pressedSymbols.Clear();
    }

    private static bool IsModifier(Key k) =>
        k
            is Key.LeftCtrl
                or Key.RightCtrl
                or Key.LeftShift
                or Key.RightShift
                or Key.LeftAlt
                or Key.RightAlt
                or Key.LWin
                or Key.RWin;
    #endregion
}
