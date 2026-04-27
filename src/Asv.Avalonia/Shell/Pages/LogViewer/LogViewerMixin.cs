namespace Asv.Avalonia;

public static class LogViewerMixin
{
    extension(PageMixin.Builder builder)
    {
        public PageMixin.Builder UseLogViewerPage()
        {
            builder.Register<LogViewerViewModel, LogViewerView>(LogViewerViewModel.PageId);
            builder.Parent.Parent.Extensions.Register<IHomePage, HomePageLogViewerExtension>();
            return builder;
        }
    }
}
