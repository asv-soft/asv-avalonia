namespace Asv.Avalonia;

public static class LogViewerMixin
{
    extension(ShellMixin.PageBuilder builder)
    {
        public ShellMixin.PageBuilder UseLogViewerPage()
        {
            builder.Register<LogViewerViewModel, LogViewerView>(LogViewerViewModel.PageId);
            builder.Parent.Parent.Extensions.Register<IHomePage, HomePageLogViewerExtension>();
            return builder;
        }
    }
}
