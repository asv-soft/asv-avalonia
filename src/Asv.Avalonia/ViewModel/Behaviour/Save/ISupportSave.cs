namespace Asv.Avalonia.Save;

public interface ISupportSave : IRoutable
{
    ValueTask Save();
}

