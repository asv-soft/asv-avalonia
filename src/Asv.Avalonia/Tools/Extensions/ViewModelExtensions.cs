using Asv.Modeling;

namespace Asv.Avalonia;

public static class ViewModelExtensions
{
    extension(IViewModel sender)
    {
        public ValueTask GoTo(NavPath path)
        {
            var rootId = new NavId(ShellViewModel.TypeId);

            NavPath fullPath;
            if (path.Count > 0 && path[0] == rootId)
            {
                fullPath = path;
            }
            else
            {
                var items = new List<NavId> { rootId };
                items.AddRange(path);
                fullPath = new NavPath(items);
            }

            if (sender is IShell shell)
            {
                return GoToShell(shell, fullPath);
            }

            return sender.Events.Rise(new NavigateEvent<IViewModel>(sender, fullPath));
        }
    }

    private static async ValueTask GoToShell(IShell shell, NavPath path)
    {
        await shell.Navigation.GoTo(path);
    }
}
