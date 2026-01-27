using System.Composition;
using Avalonia.Controls;

namespace Asv.Avalonia;

/// <summary>
/// This attribute is used to find a matching _view for the ViewModel in CompositionViewLocator.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportViewForAttribute : ExportAttribute
{
    public ExportViewForAttribute(Type viewModelType)
        : base(viewModelType.FullName, typeof(Control))
    {
        if (viewModelType.IsSubclassOf(typeof(Control)))
        {
            throw new ArgumentException(
                $"{viewModelType} cannot be a _view type. It must be ViewModel type",
                nameof(viewModelType)
            );
        }
    }
}

[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportViewForAttribute<T> : ExportAttribute
{
    public ExportViewForAttribute()
        : base(typeof(T).FullName, typeof(Control))
    {
        if (typeof(T).IsSubclassOf(typeof(Control)))
        {
            throw new ArgumentException(
                $"{typeof(T)} cannot be a _view type. It must be ViewModel type"
            );
        }
    }
}
