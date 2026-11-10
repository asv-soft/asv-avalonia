namespace Asv.Avalonia.GeoMap;

/// <summary>Localized labels for the map preload dialog.</summary>
public static class MapPreloadStrings
{
    public static string Title => Get("Title", "Download map area");
    public static string Instructions =>
        Get("Instructions", "Click the map or drag the marker to choose the center.");
    public static string Radius => Get("Radius", "Radius, km");
    public static string MinZoom => Get("MinZoom", "Minimum zoom");
    public static string MaxZoom => Get("MaxZoom", "Maximum zoom");
    public static string RequestDelay => Get("RequestDelay", "Request delay, ms");
    public static string Concurrency => Get("Concurrency", "Parallel requests");
    public static string Providers => Get("Providers", "Map providers");
    public static string Download => Get("Download", "Download");
    public static string Stop => Get("Stop", "Stop");
    public static string Estimating => Get("Estimating", "Counting tiles…");
    public static string Ready => Get("Ready", "Ready");
    public static string Downloading => Get("Downloading", "Downloading…");
    public static string Stopping => Get("Stopping", "Stopping…");
    public static string Stopped => Get("Stopped", "Stopped. Download again to resume.");
    public static string Completed => Get("Completed", "Download completed.");
    public static string InvalidParameters =>
        Get("InvalidParameters", "Choose at least one provider and a valid radius and zoom range.");
    public static string EstimateFormat => Get("EstimateFormat", "Tiles to process: {0:N0}");
    public static string ProgressFormat => Get("ProgressFormat", "{0:N0} / {1:N0} tiles ({2:F1}%)");
    public static string StatisticsFormat =>
        Get(
            "StatisticsFormat",
            "Cached: {0:N0} · Downloaded: {1:N0} · Errors: {2:N0} · Received: {3:N1} MB"
        );
    public static string CenterFormat => Get("CenterFormat", "Center: {0:F6}, {1:F6}");

    private static string Get(string key, string fallback) =>
        RS.ResourceManager.GetString($"MapPreloadDialog_{key}", RS.Culture) ?? fallback;
}
