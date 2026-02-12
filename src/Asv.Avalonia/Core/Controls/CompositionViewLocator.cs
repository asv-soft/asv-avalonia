using System.Composition.Hosting;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public class CompositionViewLocator : IDataTemplate
{
    #region Static

    public static string GetServiceKeyForView(Type type) =>
        type.FullName ?? throw new ArgumentException("type is null", nameof(type));

    public static string GetServiceKeyForView<T>() =>
        typeof(T).FullName ?? throw new ArgumentException("type is null", typeof(T).Name);

    #endregion
    
    private const string MetricBaseName = "asv.avalonia.viewlocator";
    private readonly CompositionHost _container;

    private readonly IServiceProvider _svc;
    private readonly Counter<int> _meterBuildCall;
    private readonly Histogram<double> _hist;
    private KeyValuePair<string, object?> _mefTag = new("IoC", "MEF2");
    private KeyValuePair<string, object?> _serviceProviderTag = new("IoC", "ServiceProvider");

    public CompositionViewLocator(
        CompositionHost container,
        IServiceProvider svc,
        IMeterFactory meterFactory
    )
    {
        _svc = svc;
        _container = container ?? throw new ArgumentNullException(nameof(container));
        var meter = meterFactory.Create(MetricBaseName);
        _meterBuildCall = meter.CreateCounter<int>("build_call");
        _hist = meter.CreateHistogram<double>("build_duration_ms");
    }

    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }
        var sw = Stopwatch.StartNew();

        var viewModelType = data.GetType();

        while (viewModelType != null)
        {
            var viewModelContract = GetServiceKeyForView(viewModelType);

            var obj = _svc.GetKeyedService<Control>(viewModelContract);
            if (obj != null)
            {
                sw.Stop();
                _meterBuildCall.Add(1, _serviceProviderTag);
                _hist.Record(sw.Elapsed.TotalMilliseconds, _serviceProviderTag);
                return obj;
            }

            // try to find view by attribute
            if (_container.TryGetExport<Control>(viewModelContract, out var control))
            {
                sw.Stop();
                _meterBuildCall.Add(1, _mefTag);
                _hist.Record(sw.Elapsed.TotalMilliseconds, _mefTag);
                return control;
            }

            // try to find view by implemented interfaces
            foreach (var @interface in viewModelType.GetInterfaces())
            {
                viewModelContract = GetServiceKeyForView(@interface);
                obj = _svc.GetKeyedService<Control>(viewModelContract);
                if (obj != null)
                {
                    sw.Stop();
                    _meterBuildCall.Add(1, _serviceProviderTag);
                    _hist.Record(sw.Elapsed.TotalMilliseconds, _serviceProviderTag);
                    return obj;
                }

                if (_container.TryGetExport<Control>(viewModelContract, out var ifcControl))
                {
                    sw.Stop();
                    _meterBuildCall.Add(1, _mefTag);
                    _hist.Record(sw.Elapsed.TotalMilliseconds, _mefTag);
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
        sw.Stop();
        return new TextBlock { Text = data.GetType().FullName };
    }

    public bool Match(object? data)
    {
        return data is IViewModel;
    }
}
