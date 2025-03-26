using System;
using System.Composition;


namespace Asv.Avalonia.Example;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ExportRttItemAttribute() : ExportAttribute(Contract, typeof(IRttItem))
{
    public const string Contract = "shell.flight.uav.rtt";
}