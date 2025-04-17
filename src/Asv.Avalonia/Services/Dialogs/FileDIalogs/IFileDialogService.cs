namespace Asv.Avalonia;

public interface IFileDialogService
{
    public IShellHost ShellHost { get; set; }

    /// <summary>
    /// Gets a value indicating whether indicates whether the functionality to show an open file dialog is implemented in the dialog service.
    /// </summary>
    bool IsImplementedShowOpenFileDialog { get; }

    /// <summary>
    /// Gets a value indicating whether the functionality to show a save file dialog is implemented in the dialog service.
    /// </summary>
    bool IsImplementedShowSaveFileDialog { get; }

    /// <summary>
    /// Gets a value indicating whether the functionality to show a select folder dialog is implemented in the dialog service.
    /// </summary>
    bool IsImplementedShowSelectFolderDialog { get; }

    /// <summary>
    /// Gets a value indicating whether the functionality to show a dialog for observing a folder is implemented in the dialog service.
    /// </summary>
    bool IsImplementedShowObserveFolderDialog { get; }

    /// <summary>
    /// Opens dialog to choose a file
    /// </summary>
    /// <param name="title">caption of the dialog.</param>
    /// <param name="typeFilter">extension filter, example: "txt, *, nupkg".</param>
    /// <param name="initialDirectory">directory where to start search.</param>
    /// <returns>File path or null if dialog was canceled.</returns>
    Task<string?> ShowOpenFileDialog(
        string title,
        string? typeFilter = null,
        string? initialDirectory = null
    );

    /// <summary>
    /// Opens dialog to save a file
    /// </summary>
    /// <param name="title">caption of the dialog.</param>
    /// <param name="defaultExt">default extension of the file.</param>
    /// <param name="typeFilter">extension filter, example: "txt, *, nupkg".</param>
    /// <param name="initialDirectory">directory where to start search.</param>
    /// <returns>File path or null if dialog was canceled.</returns>
    Task<string?> ShowSaveFileDialog(
        string title,
        string? defaultExt = null,
        string? typeFilter = null,
        string? initialDirectory = null
    );

    /// <summary>
    /// Opens dialog to select a folder
    /// </summary>
    /// <param name="title">caption of the dialog.</param>
    /// <param name="oldPath">default path.</param>
    /// <returns>Folder path or null if dialog was canceled.</returns>
    Task<string?> ShowSelectFolderDialog(string title, string? oldPath = null);

    /// <summary>
    /// Opens dialog to observe a folder.
    /// </summary>
    /// <param name="title">caption of the dialog.</param>
    /// <param name="defaultPath">default path.</param>
    /// <returns>Folder path or null if dialog was canceled.</returns>
    Task ShowObserveFolderDialog(string title, string? defaultPath);
}
