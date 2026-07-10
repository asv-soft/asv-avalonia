using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class SettingsPageRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder Settings => builder.Pages.Settings;
    }

    extension(PagesRegistrations.Builder builder)
    {
        public Builder Settings => new(builder);

        public PagesRegistrations.Builder RegisterSettingsPage(Action<Builder>? configure = null)
        {
            // register settings page and view for it
            builder.Register<SettingsPageViewModel, TreePageView>(SettingsPageViewModel.PageId);

            // register home page tool menu action for settings page
            builder.AppBuilder.Extensions.Register<IHomePage, HomePageSettingsExtension>();

            // register settings extension for easy extend settings tree menu
            builder.AppBuilder.Extensions.Register<ISettingsPage, DefaultSettingsExtension>();

            configure ??= b => b.RegisterDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(PagesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterUnitsSubPage();
            this.RegisterAppearanceSubPage();
            return this;
        }

        public Builder AddSubPage<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TViewModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TTreeMenu
        >(string pageId)
            where TViewModel : class, ISettingsSubPage
            where TView : Control
            where TTreeMenu : class, ITreePageMenuItem
        {
            AddSubPage<TViewModel, TView>(pageId);
            builder.AppBuilder.Services.AddKeyedTransient<ITreePageMenuItem, TTreeMenu>(
                SettingsPageViewModel.PageId
            );
            return this;
        }

        public Builder AddSubPage<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TViewModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView
        >(string pageId)
            where TViewModel : class, ISettingsSubPage
            where TView : Control
        {
            builder.AppBuilder.TreePage.Register<
                ISettingsPage,
                ISettingsSubPage,
                TViewModel,
                TView
            >(pageId);

            return this;
        }
    }
}
