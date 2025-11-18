using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public static class ActionViewModelMixin
{
    public static IActionViewModel CreateAction(
        this ICommandInfo info,
        ILoggerFactory loggerFactory,
        string? customTitle = null,
        string? customDescription = null
    )
    {
        var model = new ActionViewModel(info.Id, loggerFactory)
        {
            Icon = info.Icon,
            IconColor = info.IconColor,
            Header = customTitle ?? info.Name,
            Description = customDescription ?? info.Description,
        };
        model.Command = new BindableAsyncCommand(info.Id, model);
        return model;
    }
}
