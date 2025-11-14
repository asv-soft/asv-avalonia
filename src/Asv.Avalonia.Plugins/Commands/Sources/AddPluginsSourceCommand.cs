using System.Composition;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Plugins;

[ExportCommand]
[method: ImportingConstructor]
public sealed class AddPluginsSourceCommand(
    IPluginManager pluginManager,
    INavigationService navigationService,
    ILoggerFactory loggerFactory
) : ContextCommand<SettingsPluginsSourcesViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.plugins.sources.add";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenPluginsMarketCommand_CommandInfo_Name,
        Description = RS.OpenPluginsMarketCommand_CommandInfo_Description,
        Icon = MaterialIconKind.AddNetwork,
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
            Title = RS.PluginsSourcesViewModel_AddImpl_Title,
            PrimaryButtonText = RS.PluginsSourcesViewModel_AddImpl_Add,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.PluginsSourcesViewModel_AddImpl_Cancel,
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
