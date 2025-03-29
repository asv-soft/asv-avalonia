using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using R3;
using static System.Double;

namespace Asv.Avalonia.Example;

public class SetAltitudeDialogViewModel : DialogViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IDisposable _sub1;
    private IDisposable _sub2;

    public SetAltitudeDialogViewModel(INavigationService navigation)
        : base("dialog.altitude")
    {
        _navigation = navigation;
        _sub1 = Altitude.EnableValidation(
            s =>
            {
                var success = TryParse(s, out var altitude);
                if (success && altitude > 0)
                {
                    return ValidationResult.Success;
                }

                return ValueTask.FromResult<ValidationResult>(new Exception("Invalid Altitude"));
            },
            this,
            true
        );
    }

    public async Task<double?> ApplyDialog()
    {
        var confirm = false;
        double? altitude;
        var dialog = new ContentDialog(_navigation)
        {
            PrimaryButtonText = "Take Off",
            SecondaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = IsValid.CurrentValue,
            IsSecondaryButtonEnabled = true,
            Content = this,
            PrimaryButtonCommand = new ReactiveCommand(_ => confirm = true),
            SecondaryButtonCommand = new ReactiveCommand(_ => confirm = false),
        };
        _sub2 = IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled);
        await dialog.ShowAsync();
        if (confirm)
        {
            TryParse(Altitude.Value, out var alt);
            altitude = alt;
        }
        else
        {
            altitude = null;
        }

        return await Task.FromResult(altitude);
    }

    public BindableReactiveProperty<string> Altitude { get; set; } = new("0.0");

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void Dispose(bool disposing)
    {
        _sub1.Dispose();
        _sub2.Dispose();
        base.Dispose(disposing);
    }
}
