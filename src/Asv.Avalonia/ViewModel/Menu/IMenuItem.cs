using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia
{
    public interface IMenuItem : IActionViewModel
    {
        NavigationId ParentId { get; }
        bool StaysOpenOnClick { get; }
        public bool IsEnabled { get; }
        public KeyGesture? HotKey { get; }
    }
}
