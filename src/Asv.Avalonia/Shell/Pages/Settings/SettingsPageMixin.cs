using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class SettingsPageMixin
{
    extension(PageMixin.Builder builder)
    {
        public PageMixin.Builder UseSettingsPage(Action<Builder>? configure = null)
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

        public Builder Settings => new(builder);
    }

    public class Builder(PageMixin.Builder builder)
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
            Parent.Parent.Parent.ViewLocator.RegisterViewFor<
                CommonAppearanceSettingsSectionViewModel,
                CommonAppearanceSettingsSectionView
            >();
            Parent.Parent.Parent.Extensions.Register<
                SettingsAppearanceViewModel,
                SettingsAppearanceExtension
            >();
            Parent.Parent.Parent.Extensions.Register<
                ISettingsAppearanceSubPage,
                SettingsAppearanceExtension
            >();

            return AddSubPage<
                SettingsAppearanceViewModel,
                SettingsAppearanceView,
                AppearanceSettingTreePageMenu
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

        public PageMixin.Builder Parent => builder;
    }
}
