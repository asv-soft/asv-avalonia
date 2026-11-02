using Asv.Common;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Material.Icons;

namespace Asv.Avalonia.GeoMap;

/// <summary>Initial map selection and appearance for the preload dialog.</summary>
public sealed class MapPreloadDialogPayload
{
    public GeoPoint InitialCenter { get; init; }
    public MaterialIconKind? Icon { get; init; } = MaterialIconKind.MapMarkerRadius;
    public AsvColorKind IconColor { get; init; } = AsvColorKind.None;
    public DialogSizeMode SizeMode { get; init; } = DialogSizeMode.Expanded;
    public ShellErrorState Status { get; init; } = ShellErrorState.Normal;
}

/// <summary>Shows map preload progress and awaits cancellation before releasing its view model.</summary>
public sealed class MapPreloadDialogPrefab(
    IMapService mapService,
    IMapPreloadService preloadService
) : IDialogPrefab<MapPreloadDialogPayload, MapPreloadProgress?>
{
    public Task<MapPreloadProgress?> ShowDialogAsync(MapPreloadDialogPayload dialogPayload) =>
        ShowDialogCoreAsync(dialogPayload, null);

    /// <summary>Shows the dialog in an explicit owner, including hosts without an application lifetime.</summary>
    public Task<MapPreloadProgress?> ShowDialogAsync(
        MapPreloadDialogPayload dialogPayload,
        TopLevel owner
    )
    {
        ArgumentNullException.ThrowIfNull(owner);
        return ShowDialogCoreAsync(dialogPayload, owner);
    }

    private async Task<MapPreloadProgress?> ShowDialogCoreAsync(
        MapPreloadDialogPayload dialogPayload,
        TopLevel? owner
    )
    {
        using var viewModel = new MapPreloadDialogViewModel(
            mapService,
            preloadService,
            dialogPayload.InitialCenter
        );
        var dialog = new ContentDialog(viewModel)
        {
            Title = MapPreloadStrings.Title,
            Icon = dialogPayload.Icon,
            IconColor = dialogPayload.IconColor,
            SizeMode = dialogPayload.SizeMode,
            Status = dialogPayload.Status,
            CloseButtonText = Avalonia.RS.ShellView_WindowControlButton_Close,
        };
        dialog.Closing += OnClosing;
        dialog.TemplateApplied += OnTemplateApplied;
        try
        {
            await (owner is null ? dialog.ShowAsync() : dialog.ShowAsync(owner));
        }
        finally
        {
            dialog.Closing -= OnClosing;
            dialog.TemplateApplied -= OnTemplateApplied;
            await viewModel.CancelAndWaitAsync();
        }

        return viewModel.LastResult;

        static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs args)
        {
            // The map needs a finite viewport; only the settings panel should scroll.
            if (args.NameScope.Find<ScrollViewer>("ContentScrollViewer") is { } scrollViewer)
            {
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        }

        async void OnClosing(object? sender, ContentDialogClosingEventArgs args)
        {
            var deferral = args.GetDeferral();
            try
            {
                await viewModel.CancelAndWaitAsync();
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
