using System.Composition;

namespace Asv.Avalonia;

public interface IExtensionFor<in T> : IDisposable
{
    void Extend(T context);
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportExtensionForAttribute<T>() : ExportAttribute(typeof(IExtensionFor<T>));
