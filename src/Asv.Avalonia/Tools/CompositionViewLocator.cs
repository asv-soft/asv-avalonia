using System.Composition.Hosting;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Asv.Avalonia;

public class CompositionViewLocator(CompositionHost container) : IDataTemplate
{
    private readonly CompositionHost _container =
        container ?? throw new ArgumentNullException(nameof(container));

    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var viewModelType = data.GetType();

        while (viewModelType != null)
        {
            var viewModelContract = viewModelType.FullName;
            if (viewModelContract == null)
            {
                break;
            }

            // try to find view by attribute
            if (_container.TryGetExport<Control>(viewModelContract, out var control))
            {
                return control;
            }

            // try to find view by implemented interfaces
            foreach (var @interface in viewModelType.GetInterfaces())
            {
                viewModelContract = @interface.FullName;
                if (_container.TryGetExport<Control>(viewModelContract, out var ifcControl))
                {
                    return ifcControl;
                }
            }

            /*// try default Avalonia behaviour: rename and try to find view
            var type = Type.GetType(viewModelContract.Replace("ViewModel", "View"));
            if (type != null)
            {
                // ReSharper disable once NullableWarningSuppressionIsUsed
                var ctrl = Activator.CreateInstance(type) as Control;
                return ctrl ?? new TextBlock { Text = data.GetType().FullName };
            }*/

            // try to find view for parent class
            viewModelType = viewModelType.BaseType;
        }

        return new TextBlock { Text = data.GetType().FullName };
    }

    public bool Match(object? data)
    {
        return data is IViewModel;
    }
}
