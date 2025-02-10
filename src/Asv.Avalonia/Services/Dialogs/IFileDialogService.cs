namespace Asv.Avalonia;

public interface IFileDialogService
{
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
    /// Opens dialog to observe a folder
    /// </summary>
    /// <param name="title">caption of the dialog.</param>
    /// <param name="defaultPath">default path.</param>
    /// <returns>Folder path or null if dialog was canceled.</returns>
    Task ShowObserveFolderDialog(string title, string? defaultPath);
}

public interface ISimpleDialogService
{
    /// <summary>
    /// Gets a value indicating whether indicates whether the functionality to show an open file dialog is implemented in the dialog service.
    /// </summary>
    bool IsImplementedShowYesNoDialogDialog { get; }
    
    /// <summary>
    /// Gets a value indicating whether indicates whether the functionality to show an open file dialog is implemented in the dialog service.
    /// </summary>
    bool IsImplementedShowSaveCancelDialog { get; }
    
    /// <summary>
    /// Gets a value indicating whether indicates whether the functionality to show an open file dialog is implemented in the dialog service.
    /// </summary>
    bool IsImplementedShowUnitInputDialog { get; }
    
    /// <summary>
    /// Shows a Yes/No dialog to the user
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message displayed in the dialog.</param>
    /// <returns>True if Yes is selected, false if No is selected.</returns>
    Task<bool> ShowYesNoDialog(string title, string message);

    /// <summary>
    /// Displays a dialog to the user with options to Save, Don't Save, or Cancel.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message to be displayed in the dialog, providing context for the user's decision.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation. The result is an enum value indicating the user's choice: 
    /// Save, Don't Save, or Cancel.</returns>
    Task<bool> ShowSaveCancelDialog(string title, string message);
    
    /// <summary>
    /// Shows a dialog for entering a unit of measurement.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message displayed in the dialog.</param>
    /// <returns>The value entered by the user, or null if the dialog was canceled.</returns>
    Task<string?> ShowUnitInputDialog(string title, string message);
}
