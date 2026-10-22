using Material.Icons;

namespace Asv.Avalonia.IO;

public sealed class PacketInspectorWidget : WorkspaceWidget
{
    public PacketInspectorWidget(
        string id,
        string header,
        WorkspaceDock position,
        MaterialIconKind icon,
        PacketInspectorPageViewModel owner
    )
        : base(id)
    {
        Header = header;
        Position = position;
        Icon = icon;
        Owner = owner;
        SetParent(owner);
    }

    public PacketInspectorPageViewModel Owner { get; }
}
