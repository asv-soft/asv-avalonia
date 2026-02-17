using Asv.Avalonia.Save;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class CommandMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseCommands(Action<Builder>? configure = null)
        {
            builder.Services.AddSingleton<ICommandService, CommandService>();
            if (configure == null)
            {
                new Builder(builder).RegisterDefault();
            }
            else
            {
                configure(new Builder(builder));
            }
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeCommands()
        {
            builder.Services.AddSingleton(NullCommandService.Instance);
            return builder;
        }

        public Builder Commands => new(builder);
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder RegisterDefault()
        {
            return Register<CancelCommand>()
                .Register<ClearCommand>()
                .Register<NextPageCommand>()
                .Register<PaginationCommand>()
                .Register<PreviousPageCommand>()
                .Register<RefreshCommand>()
                .Register<SaveCommand>()
                .Register<TextSearchCommand>()
                .Register<OpenDebugWindowCommand>()
                .Register<CreateFileCommand>()
                .Register<OpenFileCommand>()
                .Register<SaveAllLayoutToFileCommand>()
                .Register<SaveLayoutToFileCommand>()
                .Register<OpenHomePageCommand>()
                .Register<OpenLogViewerCommand>()
                .Register<ChangeLanguageFreeCommand>()
                .Register<ChangeThemeFreeCommand>()
                .Register<ResetCommandHotKeysCommand>()
                .Register<ChangeMeasureUnitCommand>()
                .Register<ResetUnitsCommand>()
                .Register<OpenSettingsCommand>()
                .Register<ClosePageCommand>()
                .Register<ChangeBoolPropertyCommand>()
                .Register<ChangeEnumPropertyCommand>()
                .Register<ChangeIntPropertyCommand>()
                .Register<ChangeDoublePropertyCommand>()
                .Register<ChangeStringPropertyCommand>()
                .Register<RedoCommand>()
                .Register<UndoCommand>()
                .Register<RestartApplicationCommand>();
        }

        public Builder Register<TCommand>()
            where TCommand : class, IAsyncCommand
        {
            builder.Services.AddSingleton<IAsyncCommand, TCommand>();
            return this;
        }
    }
}
