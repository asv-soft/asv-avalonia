using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class DialogMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseDialogs(Action<Builder>? configure = null)
        {
            builder.Services.AddSingleton<IDialogService, DialogService>();
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

        public IHostApplicationBuilder UseDesignTimeDialogs()
        {
            builder.Services.AddSingleton(NullDialogService.Instance);
            return builder;
        }

        public Builder Dialogs => new(builder);
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder RegisterDefault()
        {
            builder
                .ViewLocator.RegisterViewFor<
                    DialogItemHotKeyCaptureViewModel,
                    DialogItemHotKeyCaptureView
                >()
                .RegisterViewFor<DialogItemTextViewModel, DialogItemTextView>()
                .RegisterViewFor<DialogItemTextBoxViewModel, DialogItemTextBoxView>()
                .RegisterViewFor<DialogItemHotKeyCaptureViewModel, DialogItemHotKeyCaptureView>();

            return RegisterPrefab<HotKeyCaptureDialogPrefab>()
                .RegisterPrefab<InputDialogPrefab>()
                .RegisterPrefab<ObserveFolderDialogPrefab>()
                .RegisterPrefab<OpenFileDialogDesktopPrefab>()
                .RegisterPrefab<SaveCancelDialogPrefab>()
                .RegisterPrefab<SaveFileDialogDesktopPrefab>()
                .RegisterPrefab<SelectFolderDialogDesktopPrefab>()
                .RegisterPrefab<UnsavedChangesDialogPrefab>()
                .RegisterPrefab<YesOrNoDialogPrefab>();
        }

        public Builder RegisterPrefab<TCustomDialog>()
            where TCustomDialog : class, ICustomDialog
        {
            builder.Services.AddTransient<ICustomDialog, TCustomDialog>();
            return this;
        }
    }
}
