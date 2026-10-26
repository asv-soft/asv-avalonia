using System.Text;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Asv.Avalonia.Test.IO;

public class PacketFileExporterTest
{
    [Fact]
    public async Task Export_PreservesAllMessagesInOrderAndEscapesNewlines()
    {
        using var files = new Files();
        var first = new ExampleMessage1 { Value1 = 42, Value3 = "first\nПривет" };
        var second = new ExampleMessage1 { Value1 = 84, Value3 = "second" };
        await File.WriteAllBytesAsync(files.Source, new byte[8191].Concat(first.Serialize()).Concat(second.Serialize()).ToArray(), TestContext.Current.CancellationToken);
        var original = await File.ReadAllBytesAsync(files.Source, TestContext.Current.CancellationToken);
        var exporter = new PacketFileExporter(Protocol.Create(builder => builder.Protocols.RegisterExampleProtocol()));
        var result = await exporter.ExportJsonLinesAsync(files.Source, files.Target, cancellationToken: TestContext.Current.CancellationToken);
        var lines = await File.ReadAllLinesAsync(files.Target, TestContext.Current.CancellationToken);
        Assert.Equal(2, result.Messages);
        Assert.Equal(2, lines.Length);
        var firstLine = JObject.Parse(lines[0]);
        Assert.Equal("first\nПривет", firstLine["message"]!["Value3"]!.Value<string>());
        Assert.Equal(84, JObject.Parse(lines[1])["message"]!["Value1"]!.Value<int>());
        Assert.Equal(JTokenType.Null, firstLine["timestamp"]!.Type);
        Assert.Equal(JTokenType.Null, firstLine["direction"]!.Type);
        Assert.Equal(original, await File.ReadAllBytesAsync(files.Source, TestContext.Current.CancellationToken));
        Assert.False((await File.ReadAllBytesAsync(files.Target, TestContext.Current.CancellationToken)).AsSpan().StartsWith(Encoding.UTF8.GetPreamble()));
    }

    [Fact]
    public async Task Export_CancellationPreservesExistingDestinationAndCleansTemporaryFile()
    {
        using var files = new Files();
        await File.WriteAllBytesAsync(files.Source, Enumerable.Repeat(new ExampleMessage1().Serialize(), 400).SelectMany(x => x).ToArray(), TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(files.Target, "existing", TestContext.Current.CancellationToken);
        using var cancel = new CancellationTokenSource();
        var exporter = new PacketFileExporter(Protocol.Create(builder => builder.Protocols.RegisterExampleProtocol()));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => exporter.ExportJsonLinesAsync(files.Source, files.Target,
            new CancelProgress(cancel), cancel.Token));
        Assert.Equal("existing", await File.ReadAllTextAsync(files.Target, TestContext.Current.CancellationToken));
        Assert.Equal(2, Directory.GetFiles(files.Root).Length);
    }

    [Fact]
    public async Task Export_RejectsSourceAsDestination()
    {
        using var files = new Files();
        await File.WriteAllBytesAsync(files.Source, new ExampleMessage1().Serialize(), TestContext.Current.CancellationToken);
        var exporter = new PacketFileExporter(Protocol.Create(builder => builder.Protocols.RegisterExampleProtocol()));
        await Assert.ThrowsAsync<IOException>(() => exporter.ExportJsonLinesAsync(files.Source, files.Source, cancellationToken: TestContext.Current.CancellationToken));
        Assert.NotEmpty(await File.ReadAllBytesAsync(files.Source, TestContext.Current.CancellationToken));
    }

    private sealed class CancelProgress(CancellationTokenSource cancel) : IProgress<double>
    {
        public void Report(double value) => cancel.Cancel();
    }

    private sealed class Files : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), $"asv-export-{Guid.NewGuid():N}");
        public string Source => Path.Combine(Root, "source.bin");
        public string Target => Path.Combine(Root, "export.jsonl");
        public Files() => Directory.CreateDirectory(Root);
        public void Dispose() => Directory.Delete(Root, true);
    }
}
