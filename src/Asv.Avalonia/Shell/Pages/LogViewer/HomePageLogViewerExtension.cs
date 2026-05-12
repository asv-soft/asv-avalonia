using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public sealed class HomePageLogViewerExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open-log-viewer")
        {
            Header = RS.OpenLogViewerCommand_Action_Title,
            Description = RS.OpenLogViewerCommand_Action_Description,
            Icon = LogViewerViewModel.PageIcon,
            Command = new ReactiveCommand(_ =>
                context.GoTo(new NavPath(new NavId(LogViewerViewModel.PageId)))
            ).DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);

        context.Tools.Add(action);
    }
}
