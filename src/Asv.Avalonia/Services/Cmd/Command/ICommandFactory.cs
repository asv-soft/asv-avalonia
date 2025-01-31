using System.Composition;

namespace Asv.Avalonia;

public interface ICommandFactory
{
    ICommandInfo Info { get; }
    IAsyncCommand Create();
    bool CanExecute(IRoutable context, out IRoutable? target);
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportCommandAttribute()
    : ExportAttribute(typeof(ICommandFactory))
{
 
}