﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Asv.Avalonia;

/// <summary>
/// Represents the base implementation of a view model that provides
/// property change notifications and a proper disposal mechanism.
/// This class is designed to be inherited by other view models.
/// </summary>
public abstract class ViewModelBase(NavigationId id) : IViewModel
{
    private volatile int _isDisposed;
    private NavigationId _id = id;

    public NavigationId Id
    {
        get => _id;
        private set => SetField(ref _id, value);
    }

    public override string ToString()
    {
        return $"{GetType().Name}[{Id}]";
    }

    public void InitArgs(string? args)
    {
        if (args == Id.Args)
        {
            return;
        }

        Id = Id.ChangeArgs(args);
        InternalInitArgs(args);
    }

    protected virtual void InternalInitArgs(string? args) { }

    #region Dispose

    /// <summary>
    /// Gets a value indicating whether the view model has been disposed.
    /// </summary>
    public bool IsDisposed => _isDisposed != 0;

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if the view model has already been disposed.
    /// This ensures that disposed objects are not accessed unexpectedly.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ThrowIfDisposed()
    {
        if (_isDisposed == 0)
        {
            return;
        }

        throw new ObjectDisposedException(GetType().FullName);
    }

    /// <summary>
    /// Releases resources used by the view model.
    /// Ensures that the disposal operation is only performed once.
    /// </summary>
    public void Dispose()
    {
        // Ensure that Dispose is only executed once
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
        {
            return;
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases managed resources when disposing.
    /// This method must be implemented by derived classes to handle resource cleanup.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> if called from <see cref="Dispose()"/> to release managed resources;
    /// otherwise, <c>false</c> if called from the finalizer.
    /// </param>
    protected abstract void Dispose(bool disposing);

    #endregion

    #region PropertyChanged

    /// <summary>
    /// Occurs when a property value changes.
    /// Implements <see cref="INotifyPropertyChanged"/> to support UI binding updates.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that changed. Automatically set by the caller if not provided.
    /// </param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets the field to the specified value and raises the <see cref="PropertyChanged"/> event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the field.</typeparam>
    /// <param name="field">The backing field reference.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyName">
    /// The name of the property that changed. Automatically set by the caller if not provided.
    /// </param>
    /// <returns>
    /// <c>true</c> if the field value was changed; otherwise, <c>false</c>.
    /// </returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
