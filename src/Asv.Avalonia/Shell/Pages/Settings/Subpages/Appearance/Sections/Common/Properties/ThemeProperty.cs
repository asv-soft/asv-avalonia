using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public class ThemeProperty : ViewModel
{
    public const string ViewModelId = "theme";

    private readonly IThemeService _svc;
    private bool _internalChange;
    private readonly IUndoChangeSink<ValueUndoChange<string>> _undoHandler;

    public IEnumerable<IThemeInfo> Items => _svc.Themes;
    public BindableReactiveProperty<IThemeInfo> SelectedItem { get; }

    public ThemeProperty()
        : this(DesignTime.ThemeService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public ThemeProperty(IThemeService svc)
        : base(ViewModelId)
    {
        _svc = svc;
        SelectedItem = new BindableReactiveProperty<IThemeInfo>(
            svc.CurrentTheme.CurrentValue
        ).DisposeItWith(Disposable);
        _internalChange = true;
        SelectedItem.SubscribeAwait(OnChangedByUser).DisposeItWith(Disposable);
        svc.CurrentTheme.Subscribe(OnChangeByModel).DisposeItWith(Disposable);
        _internalChange = false;
        _undoHandler = Undo.CreateValueChange<string>("default", ApplyTheme, ApplyTheme)
            .DisposeItWith(Disposable);
    }

    private ValueTask OnChangedByUser(IThemeInfo userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return ValueTask.CompletedTask;
        }

        var oldValue = _svc.CurrentTheme.Value.Id;
        if (oldValue == userValue.Id)
        {
            return ValueTask.CompletedTask;
        }

        try
        {
            _internalChange = true;
            ApplyTheme(userValue.Id);
            _undoHandler.Publish(oldValue, userValue.Id);
            return ValueTask.CompletedTask;
        }
        catch (Exception exception)
        {
            return ValueTask.FromException(exception);
        }
        finally
        {
            _internalChange = false;
        }
    }

    private void ApplyTheme(string themeId)
    {
        var theme = _svc.Themes.FirstOrDefault(x => x.Id == themeId);
        if (theme is null)
        {
            return;
        }

        _svc.CurrentTheme.Value = theme;
    }

    private void OnChangeByModel(IThemeInfo modelValue)
    {
        _internalChange = true;
        SelectedItem.Value = modelValue;
        _internalChange = false;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
