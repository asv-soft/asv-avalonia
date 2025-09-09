using System.Composition;

namespace Asv.Avalonia;

[Export(typeof(IStateSaverService))]
[Shared]
public class StateSaverServiceService : IStateSaverService { }
