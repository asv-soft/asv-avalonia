using Avalonia.Input;

namespace Asv.Avalonia
{
    public class MenuItem : ActionViewModel, IMenuItem
    {
        private bool _staysOpenOnClick;
        private bool _isEnabled = true;
        private KeyGesture? _hotKey;

        public MenuItem(string id, string header, string? parentId = null)
            : base(id)
        {
            ParentId = parentId == null ? NavigationId.Empty : new NavigationId(parentId);
            Order = 0;
            Header = header;
        }

        public NavigationId ParentId { get; }

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

        public override IEnumerable<IRoutable> GetRoutableChildren()
        {
            return [];
        }
    }
}
