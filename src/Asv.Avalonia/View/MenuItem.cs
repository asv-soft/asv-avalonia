using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia
{
    public class MenuItem : ViewModelBase, IMenuItem
    {
        private int _order;
        private MaterialIconKind _icon;
        private string _header;
        private bool _isVisible = true;
        private bool _staysOpenOnClick;
        private bool _isEnabled = true;
        private ReadOnlyObservableCollection<IMenuItem>? _items;
        private KeyGesture? _hotKey;
        private ICommand _command;
        private object? _commandParameter;

        public MenuItem(string id)
            : base(id)
        {
        }

        public int Order
        {
            get => _order;
            set => SetField(ref _order, value);
        }

        public MaterialIconKind Icon
        {
            get => _icon;
            set => SetField(ref _icon, value);
        }

        public string Header
        {
            get => _header;
            set => SetField(ref _header, value);
        }

        public ICommand Command
        {
            get => _command;
            set => SetField(ref _command, value);
        }

        public object? CommandParameter
        {
            get => _commandParameter;
            set => SetField(ref _commandParameter, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetField(ref _isVisible, value);
        }

        public bool StaysOpenOnClick
        {
            get => _staysOpenOnClick;
            set => SetField(ref _staysOpenOnClick, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetField(ref _isEnabled, value);
        }

        public virtual ReadOnlyObservableCollection<IMenuItem>? Items
        {
            get => _items;
            set => SetField(ref _items, value);
        }

        public KeyGesture? HotKey
        {
            get => _hotKey;
            set => SetField(ref _hotKey, value);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}