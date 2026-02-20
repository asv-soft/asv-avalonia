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
            builder.Shell.Pages.Home.RegisterExtension<HomePagePluginExtension>();
            return builder;
        }
    }
}
