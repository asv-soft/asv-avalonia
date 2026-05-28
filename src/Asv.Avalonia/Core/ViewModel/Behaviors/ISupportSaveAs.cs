namespace Asv.Avalonia;

public interface ISupportSaveAs : ISupportSave
{
    string? CurrentFilePath { get; }

    string? DefaultFileName { get; }

    string? DefaultExtension { get; }

    string? TypeFilter { get; }

    ValueTask SaveAs(string filePath, CancellationToken cancel = default);
}
