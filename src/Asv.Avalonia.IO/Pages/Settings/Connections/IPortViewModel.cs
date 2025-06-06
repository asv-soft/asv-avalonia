﻿using Asv.Avalonia.Routable;
using Asv.IO;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

public interface IPortViewModel : IRoutable, IExportable
{
    BindableReactiveProperty<string> Name { get; }
    void Init(IProtocolPort protocolPort);
    NotifyCollectionChangedSynchronizedViewList<TagViewModel> TagsView { get; }
    MaterialIconKind? Icon { get; }
    BindableReactiveProperty<bool> IsEnabled { get; }
    ReactiveCommand RemovePortCommand { get; }
}
