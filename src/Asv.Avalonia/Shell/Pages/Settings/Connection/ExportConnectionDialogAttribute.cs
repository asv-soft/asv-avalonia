using System.Composition;

namespace Asv.Avalonia;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportConnectionDialogAttribute : ExportAttribute
{
    public ExportConnectionDialogAttribute(string id)
        : base(id, typeof(IConnectionDialog)) { }
}