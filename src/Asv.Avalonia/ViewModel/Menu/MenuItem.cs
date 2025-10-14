using Avalonia.Input;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia
{
    public class MenuItem : ActionViewModel, IMenuItem
    {
        public MenuItem(
            string id,
            string header,
            ILayoutService layoutService,
            ILoggerFactory loggerFactory,
            string? parentId = null
        )
            : base(id, layoutService, loggerFactory)
        {
            ParentId = parentId == null ? NavigationId.Empty : new NavigationId(parentId);
            Order = 0;
            Header = header;
        }

        public NavigationId ParentId { get; }

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

        public override IEnumerable<IRoutable> GetRoutableChildren()
        {
            return [];
        }
    }
}
