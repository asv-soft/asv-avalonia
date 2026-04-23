using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class PropertyEditorTreeMenu : TreePage
{
    public PropertyEditorTreeMenu(ILoggerFactory loggerFactory)
        : base(
            PropertyEditorPageViewModel.PageId,
            RS.PropertyEditorTreeMenu_PropertyEditorTreeMenu_Property_Editor,
            PropertyEditorPageViewModel.PageIcon,
            PropertyEditorPageViewModel.PageId,
            NavId.Empty,
            loggerFactory
        ) { }
}
