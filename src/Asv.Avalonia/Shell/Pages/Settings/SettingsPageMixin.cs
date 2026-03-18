using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class SettingsPageMixin
{
    extension(ShellMixin.PageBuilder builder)
    {
        public ShellMixin.PageBuilder UseSettingsPage(Action<Builder>? configure = null)
        {
            // register settings page and view for it
            builder.Register<SettingsPageViewModel, TreePageView>(SettingsPageViewModel.PageId);

            // register home page tool menu action for settings page
            builder.Parent.Parent.Extensions.Register<IHomePage, HomePageSettingsExtension>();

            // register settings extension for easy extend settings tree menu
            builder.Parent.Parent.Extensions.Register<ISettingsPage, DefaultSettingsExtension>();

            configure ??= b => b.UseDefaultSettings();
            configure(new Builder(builder));
            return builder;
        }

        public Builder Settings => new Builder(builder);
    }

    public class Builder(ShellMixin.PageBuilder builder)
    {
        public Builder UseDefaultSettings()
        {
            return UseUnitsSettings().AddAppearanceSettingsSubPage().AddCommandsSettingsSubPage();
        }

        public Builder UseUnitsSettings()
        {
            return AddSubPage<SettingsUnitsViewModel, SettingsUnitsView, SettingsUnitTreePageMenu>(
                SettingsUnitsViewModel.PageId
            );
        }

        public Builder AddAppearanceSettingsSubPage()
        {
            return AddSubPage<
                SettingsAppearanceViewModel,
                SettingsAppearanceView,
                SettingsAppearanceTreePageMenu
            >(SettingsAppearanceViewModel.PageId);
        }

        public Builder AddCommandsSettingsSubPage()
        {
            return AddSubPage<
                SettingsCommandListViewModel,
                SettingsCommandListView,
                SettingsCommandTreePageMenu
            >(SettingsCommandListViewModel.PageId);
        }

        public Builder AddSubPage<TViewModel, TView>(string pageId)
            where TViewModel : class, ISettingsSubPage
            where TView : Control
        {
            builder.Parent.Parent.ViewLocator.RegisterViewFor<TViewModel, TView>();
            builder.Parent.Parent.Services.AddKeyedTransient<ISettingsSubPage, TViewModel>(pageId);
            return this;
        }

        public Builder AddSubPage<TViewModel, TView, TTreeMenu>(string pageId)
            where TViewModel : class, ISettingsSubPage
            where TView : Control
            where TTreeMenu : class, ITreePage
        {
            AddSubPage<TViewModel, TView>(pageId);
            builder.Parent.Parent.Services.AddKeyedTransient<ITreePage, TTreeMenu>(
                SettingsPageViewModel.PageId
            );
            return this;
        }

        public ShellMixin.PageBuilder Parent => builder;
    }
}
