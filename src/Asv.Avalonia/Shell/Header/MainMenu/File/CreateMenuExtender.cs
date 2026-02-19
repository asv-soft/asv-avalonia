using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class CreateMenuExtender(IFileAssociationService svc, ILoggerFactory loggerFactory)
    : IExtensionFor<IShell>
{
    public void Extend(IShell context, CompositeDisposable contextDispose)
    {
        foreach (var file in svc.SupportedFiles.Where(x => x.CanCreate))
        {
            var menu = new MenuItem(
                $"{CreateMenu.MenuId}.{file.Id}",
                file.Title,
                loggerFactory,
                CreateMenu.MenuId
            )
            {
                Icon = file.Icon,
            };
            var cmd = new BindableAsyncCommand(CreateFileCommand.Id, menu);
            menu.Command = cmd;
            menu.CommandParameter = CreateFileCommand.CreateArg(file);
            context.MainMenu.Add(menu);
        }
    }
}
