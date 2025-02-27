namespace Asv.Avalonia;

public interface IExportInfo
{
    string ModuleName { get; }
}

/// <summary>
/// Represents a base interface for all exported objects in MEF2.
/// Provides metadata about the source module from which the data was exported.
/// </summary>
public interface IExportable
{
    /// <summary>
    /// Gets the export source metadata, which identifies the module that provided the exported data.
    /// </summary>
    IExportInfo Source { get; }
}
