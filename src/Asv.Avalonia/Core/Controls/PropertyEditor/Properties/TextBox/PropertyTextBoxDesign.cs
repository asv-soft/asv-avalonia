using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class PropertyTextBoxDesign : PropertyTextBoxViewModel
{
    public PropertyTextBoxDesign()
        : base(NavId.GenerateRandomAsString())
    {
        DesignTime.ThrowIfNotDesignMode();
        ShortHeader = "Name";
        Icon = MaterialIconKind.FormTextbox;
        IconColor = AsvColorKind.Info3;
        Header = "Display name";
        Units = "km\\h";
        Description =
            "Editable text property with the same visual structure as combo box properties.";
        Text.Value = "Survey profile";
        Menu.Add(
            new MenuItem("copy_value", "Copy value")
            {
                Icon = MaterialIconKind.ContentCopy,
                Command = DesignTime.EmptyCommand,
            }
        );
        Menu.Add(
            new MenuItem("reset_value", "Reset value")
            {
                Icon = MaterialIconKind.Restore,
                Command = DesignTime.EmptyCommand,
            }
        );
        Menu.Add(
            new MenuItem("more_actions", "More actions") { Icon = MaterialIconKind.DotsHorizontal }
        );
        Menu.Add(
            new MenuItem("validate_value", "Validate", "more_actions")
            {
                Icon = MaterialIconKind.CheckCircle,
                Command = DesignTime.EmptyCommand,
            }
        );

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                MarkUpdated();
            })
            .AddTo(ref DisposableBag);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                IsBusy = !IsBusy;
            })
            .AddTo(ref DisposableBag);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                Text.Value = Text.Value == null ? "Survey profile" : null;
            })
            .AddTo(ref DisposableBag);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                ErrorMessage = ErrorMessage == null ? "Text sync error" : null;
            })
            .AddTo(ref DisposableBag);

        Text.EnableValidation(x =>
            {
                if (string.IsNullOrEmpty(x))
                {
                    return new ValidationException("Text cannot be empty");
                }

                return null;
            })
            .AddTo(ref DisposableBag);
        Text.ForceValidate();
    }

    protected override ValueTask ApplyFromUserCore(CancellationToken cancel)
    {
        return ValueTask.CompletedTask;
    }
}
