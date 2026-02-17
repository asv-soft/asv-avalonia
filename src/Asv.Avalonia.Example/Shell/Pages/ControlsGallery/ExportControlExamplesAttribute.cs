using System;

namespace Asv.Avalonia.Example;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportControlExamplesAttribute : ExportAttribute
{
    public ExportControlExamplesAttribute(string id)
        : base(id, typeof(IControlsGallerySubPage)) { }
}
