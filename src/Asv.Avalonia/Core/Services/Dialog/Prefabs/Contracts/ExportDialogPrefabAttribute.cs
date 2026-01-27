using System.Composition;

namespace Asv.Avalonia;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportDialogPrefabAttribute() : ExportAttribute(typeof(ICustomDialog)) { }
