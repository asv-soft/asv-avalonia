namespace Asv.Avalonia;

public class MobileDialogService : IDialogService, ISimpleDialogService
{
    public bool IsImplementedShowOpenFileDialog { get; } = false;
    public bool IsImplementedShowSaveFileDialog { get; } = false;
    public bool IsImplementedShowSelectFolderDialog { get; } = false;
    public bool IsImplementedShowObserveFolderDialog { get; } = false;
    public bool IsImplementedShowYesNoDialogDialog { get; } = false;
    public bool IsImplementedShowSaveCancelDialog { get; } = false;
    public bool IsImplementedShowUnitInputDialog { get; } = false;
    public Task<string?> ShowOpenFileDialog(
        string title,
        string? typeFilter = null,
        string? initialDirectory = null
    )
    {
        throw new System.NotImplementedException();
    }

    public Task<string?> ShowSaveFileDialog(
        string title,
        string? defaultExt = null,
        string? typeFilter = null,
        string? initialDirectory = null
    )
    {
        throw new System.NotImplementedException();
    }

    public Task<string?> ShowSelectFolderDialog(string title, string? oldPath = null)
    {
        throw new System.NotImplementedException();
    }

    public Task ShowObserveFolderDialog(string title, string? defaultPath = null)
    {
        throw new System.NotImplementedException();
    }

    public Task<bool> ShowYesNoDialog(string title, string message)
    {
        throw new System.NotImplementedException();
    }

    public Task<bool> ShowSaveCancelDialog(string title, string message)
    {
        throw new NotImplementedException();
    }

    public Task<string?> ShowUnitInputDialog(string title, string message)
    {
        throw new NotImplementedException();
    }
}
