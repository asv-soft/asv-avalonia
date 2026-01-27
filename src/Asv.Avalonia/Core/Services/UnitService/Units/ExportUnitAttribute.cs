using System.Composition;

namespace Asv.Avalonia;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportUnitAttribute() : ExportAttribute(typeof(IUnit)) { }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportUnitItemAttribute(string unitId) : ExportAttribute(unitId, typeof(IUnitItem)) { }
