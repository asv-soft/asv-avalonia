namespace Asv.Avalonia;

public static class HomeMixin
{
    extension(ShellMixin.PageBuilder builder)
    {
        public ShellMixin.PageBuilder UseDefaultHomePage()
        {
            builder.Register<HomePageViewModel, HomePageView>(HomePageViewModel.PageId);
            return builder;
        }
    }
}
