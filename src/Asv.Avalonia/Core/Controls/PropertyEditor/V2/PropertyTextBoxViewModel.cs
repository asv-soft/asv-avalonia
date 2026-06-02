using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class PropertyTextBoxViewModel : HeadlinedViewModel, IPropertyViewModel
{
    public PropertyTextBoxViewModel()
        : this(NavId.GenerateRandomAsString())
    {
        DesignTime.ThrowIfNotDesignMode();
        ShortName = "Name";
        Icon = MaterialIconKind.FormTextbox;
        IconColor = AsvColorKind.Info3;
        Header = "Display name";
        Units = "km\\h";
        Description =
            "Editable text property with the same visual structure as combo box properties.";
        Text.Value = "Survey profile";

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                UpdatedFlag = !UpdatedFlag;
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

    public PropertyTextBoxViewModel(string typeId)
        : base(typeId)
    {
        Text = new BindableReactiveProperty<string?>().AddTo(ref DisposableBag);
    }

    public BindableReactiveProperty<string?> Text { get; }

    public bool UpdatedFlag
    {
        get;
        private set => SetField(ref field, value);
    }

    public string? ShortName
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsBusy
    {
        get;
        set => SetField(ref field, value);
    }

    public MaterialIconKind ErrorIcon
    {
        get;
        set => SetField(ref field, value);
    } = MaterialIconKind.CloseNetwork;

    public string? ErrorMessage
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSync
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public string? Units
    {
        get;
        set => SetField(ref field, value);
    }
}
