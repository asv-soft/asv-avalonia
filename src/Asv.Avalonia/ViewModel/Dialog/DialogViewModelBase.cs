using R3;

namespace Asv.Avalonia;

public abstract class DialogViewModelBase(NavigationId id) : RoutableViewModel(id)
{
    public ReactiveProperty<bool> IsValid { get; } = new(true);

    private readonly HashSet<IBindableReactiveProperty> _validationData = new(
        EqualityComparer<IBindableReactiveProperty>.Default
    );

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is ValidationEvent validation)
        {
            if (validation.Sender is not IBindableReactiveProperty property)
            {
                return ValueTask.CompletedTask;
            }

            validation.IsHandled = true;

            _validationData.Add(property);

            IsValid.Value = _validationData.All(prop => !prop.HasErrors);
        }

        if (e is ExecuteCommandEvent)
        {
            e.IsHandled = true;
        }

        return base.InternalCatchEvent(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _validationData.Clear();
            IsValid.Dispose();
        }

        base.Dispose(disposing);
    }
}
