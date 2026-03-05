using Asv.Avalonia.Plugins;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

public static class PluginExampleMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UsePluginExample()
        {
            builder.Shell.Pages.Register<HomePageViewModel, HomePageView>(HomePageViewModel.PageId);
            builder.Commands.Register<OpenExamplePageCommand>();
            builder.Shell.Pages.Home.UseExtension<HomePagePluginExtension>();
            return builder;
        }
    }
}

public class PluginEntryPoint : IPluginAppBuilder
{
    public void Register(IHostApplicationBuilder builder)
    {
        builder.UsePluginExample();
    }
}
