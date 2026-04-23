namespace Asv.Avalonia.Save;

public interface ISupportSave : IViewModel
{
    ValueTask Save();
}
