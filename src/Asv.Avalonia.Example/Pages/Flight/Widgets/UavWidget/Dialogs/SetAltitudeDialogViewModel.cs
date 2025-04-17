using System;
using System.Collections.Generic;
using Asv.Common;
using R3;

namespace Asv.Avalonia.Example;

public class SetAltitudeDialogViewModel : DialogViewModelBase
{
    public const string DialogId = "dialog.altitude";

    public SetAltitudeDialogViewModel(in IUnit unit)
        : base(DialogId)
    {
        AltitudeUnit =
            unit as AltitudeBase
            ?? throw new InvalidCastException($"Unit must be an {nameof(AltitudeBase)}");
        _sub1 = Altitude.EnableValidation(
            s =>
            {
                var valid = AltitudeUnit.Current.Value.ValidateValue(s);
                return valid;
            },
            this,
            true
        );
    }

    public override void ApplyDialog(ContentDialog dialog)
    {
        _sub2 = IsValid
            .Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled)
            .DisposeItWith(Disposable);
    }

    public AltitudeBase AltitudeUnit { get; }
    public BindableReactiveProperty<string> Altitude { get; } = new();

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            Altitude.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
