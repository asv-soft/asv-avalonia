// Dependency Collector util

using System.Text.Json;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

if (args.Length == 0)
{
    Console.WriteLine("ERROR: .csproj path is not given ( args[0] ).");
    return;
}

var csprojPath = args[0];

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

var project = workspace.OpenProjectAsync(csprojPath).Result;

var packageReferences = project
    .MetadataReferences.Select(metadata => Path.GetFileNameWithoutExtension(metadata.Display))
    .Where(refName => !string.IsNullOrEmpty(refName))
    .ToList();

var dependenciesPath = Path.Combine(Path.GetDirectoryName(csprojPath)!, "dependencies.json");

var json = JsonSerializer.Serialize(
    packageReferences,
    new JsonSerializerOptions { WriteIndented = true }
);
File.WriteAllText(dependenciesPath, json);

Console.WriteLine($"File saved: {dependenciesPath}");
