using System.Composition;

namespace Asv.Avalonia;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportPageAttribute : ExportAttribute
{
    public ExportPageAttribute(string pageId)
        : base(pageId, typeof(IPage)) { }
}
