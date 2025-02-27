using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class UndoCommand : ContextCommand<IPage>
{
    #region Static

    public const string Id = $"{BaseId}.global.undo";

    public static ICommandInfo StaticInfo { get; } =
        new CommandInfo
        {
            Id = Id,
            Name = RS.UndoCommand_CommandInfo_Name,
            Description = RS.UndoCommand_CommandInfo_Description,
            Icon = MaterialIconKind.UndoVariant,
            DefaultHotKey = KeyGesture.Parse("Ctrl+Z"),
            Source = SystemModule.Instance,
        };

    #endregion
    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<IPersistable?> InternalExecute(
        IPage context,
        IPersistable newValue,
        CancellationToken cancel
    )
    {
        await context.History.UndoAsync(cancel);
        return null;
    }
}
