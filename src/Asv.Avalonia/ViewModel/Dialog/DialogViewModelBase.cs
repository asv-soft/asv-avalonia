using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public abstract class DialogViewModelBase : ViewModel
{
    protected const string BaseId = "dialog";

    private readonly HashSet<IBindableReactiveProperty> _validationData = new(
        EqualityComparer<IBindableReactiveProperty>.Default
    );

    protected DialogViewModelBase(string typeId)
        : base(typeId)
    {
        Events.Catch(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public ReactiveProperty<bool> IsValid { get; } = new(true);

    public virtual void ApplyDialog(ContentDialog dialog)
    {
        return;
    }

    private ValueTask InternalCatchEvent(IViewModel src, AsyncRoutedEvent<IViewModel> e, CancellationToken cancel)
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
