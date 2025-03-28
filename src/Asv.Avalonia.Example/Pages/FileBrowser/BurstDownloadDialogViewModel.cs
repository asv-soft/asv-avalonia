using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Asv.Mavlink;
using R3;

namespace Asv.Avalonia.Example;

public class BurstDownloadDialogViewModel : DialogViewModelBase
{
    private readonly IFtpService _ftp;
    private readonly INavigationService _navigation;

    public BurstDownloadDialogViewModel(string id, IFtpService ftp, INavigationService navigation)
        : base(id)
    {
        _ftp = ftp;
        _navigation = navigation;
        PacketSize = new BindableReactiveProperty<decimal?>(239).EnableValidation();
    }

    [Range(1, MavlinkFtpHelper.MaxDataSize)]
    public BindableReactiveProperty<decimal?> PacketSize { get; set; }

    public async ValueTask<bool> ApplyDialog()
    {
        var process = true;

        var dialog = new ContentDialog(_navigation)
        {
            Title = "Burst Download", //TODO: localization
            PrimaryButtonText = "Download", //TODO: localization
            SecondaryButtonText = "Cancel", //TODO: localization
            IsPrimaryButtonEnabled = IsValid.Value,
            IsSecondaryButtonEnabled = true,
            Content = this,
            PrimaryButtonCommand = new ReactiveCommand(_ =>
                _ftp.BurstDownloadPacketSize = decimal.ToByte(
                    PacketSize.Value ?? MavlinkFtpHelper.MaxDataSize
                )
            ),
            SecondaryButtonCommand = new ReactiveCommand(_ => process = false),
        };

        _sub1 = IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled);

        await dialog.ShowAsync();

        return process;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    private IDisposable _sub1;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            PacketSize.Dispose();
        }

        base.Dispose(disposing);
    }
}
