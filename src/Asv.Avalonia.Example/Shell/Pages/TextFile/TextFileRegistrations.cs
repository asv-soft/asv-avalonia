using Asv.Avalonia;

namespace Asv.Avalonia.Example;

public static class TextFileRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterTextFile()
        {
            builder.AppBuilder.Pages.Register<TextFilePageViewModel, TextFilePageView>(
                TextFilePageViewModel.PageId
            );
            return builder;
        }
    }
}
