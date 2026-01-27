using System.Composition;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Plugins;

[ExportCommand]
[Shared]
[method: ImportingConstructor]
public sealed class AddPluginsSourceCommand(
    IPluginManager pluginManager,
    INavigationService navigationService,
    ILoggerFactory loggerFactory
) : ContextCommand<SettingsPluginsSourcesViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.settings.sources.add";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.AddPluginsSourceCommand_CommandInfo_Title,
        Description = RS.AddPluginsSourceCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Add,
        DefaultHotKey = null,
        Source = PluginManagerModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        SettingsPluginsSourcesViewModel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        using var viewModel = new SourceDialogViewModel(loggerFactory);
        var dialog = new ContentDialog(viewModel, navigationService)
        {
            Title = RS.SettingsPluginsSourcesViewModel_AddDialog_Title,
            PrimaryButtonText = RS.SettingsPluginsSourcesViewModel_AddDialog_PrimaryButtonText,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = Avalonia.RS.DialogButton_Cancel,
        };

        viewModel.ApplyDialog(dialog);

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            pluginManager.AddServer(
                new PluginServer(
                    viewModel.Name.Value,
                    viewModel.SourceUri.Value,
                    viewModel.Username.Value,
                    viewModel.Password.Value
                )
            );

            context.Refresh();
        }

        return null;
    }
}
