namespace Asv.Avalonia;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportMainMenuAttribute() : ExportAttribute(Contract, typeof(IMenuItem))
{
    public const string Contract = "shell.menu.main";
}
