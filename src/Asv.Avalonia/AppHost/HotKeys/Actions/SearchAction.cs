using Asv.Common;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class SearchAction : HotKeyAction<IShell>
{
    public const string Id = "search";
    public override string ActionId => Id;
    public override string Name => RS.FocusSearchBoxCommand_CommandInfo_Name;
    public override string Description => RS.FocusSearchBoxCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => MaterialIconKind.Search;
    public override KeyGesture DefaultHotKey => new(Key.F, KeyModifiers.Control);
    protected override async ValueTask<bool> Execute(IShell target, CancellationToken cancel)
    {
        var found = await TreeVisitorEvent.VisitAll<ISupportTextSearch>(target, cancel);
        if (found.Count == 0)
        {
            return false;
        }

        // we assume that the ISearchBox with the longest path to root is the main search box
        found.MaxItem(x => RoutableMixin.GetPathFromRoot(x).Count)?.Focus();
        return false;
    }
}