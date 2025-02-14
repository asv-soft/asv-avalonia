namespace Asv.Avalonia;

#pragma warning disable RCS1194
public class CommandNotSupportedContextException(
    ICommandInfo commandInfo,
    IRoutable originContext,
    Type supportedContext
)
    : CommandBaseException(
        commandInfo,
        $"Command '{commandInfo.Id}' is not supported in the context '{originContext.GetType().Name}'. Expected context type: '{supportedContext.Name}'."
    ) { }

public class CommandCannotExecuteException(ICommandInfo commandInfo, IRoutable originContext)
    : CommandBaseException(
        commandInfo,
        $"Command '{commandInfo.Id}' cannot be executed in the context of '{originContext.GetType().Name}'."
    ) { }
