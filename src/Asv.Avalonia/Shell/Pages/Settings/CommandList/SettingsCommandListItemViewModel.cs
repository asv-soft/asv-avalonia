using System.Windows.Input;
using Asv.Common;
using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public class SettingsCommandListItemViewModel : RoutableViewModel
{
    private readonly ICommandService _commandService;

    public SettingsCommandListItemViewModel(ICommandInfo commandInfo, ICommandService svc)
        : base(commandInfo.Id)
    {
        Info = commandInfo;
        _commandService = svc;
        IsReset = new ReactiveProperty<bool>(false).DisposeItWith(Disposable);
        CurrentHotKeyValue = new BindableReactiveProperty<KeyGesture?>(
            Info.CustomHotKey
        ).DisposeItWith(Disposable);
        PreviousHotKeyValue = new BindableReactiveProperty<KeyGesture?>().DisposeItWith(Disposable);
        NewHotKeyValue = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        IsChangingHotKey = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsValid = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsSelected = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);

        IsSelected
            .Subscribe(isSelected =>
            {
                if (!isSelected)
                {
                    IsChangingHotKey.Value = false;
                }
            })
            .DisposeItWith(Disposable);

        IsReset
            .Subscribe(reset =>
            {
                if (!reset)
                {
                    return;
                }

                Info.CustomHotKey = null;
                if (Info.DefaultHotKey is not null)
                {
                    svc.SetHotKey(Info.Id, Info.DefaultHotKey);
                }

                IsReset.Value = false;
                CurrentHotKeyValue.Value = Info.CustomHotKey;
            })
            .DisposeItWith(Disposable);

        NewHotKeyValue
            .Subscribe(_ =>
            {
                if (_ == null)
                {
                    return;
                }

                try
                {
                    KeyGesture.Parse(_.TrimEnd('+'));
                    IsValid.Value = true;
                }
                catch (Exception)
                {
                    IsValid.Value = false;
                }
            })
            .DisposeItWith(Disposable);
        ChangeHotKeyCommand = IsSelected
            .ToReactiveCommand(_ =>
            {
                if (commandInfo.DefaultHotKey is null)
                {
                    return;
                }

                NewHotKeyValue.Value = string.Empty;
                PreviousHotKeyValue.Value = Info.CustomHotKey ?? Info.DefaultHotKey;
                IsChangingHotKey.Value = true;
            })
            .DisposeItWith(Disposable);
        CancelChangeHotKeyCommand = new ReactiveCommand(_ =>
        {
            NewHotKeyValue.Value = PreviousHotKeyValue.Value?.ToString();
            IsChangingHotKey.Value = false;
        }).DisposeItWith(Disposable);
        ConfirmChangeHotKey = new BindableAsyncCommand(ConfirmChangeHotKeyCommand.Id, this);
    }

    internal void ConfirmChangeHotKeyImpl()
    {
        if (NewHotKeyValue.Value == null)
        {
            return;
        }

        Info.CustomHotKey = KeyGesture.Parse(NewHotKeyValue.Value.TrimEnd('+'));
        _commandService.SetHotKey(Info.Id, Info.CustomHotKey);
        CurrentHotKeyValue.Value =
            _commandService.GetHostKey(Info.Id) == Info.DefaultHotKey
                ? null
                : _commandService.GetHostKey(Info.Id);

        IsChangingHotKey.Value = false;
    }

    public ICommandInfo Info { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public bool Filter(string text)
    {
        return (
                Info.CustomHotKey?.ToString().Contains(text, StringComparison.OrdinalIgnoreCase)
                == true
            )
            || (
                Info.DefaultHotKey?.ToString().Contains(text, StringComparison.OrdinalIgnoreCase)
                == true
            )
            || Info.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
            || Info.Source.ModuleName.Contains(text, StringComparison.OrdinalIgnoreCase)
            || Info.Description.Contains(text, StringComparison.OrdinalIgnoreCase);
    }

    public ReactiveProperty<bool> IsReset { get; }
    public BindableReactiveProperty<KeyGesture?> CurrentHotKeyValue { get; }
    public BindableReactiveProperty<KeyGesture?> PreviousHotKeyValue { get; }
    public BindableReactiveProperty<string?> NewHotKeyValue { get; }
    public BindableReactiveProperty<bool> IsChangingHotKey { get; }
    public BindableReactiveProperty<bool> IsValid { get; }
    public BindableReactiveProperty<bool> IsSelected { get; }
    public ReactiveCommand ChangeHotKeyCommand { get; set; }
    public ReactiveCommand CancelChangeHotKeyCommand { get; set; }
    public ICommand ConfirmChangeHotKey { get; set; }
}
