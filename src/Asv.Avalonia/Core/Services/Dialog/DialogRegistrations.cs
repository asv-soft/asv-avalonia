using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class DialogRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder Dialogs => builder.Core.Services.Dialogs;
    }

    extension(ServicesRegistrations.Builder builder)
    {
        public Builder Dialogs => new(builder);

        public ServicesRegistrations.Builder RegisterDialogs(Action<Builder>? configure = null)
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                return builder.RegisterDesignTimeDialogs(configure);
            }

            builder.AppBuilder.Services.AddSingleton<IDialogService, DialogService>();

            builder
                .AppBuilder.ViewLocator.RegisterViewFor<
                    DialogItemHotKeyCaptureViewModel,
                    DialogItemHotKeyCaptureView
                >()
                .RegisterViewFor<DialogItemTextViewModel, DialogItemTextView>()
                .RegisterViewFor<DialogItemTextBoxViewModel, DialogItemTextBoxView>()
                .RegisterViewFor<DialogItemHotKeyCaptureViewModel, DialogItemHotKeyCaptureView>()
                .RegisterViewFor<DialogItemUnsavedChangesViewModel, DialogItemUnsavedChangesView>()
                .RegisterViewFor<DialogItemMarkdownViewModel, DialogItemMarkdownView>();

            var customBuilder = new Builder(builder);
            if (configure == null)
            {
                customBuilder.RegisterDefault();
            }
            else
            {
                configure(customBuilder);
            }
            return builder;
        }

        private ServicesRegistrations.Builder RegisterDesignTimeDialogs(
            Action<Builder>? configure = null
        )
        {
            builder.AppBuilder.Services.AddSingleton(NullDialogService.Instance);
            return builder;
        }
    }

    public class Builder(ServicesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            return RegisterPrefab<HotKeyCaptureDialogPrefab>()
                .RegisterPrefab<InputDialogPrefab>()
                .RegisterPrefab<MarkdownDetailsDialogPrefab>()
                .RegisterPrefab<ObserveFolderDialogPrefab>()
                .RegisterPrefab<OpenFileDialogDesktopPrefab>()
                .RegisterPrefab<SaveCancelDialogPrefab>()
                .RegisterPrefab<SaveFileDialogDesktopPrefab>()
                .RegisterPrefab<SelectFolderDialogDesktopPrefab>()
                .RegisterPrefab<UnsavedChangesDialogPrefab>()
                .RegisterPrefab<YesOrNoDialogPrefab>();
        }

        public Builder RegisterPrefab<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TCustomDialog
        >()
            where TCustomDialog : class, ICustomDialog
        {
            builder.AppBuilder.Services.AddTransient<ICustomDialog, TCustomDialog>();
            return this;
        }
    }
}
