using R3;

namespace Asv.Avalonia;

public class ReactiveUnitProperty
{
    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    public ReactiveUnitProperty(IUnit unit)
    {
        Value = new BindableReactiveProperty<double>(double.NaN);
        StringValue = new BindableReactiveProperty<string>(string.Empty);
        Unit = unit;
        _sub1 = StringValue.Subscribe(StringToValue);
        _sub2 = Value.Subscribe(ValueToString);
    }

    private void ValueToString(double value)
    {
        throw new NotImplementedException();
    }

    private void StringToValue(string value)
    {
        throw new NotImplementedException();
    }

    public IUnit Unit { get; }

    public BindableReactiveProperty<double> Value { get; }

    public BindableReactiveProperty<string> StringValue { get; }
}