using System.Windows.Input;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class HotKeyViewModel : RoutableViewModel
{
    private readonly ICommandInfo _command;
    private readonly ICommandService _svc;

    public HotKeyViewModel(
        IRoutable parent,
        ICommandInfo command,
        ICommandService svc,
        IDialogService dialogService,
        ILoggerFactory loggerFactory
    )
        : base($"{command.Id}.hotkey", loggerFactory)
    {
        Parent = parent;
        _command = command;
        _svc = svc;

        UserHotKey = new HistoricalHotKeyProperty(
            $"{Parent?.Id}.{command.Id}.hotkey.user",
            svc,
            command.Id,
            loggerFactory
        )
        {
            Parent = this,
        };

        EditCommand = new ReactiveCommand(
            async (_, _) =>
            {
                var dialog = dialogService.GetDialogPrefab<HotKeyCaptureDialogPrefab>();
                var payload = new HotKeyCaptureDialogPayload
                {
                    Title = "Edit",
                    Message = "Edit hot key",
                    CurrentHotKey = _command.DefaultHotKey,
                };

                var hotKey = await dialog.ShowDialogAsync(payload);
                if (hotKey != null)
                {
                    UserHotKey.ModelValue.OnNext(hotKey);
                    svc.SetHotKey(_command.Id, hotKey);
                }
            }
        );

        SyncConflict(UserHotKey.ModelValue.Value);

        _sub1 = UserHotKey.ModelValue.Subscribe(SyncConflict);
        _sub2 = svc.OnHotKey.Subscribe(_ => SyncConflict(UserHotKey.ModelValue.Value));
    }

    #region Table columns

    public MaterialIconKind Icon => _command.Icon;
    public string Name => _command.Name;
    public string Description => _command.Description;
    public string Source => _command.Source.ModuleName;
    public string? DefaultHotKey => _command.DefaultHotKey?.ToString();
    public HistoricalHotKeyProperty UserHotKey { get; }
    public BindableReactiveProperty<bool> HasConflict { get; } = new();

    #endregion

    private void SyncConflict(HotKeyInfo? value)
    {
        if (value == null)
        {
            HasConflict.Value = false;
            return;
        }

        var duplicateExists = _svc
            .Commands.Where(c => c.Id != _command.Id)
            .Select(c => _svc.GetHotKey(c.Id))
            .Any(hk => hk == value);

        HasConflict.Value = duplicateExists;
    }

    public ICommand EditCommand { get; init; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return UserHotKey;
    }

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            HasConflict.Dispose();
            UserHotKey.Dispose();
        }

        base.Dispose(disposing);
    }
}
