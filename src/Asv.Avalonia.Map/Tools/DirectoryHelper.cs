namespace Asv.Avalonia.Map;

public static class DirectoryHelper
{
    public static void GetDirectorySize(string path, ref long count, ref long size)
    {
        DirectoryInfo dir = new DirectoryInfo(path);

        // Проверяем, существует ли директория
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Директория {path} не найдена.");
        }

        // Подсчитываем файлы и их размеры в текущей директории
        foreach (FileInfo file in dir.GetFiles())
        {
            count++; // Увеличиваем счетчик файлов
            size += file.Length; // Добавляем размер файла
        }

        // Рекурсивно обходим поддиректории
        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            GetDirectorySize(subDir.FullName, ref count, ref size);
        }
    }
}
