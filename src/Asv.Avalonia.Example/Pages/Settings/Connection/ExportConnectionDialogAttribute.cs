using System;
using System.Composition;
using Asv.Avalonia.Example;

namespace Asv.Avalonia.Example;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportConnectionDialogAttribute : ExportAttribute
{
    public ExportConnectionDialogAttribute(string id)
        : base(id, typeof(IConnectionDialog)) { }
}
