using Asv.Modeling;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia
{
    public class MenuItem : ActionViewModel, IMenuItem
    {
        public MenuItem(
            string id,
            string header,
            ILoggerFactory loggerFactory,
            string? parentId = null
        )
            : base(id, loggerFactory)
        {
            ParentId = parentId == null ? NavId.Empty : new NavId(parentId);
            Order = 0;
            Header = header;
        }

        public NavId ParentId { get; }

        public bool StaysOpenOnClick
        {
            get;
            set => SetField(ref field, value);
        }

        public bool IsEnabled
        {
            get;
            set => SetField(ref field, value);
        } = true;

        public KeyGesture? HotKey
        {
            get;
            set => SetField(ref field, value);
        }

        public MenuItemToggleType ToggleType
        {
            get;
            set => SetField(ref field, value);
        }

        public bool IsChecked
        {
            get;
            set => SetField(ref field, value);
        }

        public string? GroupName
        {
            get;
            set => SetField(ref field, value);
        }

        public override IEnumerable<IViewModel> GetChildren()
        {
            return [];
        }
    }
}
