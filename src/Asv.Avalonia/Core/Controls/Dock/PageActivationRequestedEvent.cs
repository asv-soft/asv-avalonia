using Asv.Common;
using Asv.Modeling;

namespace Asv.Avalonia;

/// <summary>Requests activation of the window hosting an already open page.</summary>
public sealed class PageActivationRequestedEvent(IPage page)
    : AsyncRoutedEvent<IViewModel>(page, RoutingStrategy.Bubble);
