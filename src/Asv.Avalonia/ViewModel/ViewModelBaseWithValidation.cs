using System.ComponentModel;
using System.Reflection;
using R3;

namespace Asv.Avalonia;

public abstract class ViewModelBaseWithValidation : ViewModelBase
{
    protected ReactiveProperty<ValidationResult> IsValid { get; }

    protected ViewModelBaseWithValidation(string id)
        : base(id)
    {
        IsValid = new ReactiveProperty<ValidationResult>(Validate());
    }

    /// <summary>
    /// Subscribes to ErrorChanged of all BindableReactiveProperties of the class.
    /// <remarks>
    /// !!!You must call this method after instantiation of all the BindableReactiveProperties!!!
    /// </remarks>
    /// </summary>
    protected void SubscribeToErrorsChanged()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var propertyType = property.PropertyType;
            if (
                !propertyType.IsGenericType
                || propertyType.GetGenericTypeDefinition() != typeof(BindableReactiveProperty<>)
            )
            {
                continue;
            }

            var value = property.GetValue(this);

            var errorsChangedEvent = propertyType.GetEvent("ErrorsChanged");
            if (errorsChangedEvent is not null)
            {
                var handler = new EventHandler<DataErrorsChangedEventArgs>(
                    (sender, args) => IsValid.OnNext(Validate())
                );
                errorsChangedEvent.AddEventHandler(value, handler);
            }
        }

        IsValid.OnNext(Validate());
    }

    private ValidationResult Validate()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var propertyType = property.PropertyType;
            if (
                !propertyType.IsGenericType
                || propertyType.GetGenericTypeDefinition() != typeof(BindableReactiveProperty<>)
            )
            {
                continue;
            }

            var value = property.GetValue(this);
            if (value is null)
            {
                return new Exception("Initialization has not been completed yet.");
            }

            if (value is not IBindableReactiveProperty bindable)
            {
                return new Exception("HasError property is missing");
            }

            if (bindable.HasErrors)
            {
                return new Exception("Has Error");
            }
        }

        return ValidationResult.Success;
    }

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            IsValid.Dispose();
        }
    }
}
