using System.Windows.Input;
using Avalonia.Input;

namespace Asv.Avalonia
{
    public class MenuItem : TitledViewModel, IMenuItem
    {
        private bool _isVisible = true;
        private bool _staysOpenOnClick;
        private bool _isEnabled = true;
        private KeyGesture? _hotKey;
        private ICommand? _command;
        private object? _commandParameter;

        public MenuItem(string id, string header, string? parentId = null)
            : base(id)
        {
            ParentId = parentId;
            Order = 0;
            Title = header;
        }

        public string? ParentId { get; }

        public int Order { get; set; }

        public ICommand? Command
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

        public KeyGesture? HotKey
        {
            get => _hotKey;
            set => SetField(ref _hotKey, value);
        }

        public override ValueTask<IRoutable> Navigate(string id)
        {
            return ValueTask.FromResult<IRoutable>(this);
        }

        public override IEnumerable<IRoutable> GetRoutableChildren()
        {
            return [];
        }
    }
}
