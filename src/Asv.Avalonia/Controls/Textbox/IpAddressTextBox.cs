using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Asv.Avalonia.Textbox;

public class IpAddressTextBox : ItemsControl
{
    private MaskedTextBox? Octet1;
    private MaskedTextBox? Octet2;
    private MaskedTextBox? Octet3;
    private MaskedTextBox? Octet4;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Octet1 = e.NameScope.Find<MaskedTextBox>("Octet1");
        Octet2 = e.NameScope.Find<MaskedTextBox>("Octet2");
        Octet3 = e.NameScope.Find<MaskedTextBox>("Octet3");
        Octet4 = e.NameScope.Find<MaskedTextBox>("Octet4");

        if (Octet1 == null || Octet2 == null || Octet3 == null || Octet4 == null)
        {
            throw new AggregateException();
        }

        Octet1.TextChanged += OnTextChanged;
        Octet2.TextChanged += OnTextChanged;
        Octet3.TextChanged += OnTextChanged;
        Octet4.TextChanged += OnTextChanged;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (Octet1 == null || Octet2 == null || Octet3 == null || Octet4 == null)
        {
            return;
        }

        if (
            Octet1.Text == null
            || Octet2.Text == null
            || Octet3.Text == null
            || Octet4.Text == null
        )
        {
            return;
        }

        var octet1 = ValidateOctet(Octet1.Text);
        var octet2 = ValidateOctet(Octet2.Text);
        var octet3 = ValidateOctet(Octet3.Text);
        var octet4 = ValidateOctet(Octet4.Text);
        Octet1.Text = octet1;
        Octet2.Text = octet2;
        Octet3.Text = octet3;
        Octet4.Text = octet4;

        IpAddressValue = $"{octet1}.{octet2}.{octet3}.{octet4}";
    }

    private string ValidateOctet(string value)
    {
        if (int.TryParse(value, out int num))
        {
            if (num >= 0 && num <= 255)
            {
                return num.ToString();
            }
        }

        return "0";
    }

    public static StyledProperty<string> IpAddressValueProperty = AvaloniaProperty.Register<
        IpAddressTextBox,
        string
    >(nameof(IpAddressValue));

    public string IpAddressValue
    {
        get => GetValue(IpAddressValueProperty);
        set => SetValue(IpAddressValueProperty, value);
    }
}
