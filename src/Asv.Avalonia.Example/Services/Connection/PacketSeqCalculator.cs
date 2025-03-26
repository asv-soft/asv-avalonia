using System.Composition;
using Asv.Mavlink;

namespace Asv.Avalonia.Example;

[Export(typeof(IPacketSequenceCalculator))]
[Shared]
public class PacketSeqCalculator : PacketSequenceCalculator { }
