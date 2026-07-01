namespace Asv.Avalonia;

public static class LogViewerRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterLogViewerPage()
        {
            builder.Register<LogViewerViewModel, LogViewerView>(LogViewerViewModel.PageId);
            builder.AppBuilder.Extensions.Register<IHomePage, HomePageLogViewerExtension>();
            return builder;
        }
    }
}
