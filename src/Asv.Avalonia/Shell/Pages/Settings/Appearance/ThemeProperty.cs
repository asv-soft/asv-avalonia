using System.Collections.Immutable;
using System.Composition;
using Avalonia.Controls;
using R3;

namespace Asv.Avalonia;

public class ThemeProperty : RoutableViewModel
{
    private readonly IThemeService _svc;
    private bool _internalChange;
    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    public const string ViewModelId = "theme.current";

    public ThemeProperty()
        : this(DesignTime.ThemeService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public ThemeProperty(IThemeService svc)
        : base(ViewModelId)
    {
        _svc = svc;
        SelectedItem = new BindableReactiveProperty<IThemeInfo>(svc.CurrentTheme.CurrentValue);
        _internalChange = true;
        _sub1 = SelectedItem.SubscribeAwait(OnChangedByUser);
        _sub2 = svc.CurrentTheme.Subscribe(OnChangeByModel);
        _internalChange = false;
    }

    private async ValueTask OnChangedByUser(IThemeInfo userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        _internalChange = true;
        var newValue = new Persistable<string>(userValue.Id);
        await this.ExecuteCommand(ChangeThemeCommand.Id, newValue);
        _internalChange = false;
    }

    private void OnChangeByModel(IThemeInfo modelValue)
    {
        _internalChange = true;
        SelectedItem.Value = modelValue;
        _internalChange = false;
    }

    public IEnumerable<IThemeInfo> Items => _svc.Themes;
    public BindableReactiveProperty<IThemeInfo> SelectedItem { get; }

    public override ValueTask<IRoutable> NavigateTo(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        return ValueTask.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            SelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }
}