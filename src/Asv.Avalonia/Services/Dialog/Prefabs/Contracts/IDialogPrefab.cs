namespace Asv.Avalonia;

public interface ICustomDialog { }

public interface IDialogPrefab<in TPayload, TOutput> : ICustomDialog
    where TPayload : class
{
    /// <summary>
    /// Shows a dialog to the user.
    /// </summary>
    /// <param name="dialogPayload">
    /// <see cref="TPayload"/> contains data required for the dialog.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TOutput}"/> that represents an asynchronous operation.
    /// The <see cref="TOutput"/> is the result of the user's interaction with the dialog.
    /// </returns>
    public Task<TOutput> ShowDialogAsync(TPayload dialogPayload);
}
