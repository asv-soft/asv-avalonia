using System.Diagnostics;
using Newtonsoft.Json;

namespace Asv.Avalonia.Plugins;

public class PluginState
{
    private const string PluginStateFileName = "__PLUGIN_STATE__";
    
    public static PluginState? Read(string folder)
    {
        var stateFilePath = Path.Combine(folder, PluginStateFileName);
        return !File.Exists(stateFilePath) ? null : JsonConvert.DeserializeObject<PluginState>(File.ReadAllText(stateFilePath));
    }
    
    public static PluginState Write(string folder, PluginState state)
    {
        var stateFilePath = Path.Combine(folder, PluginStateFileName);
        if (File.Exists(stateFilePath))
        {
            File.Delete(stateFilePath);
        }
        File.WriteAllText(stateFilePath, JsonConvert.SerializeObject(state));
        return state;
    }

    public static void Edit(string folder, Action<PluginState> edit)
    {
        var state = Read(folder) ?? new PluginState();
        edit(state);
        Write(folder, state);
    }
    
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
