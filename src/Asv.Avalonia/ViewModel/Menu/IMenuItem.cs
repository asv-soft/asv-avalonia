using Avalonia.Input;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public interface IMenuItem : IActionViewModel
{
    NavigationId ParentId { get; }
    bool StaysOpenOnClick { get; }
    public bool IsEnabled { get; }
    public KeyGesture? HotKey { get; }
}

public static class MenuItemMixin
{
    public static IMenuItem CreateMenu(this ICommandInfo cmdInfo, ILoggerFactory loggerFactory)
    {
        var item = new MenuItem(cmdInfo.Id, cmdInfo.Name, loggerFactory)
        {
            Description = cmdInfo.Description,
            Icon = cmdInfo.Icon,
        };
        item.Command = cmdInfo.CreateSystemCommand(item);
        return item;
    }
}
