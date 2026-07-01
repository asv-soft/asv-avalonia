using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class MainMenuRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder MainMenu => builder.Shell.MainMenu;
    }

    extension(ShellRegistrations.Builder builder)
    {
        public Builder MainMenu => new(builder);

        public ShellRegistrations.Builder RegisterMainMenu(Action<Builder>? configure = null)
        {
            builder.AppBuilder.Extensions.Register<IShell, CreateMenuExtender>();
            builder.AppBuilder.Extensions.Register<IShell, OpenMenuExtender>();

            configure ??= b => b.RegisterDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(ShellRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder Register<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TMenuViewModel
        >()
            where TMenuViewModel : class, IMenuItem
        {
            builder.AppBuilder.Services.AddKeyedTransient<IMenuItem, TMenuViewModel>(
                MainMenuDefaultMenuExtender.Contract
            );
            return this;
        }

        public Builder RegisterDefault()
        {
            return Register<EditMenu>()
                .Register<EditUndoMenu>()
                .Register<EditRedoMenu>()
                .Register<CreateMenu>()
                .Register<OpenMenu>()
                .Register<SaveMenu>()
                .Register<SaveAsMenu>()
                .Register<HelpMenu>()
                .Register<ToolsMenu>()
                .Register<ToolsHomeMenu>()
                .Register<ToolsSettingsMenu>()
                .Register<ViewMenu>();
        }
    }
}
