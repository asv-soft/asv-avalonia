using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public abstract class MapZoomPropertyBase : ViewModel
{
    private bool _internalChange;
    private readonly SynchronizedReactiveProperty<int> _modelProperty;
    private readonly IUndoChangeSink<ValueUndoChange<int>> _undoSink;

    protected MapZoomPropertyBase(
        string id,
        SynchronizedReactiveProperty<int> modelProperty,
        ILoggerFactory loggerFactory
    )
        : base(id)
    {
        _modelProperty = modelProperty;
        SelectedItem = new BindableReactiveProperty<int>().DisposeItWith(Disposable);

        _internalChange = true;
        _undoSink = Undo.CreateValueChange<int>("default", ApplyZoomValue, ApplyZoomValue)
            .DisposeItWith(Disposable);
        SelectedItem
            .SubscribeAwait(
                (userValue, _) =>
                {
                    if (_internalChange)
                    {
                        return ValueTask.CompletedTask;
                    }

                    var oldValue = _modelProperty.Value;
                    if (oldValue == userValue)
                    {
                        return ValueTask.CompletedTask;
                    }

                    try
                    {
                        _internalChange = true;
                        ApplyZoomValue(userValue);
                        _undoSink.Publish(oldValue, userValue);
                        return ValueTask.CompletedTask;
                    }
                    catch (Exception exception)
                    {
                        return ValueTask.FromException(exception);
                    }
                    finally
                    {
                        _internalChange = false;
                    }
                }
            )
            .DisposeItWith(Disposable);
        modelProperty
            .Synchronize()
            .Subscribe(modelValue =>
            {
                _internalChange = true;
                SelectedItem.Value = modelValue;
                _internalChange = false;
            })
            .DisposeItWith(Disposable);
        _internalChange = false;
    }

    public BindableReactiveProperty<int> SelectedItem { get; }

    private void ApplyZoomValue(int value)
    {
        _modelProperty.Value = value;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
