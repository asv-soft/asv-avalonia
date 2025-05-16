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
        CurrentHotKeyString = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        CurrentHotKey = new BindableReactiveProperty<KeyGesture?>().DisposeItWith(Disposable);
        IsChangingHotKey = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsSelected = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsValid = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);

        IsReset
            .Subscribe(reset =>
            {
                if (!reset)
                {
                    return;
                }

                Info.HotKeyInfo.CustomHotKey.Value = null;
                if (Info.HotKeyInfo.DefaultHotKey is not null)
                {
                    svc.SetHotKey(Info.Id, Info.HotKeyInfo.DefaultHotKey);
                }

                CurrentHotKeyString.Value = string.Empty;
                CurrentHotKey.Value = null;
                IsReset.Value = false;
            })
            .DisposeItWith(Disposable);

        CurrentHotKeyString
            .Subscribe(_ =>
            {
                if (_ == null)
                {
                    return;
                }

                try
                {
                    CurrentHotKey.Value = KeyGesture.Parse(_.TrimEnd('+'));
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
                if (commandInfo.HotKeyInfo.DefaultHotKey is null)
                {
                    return;
                }

                CurrentHotKeyString.Value = string.Empty;
                CurrentHotKey.Value = null;
                IsChangingHotKey.Value = true;
            })
            .DisposeItWith(Disposable);
        CancelChangeHotKeyCommand = new ReactiveCommand(_ =>
        {
            CurrentHotKeyString.Value = Info.HotKeyInfo.CustomHotKey.Value?.ToString();
            CurrentHotKey.Value = Info.HotKeyInfo.CustomHotKey.Value;
            IsChangingHotKey.Value = false;
        }).DisposeItWith(Disposable);
        ConfirmChangeHotKey = new BindableAsyncCommand(ConfirmChangeHotKeyCommand.Id, this);

        IsSelected
            .Subscribe(isSelected =>
            {
                if (!isSelected)
                {
                    CancelChangeHotKeyCommand.Execute(Unit.Default);
                }
            })
            .DisposeItWith(Disposable);
    }

    internal void ConfirmChangeHotKeyImpl()
    {
        if (CurrentHotKey.Value is null)
        {
            return; // TODO: work more on ctrl+z
        }

        if (CurrentHotKey.Value is not null)
        {
            _commandService.SetHotKey(Info.Id, CurrentHotKey.Value);
            Info.HotKeyInfo.CustomHotKey.Value = CurrentHotKey.Value;
        }

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
                Info.HotKeyInfo.CustomHotKey.Value?.ToString()
                    .Contains(text, StringComparison.OrdinalIgnoreCase) == true
            )
            || (
                Info.HotKeyInfo.DefaultHotKey?.ToString()
                    .Contains(text, StringComparison.OrdinalIgnoreCase) == true
            )
            || Info.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
            || Info.Source.ModuleName.Contains(text, StringComparison.OrdinalIgnoreCase)
            || Info.Description.Contains(text, StringComparison.OrdinalIgnoreCase);
    }

    public ReactiveProperty<bool> IsReset { get; }
    public BindableReactiveProperty<string?> CurrentHotKeyString { get; }
    public BindableReactiveProperty<KeyGesture?> CurrentHotKey { get; }
    public BindableReactiveProperty<bool> IsChangingHotKey { get; }
    public BindableReactiveProperty<bool> IsValid { get; }
    public BindableReactiveProperty<bool> IsSelected { get; }
    public ReactiveCommand ChangeHotKeyCommand { get; set; }
    public ReactiveCommand CancelChangeHotKeyCommand { get; set; }
    public ICommand ConfirmChangeHotKey { get; set; }
}
