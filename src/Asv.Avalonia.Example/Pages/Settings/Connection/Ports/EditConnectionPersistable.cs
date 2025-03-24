using System.Collections.Generic;
using Asv.IO;

namespace Asv.Avalonia.Example;

public class EditConnectionPersistable
{
    public required IProtocolPort Port;
    public required KeyValuePair<string, string> NewValue;
}
