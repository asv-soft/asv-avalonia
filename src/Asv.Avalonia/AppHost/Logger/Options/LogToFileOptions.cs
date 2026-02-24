namespace Asv.Avalonia;

public class LogToFileOptions
{
    public LogToFileOptions(string? folder, int? rollingSizeKb = null)
    {
        if (folder is not null)
        {
            Folder = folder;
        }
        if (rollingSizeKb is not null)
        {
            RollingSizeKb = rollingSizeKb.Value;
        }
    }

    public string Folder { get; } = "logs";
    public int RollingSizeKb { get; } = 50;
}
