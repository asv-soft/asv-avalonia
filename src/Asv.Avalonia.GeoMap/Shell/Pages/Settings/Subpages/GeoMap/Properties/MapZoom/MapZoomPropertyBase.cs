using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public abstract class MapZoomPropertyBase : ViewModelBase
{
    private bool _internalChange;

    protected MapZoomPropertyBase(
        string id,
        SynchronizedReactiveProperty<int> modelProperty,
        string commandId,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        SelectedItem = new BindableReactiveProperty<int>().DisposeItWith(Disposable);

        _internalChange = true;
        SelectedItem
            .SubscribeAwait(
                async (userValue, cancel) =>
                {
                    if (_internalChange)
                    {
                        return;
                    }

                    _internalChange = true;
                    await this.ExecuteCommand(commandId, new IntArg(userValue), cancel: cancel);
                    _internalChange = false;
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

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
