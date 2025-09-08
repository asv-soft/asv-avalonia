using System.Composition;

namespace Asv.Avalonia;

[Export(typeof(ISearchService))]
[Shared]
public class StateSaver : IStateSaver { }
