using System.Composition;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Plugins;

[ExportCommand]
[Shared]
[method: ImportingConstructor]
public sealed class EditPluginsSourceCommand(
    INavigationService navigationService,
    ILoggerFactory loggerFactory
) : ContextCommand<PluginsSourceViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.settings.sources.source.edit";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.EditPluginsSourceCommand_CommandInfo_Title,
        Description = RS.EditPluginsSourceCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Edit,
        DefaultHotKey = null,
        Source = PluginManagerModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        PluginsSourceViewModel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        using var viewModel = new SourceDialogViewModel(loggerFactory, context);
        var dialog = new ContentDialog(viewModel, navigationService)
        {
            Title = RS.PluginsSourceViewModel_EditDialog_Title,
            PrimaryButtonText = RS.PluginsSourceViewModel_EditDialog_PrimaryButtonText,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = Avalonia.RS.DialogButton_Cancel,
        };

        viewModel.ApplyDialog(dialog);

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await context.Rise(
                new UpdatePluginsSourceEvent(
                    context,
                    context.Model,
                    new PluginServer(
                        viewModel.Name.Value,
                        viewModel.SourceUri.Value,
                        viewModel.Username.Value,
                        viewModel.Password.Value
                    )
                ),
                cancel
            );
        }

        return null;
    }
}
