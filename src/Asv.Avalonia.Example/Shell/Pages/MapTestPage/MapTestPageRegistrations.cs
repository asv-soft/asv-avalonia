using Asv.Avalonia;

namespace Asv.Avalonia.Example;

public static class MapTestPageRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterMapTest()
        {
            builder.AppBuilder.Extensions.Register<IHomePage, HomePageMapTestPageExtension>();
            builder.AppBuilder.Pages.Register<MapTestPageViewModel, MapTestPageView>(
                MapTestPageViewModel.PageId
            );
            return builder;
        }
    }
}
