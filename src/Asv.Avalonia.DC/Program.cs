// Dependency Collector util

// Asv.Avalonia.DC [.csproj file] [output directory]
// Example for if you use it from "bin" on windows:
// .\Asv.Avalonia.DC.exe ..\..\..\..\Asv.Avalonia.Example.Desktop\Asv.Avalonia.Example.Desktop.csproj ..\..\..\..\Asv.Avalonia.Example.Desktop\bin\Debug\net8.0\dependencies.json
using System.Text.Json;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

switch (args.Length)
{
    case 0
    or 1:
        Console.WriteLine("Usage: Asv.Avalonia.DC [.csproj file] [output directory]");
        return;
}

var csprojPath = args[0];
var dependenciesPath = Path.GetFullPath(args[1]);

Console.WriteLine(csprojPath);

if (!File.Exists(csprojPath))
{
    Console.WriteLine($"ERROR: file {csprojPath} is not found.");
    return;
}

Console.WriteLine($"Analyzing .csproj: {csprojPath} ...");

if (!MSBuildLocator.IsRegistered)
{
    MSBuildLocator.RegisterDefaults();
}

using var workspace = MSBuildWorkspace.Create();
workspace.LoadMetadataForReferencedProjects = true;

var project = workspace.OpenProjectAsync(Path.GetFullPath(csprojPath)).Result;

var packageReferences = project
    .MetadataReferences.Select(metadata => Path.GetFileNameWithoutExtension(metadata.Display))
    .Where(refName => !string.IsNullOrEmpty(refName))
    .ToList();

var json = JsonSerializer.Serialize(
    packageReferences,
    new JsonSerializerOptions { WriteIndented = true }
);
File.WriteAllText(dependenciesPath, json);

Console.WriteLine($"File saved: {dependenciesPath}");
