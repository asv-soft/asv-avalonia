using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Asv.Modeling;
using Material.Icons;

namespace Asv.Avalonia.Example;

public class TextFileHandler(IShellHost shellHost) : IFileHandler
{
    private static readonly FileTypeInfo[] StaticTypes =
    [
        new(
            "asvmd",
            "ASV Markdown file",
            TextFilePageViewModel.FileExtension,
            true,
            true,
            MaterialIconKind.FileOutline
        ),
    ];

    public int Priority => -100;
    public IEnumerable<FileTypeInfo> SupportedFiles => StaticTypes;

    public bool CanOpen(string path)
    {
        return string.Equals(
            Path.GetExtension(path),
            $".{TextFilePageViewModel.FileExtension}",
            StringComparison.OrdinalIgnoreCase
        );
    }

    public async ValueTask Open(string path, CancellationToken cancel = default)
    {
        var shell = shellHost.Shell;
        if (shell == null)
        {
            return;
        }

        await shell.GoTo(
            new NavPath(
                new NavId(TextFilePageViewModel.PageId, TextFilePageViewModel.CreateOpenArgs(path))
            ),
            cancel
        );
    }

    public async ValueTask Create(
        string path,
        FileTypeInfo type,
        CancellationToken cancel = default
    )
    {
        await File.WriteAllTextAsync(path, string.Empty, cancel);
        await Open(path, cancel);
    }
}
