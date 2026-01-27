namespace Asv.Avalonia.GeoMap;

public static class DirectoryHelper
{
    public static void GetDirectorySize(string path, ref long count, ref long size)
    {
        DirectoryInfo dir = new DirectoryInfo(path);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Directory {path} not found.");
        }

        foreach (FileInfo file in dir.GetFiles())
        {
            count++;
            size += file.Length;
        }

        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            GetDirectorySize(subDir.FullName, ref count, ref size);
        }
    }
}
