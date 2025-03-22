using System.Collections.Generic;

namespace Asv.Avalonia.Example;

public class TryCloseWithApprovalDialogViewModel : DialogViewModelBase
{
    public const string DialogId = "params.close-with-approval-dialog.text";

    public TryCloseWithApprovalDialogViewModel()
        : base(DialogId)
    {
        Message = RS.ParamPageViewModel_DataLossDialog_Content;
    }

    public readonly string Message;

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
