using Asv.Common;
using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public class SettingsKeyMapItemViewModel : RoutableViewModel
{
    public SettingsKeyMapItemViewModel(ICommandInfo commandInfo, ICommandService svc)
        : base(commandInfo.Id)
    {
        Info = commandInfo;
        IsReset.Subscribe(reset =>
        {
            if (!reset)
            {
                return;
            }

            Info.CustomHotKey = null;
            if (Info.DefaultHotKey != null)
            {
                svc.SetHotKey(Info.Id, Info.DefaultHotKey);
            }

            IsReset.Value = false;
            CurrentHotKeyValue.Value = Info.CustomHotKey;
        });
        CurrentHotKeyValue.Value = Info.CustomHotKey;
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
        ChangeHotKeyCommand = new ReactiveCommand(_ =>
        {
            NewHotKeyValue.Value = string.Empty;
            PreviousHotKeyValue.Value = Info.CustomHotKey ?? Info.DefaultHotKey;
            IsChangingHotKey.Value = true;
        });
        CancelChangeHotKeyCommand = new ReactiveCommand(_ =>
        {
            NewHotKeyValue.Value = PreviousHotKeyValue.Value!.ToString();
            IsChangingHotKey.Value = false;
        });
        ConfirmChangeHotKeyCommand = new ReactiveCommand(_ =>
        {
            if (NewHotKeyValue.Value == null)
            {
                return;
            }

            Info.CustomHotKey = KeyGesture.Parse(NewHotKeyValue.Value.TrimEnd('+'));
            svc.SetHotKey(Info.Id, Info.CustomHotKey);
            CurrentHotKeyValue.Value =
                svc.GetHostKey(Info.Id) == Info.DefaultHotKey ? null : svc.GetHostKey(Info.Id);

            IsChangingHotKey.Value = false;
        });
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

    public ReactiveProperty<bool> IsReset { get; set; } = new(false);
    public BindableReactiveProperty<KeyGesture?> CurrentHotKeyValue { get; set; } = new();
    public BindableReactiveProperty<KeyGesture?> PreviousHotKeyValue { get; set; } = new();
    public BindableReactiveProperty<string?> NewHotKeyValue { get; set; } = new();
    public BindableReactiveProperty<bool> IsChangingHotKey { get; set; } = new(false);
    public BindableReactiveProperty<bool> IsValid { get; set; } = new(false);
    public ReactiveCommand ChangeHotKeyCommand { get; set; }
    public ReactiveCommand CancelChangeHotKeyCommand { get; set; }
    public ReactiveCommand ConfirmChangeHotKeyCommand { get; set; }
}
