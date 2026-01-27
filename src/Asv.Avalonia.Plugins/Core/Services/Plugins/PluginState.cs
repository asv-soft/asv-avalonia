namespace Asv.Avalonia.Plugins;

public class PluginState
{
    public bool IsUninstalled { get; set; }
    public bool IsLoaded { get; set; }
    public string? LoadingError { get; set; }
    public string? InstalledFromSourceUri { get; set; }

    public void CopyFrom(PluginState state)
    {
        IsLoaded = state.IsLoaded;
        LoadingError = state.LoadingError;
        IsUninstalled = state.IsUninstalled;
        InstalledFromSourceUri = state.InstalledFromSourceUri;
    }
}
