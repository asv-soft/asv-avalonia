using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public abstract class PropertyViewModel : HeadlinedViewModel, IPropertyViewModel, ISupportFocus
{
    private readonly SerialDisposable _menuView = new();

    protected PropertyViewModel(string typeId)
        : base(typeId)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        _menuView.DisposeItWith(Disposable);
        Menu.ObserveCountChanged().Subscribe(_ => UpdateMenuView()).AddTo(ref DisposableBag);
        UpdateMenuView();
    }

    public MenuTree? MenuView
    {
        get;
        private set => SetField(ref field, value);
    }

    public ObservableList<IMenuItem> Menu { get; } = [];

    public bool UpdatedFlag
    {
        get;
        private set => SetField(ref field, value);
    }

    public string? ShortHeader
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsBusy
    {
        get;
        protected set => SetField(ref field, value);
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

    protected void MarkUpdated()
    {
        UpdatedFlag = false;
        UpdatedFlag = true;
    }

    protected void ApplyErrorFromModel(Exception ex)
    {
        ErrorMessage = ex.Message;
    }

    protected void ClearModelErrors()
    {
        ErrorMessage = null;
    }

    private void UpdateMenuView()
    {
        if (Menu.Count == 0)
        {
            MenuView = null;
            _menuView.Disposable = null;
            return;
        }

        if (MenuView is not null)
        {
            return;
        }

        MenuView = new MenuTree(Menu);
        _menuView.Disposable = MenuView;
    }

    public bool IsFocused
    {
        get;
        set => SetField(ref field, value);
    }

    public void Focus()
    {
        IsFocused = true;
    }
}
