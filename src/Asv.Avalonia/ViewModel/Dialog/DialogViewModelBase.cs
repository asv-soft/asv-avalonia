using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public abstract class DialogViewModelBase : RoutableViewModel
{
    protected const string BaseId = "dialog";

    private readonly HashSet<IBindableReactiveProperty> _validationData = new(
        EqualityComparer<IBindableReactiveProperty>.Default
    );

    protected DialogViewModelBase(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public ReactiveProperty<bool> IsValid { get; } = new(true);

    public virtual void ApplyDialog(ContentDialog dialog)
    {
        return;
    }

    private ValueTask InternalCatchEvent(IRoutable src, AsyncRoutedEvent<IRoutable> e)
    {
        if (e is ValidationEvent validation)
        {
            if (validation.ValidatedObject is not IBindableReactiveProperty property)
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

        return ValueTask.CompletedTask;
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
