﻿using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Avalonia.Example;

[ExportCommand]
[Shared]
public class RTLCommand : ContextCommand<UavWidgetViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.uav.rtl";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UavAction_Rtl_Name,
        Description = RS.UavAction_Rtl_Description,
        Icon = MaterialIconKind.Home,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null },
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandArg?> InternalExecute(
        UavWidgetViewModel context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        var control = context.Device.GetMicroservice<ControlClient>();
        if (control is null)
        {
            return default;
        }

        control.EnsureGuidedMode(cancel: cancel);
        control.DoRtl(cancel);
        return default;
    }
}
