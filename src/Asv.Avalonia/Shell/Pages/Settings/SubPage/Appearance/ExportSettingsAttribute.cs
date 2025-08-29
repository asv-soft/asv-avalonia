using System.Composition;

namespace Asv.Avalonia;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportSettingsAttribute : ExportAttribute
{
    public ExportSettingsAttribute(string id)
        : base(id, typeof(ISettingsSubPage)) { }
}
