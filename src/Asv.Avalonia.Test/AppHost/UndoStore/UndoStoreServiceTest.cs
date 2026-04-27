using Asv.Modeling;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Asv.Avalonia.Test;

public class UndoStoreServiceTest : IDisposable
{
    private readonly string _rootFolder = Path.Combine(
        Path.GetTempPath(),
        "Asv.Avalonia.Test",
        Guid.NewGuid().ToString("N")
    );

    [Fact]
    public void CreateUndoHistoryStore_CreatesSafeFolderForNavId()
    {
        Directory.CreateDirectory(_rootFolder);
        var sut = CreateSut();

        var ownerId = new NavId(
            "test.page",
            new NavArgs(
                new KeyValuePair<string, string?>("name", "value with space"),
                new KeyValuePair<string, string?>("path", "a/b?x=1")
            )
        );

        sut.CreateUndoHistoryStore(ownerId);

        var undoRoot = Path.Combine(_rootFolder, "undo");
        var createdFolders = Directory.GetDirectories(undoRoot);

        Assert.Single(createdFolders);
        var folderName = Path.GetFileName(createdFolders[0]);
        Assert.StartsWith("nav_test.page", folderName);
        Assert.DoesNotContain('/', folderName);
        Assert.DoesNotContain('\\', folderName);
    }

    [Fact]
    public void CreateUndoHistoryStore_WithSameNavId_UsesSameFolder()
    {
        Directory.CreateDirectory(_rootFolder);
        var sut = CreateSut();
        var ownerId = new NavId("test.page?arg=value");

        sut.CreateUndoHistoryStore(ownerId);
        sut.CreateUndoHistoryStore(ownerId);

        var createdFolders = Directory.GetDirectories(Path.Combine(_rootFolder, "undo"));
        Assert.Single(createdFolders);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootFolder))
        {
            Directory.Delete(_rootFolder, true);
        }
    }

    private UndoStoreService CreateSut()
    {
        var options = Options.Create(new UndoStoreServiceOptions());
        var environment = new TestHostEnvironment { ContentRootPath = _rootFolder };
        return new UndoStoreService(options, environment);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = nameof(UndoStoreServiceTest);
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
