using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class MainMenuMixin
{
    extension(ShellMixin.Builder builder)
    {
        public MainMenuBuilder MainMenu => new(builder);
    }
    
    public class MainMenuBuilder(ShellMixin.Builder builder)
    {
        public MainMenuBuilder Register<TMenuViewModel>()
            where TMenuViewModel : class, IMenuItem
        {
            builder.Parent.Services.AddKeyedTransient<IMenuItem, TMenuViewModel>(
                MainMenuDefaultMenuExtender.Contract
            );
            return this;
        }

        public ShellMixin.Builder Parent => builder;

        public MainMenuBuilder UseDefault()
        {
            Parent.Parent.Extensions.Register<IShell, CreateMenuExtender>();
            return Register<EditMenu>()
                .Register<EditUndoMenu>()
                .Register<EditRedoMenu>()
                .Register<CreateMenu>()
                .Register<OpenMenu>()
                .Register<HelpMenu>()
                .Register<ToolsMenu>()
                .Register<ToolsHomeMenu>()
                .Register<ToolsSettingsMenu>()
                .Register<ViewMenu>()
                .Register<ViewSaveMenu>()
                .Register<ViewSaveAllMenu>();
        }
    }
}