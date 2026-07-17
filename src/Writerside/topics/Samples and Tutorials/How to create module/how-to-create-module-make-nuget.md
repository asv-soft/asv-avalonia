# Create a NuGet package

If you want to share your module with others, you need to create a NuGet package.

Open the `.csproj` file and add the following properties to the `<PropertyGroup>` section:

```xml
<PackageId>Asv.Module</PackageId>
<Version>1.0.0</Version>
<Authors>Asv</Authors>
<Description>Short description of the library.</Description>
<PackageLicenseExpression>MIT</PackageLicenseExpression>
<RepositoryUrl>https://github.com/mycompany/mylibrary</RepositoryUrl>
```

You can change these values according to your needs; this configuration is provided only as an example.
For more information about NuGet packages, see the official documentation [here](https://learn.microsoft.com/en-us/nuget/).

Next, run the following command in the terminal:

```bash
dotnet pack -c Release
```

This command creates a NuGet package in the `bin/Release` folder.
You can customize the output directory by adding the `-o` parameter.

Now you need to create a local NuGet feed for your package.
You can also publish the package to NuGet.org, but this is outside the scope of this tutorial.

Run the following command in the terminal:

```bash
dotnet nuget add source "/local/nuget/folder" --name YourName 
```

After that, you can install your module from the local feed.
Open your NuGet package manager and install the package from the newly added source.

![install-module](how-to-create-module-install-nuget.png)

## Summary

In this tutorial, we created a simple module with two pages and packaged it as a NuGet package.
The module includes a configurable builder that allows optional features to be enabled or disabled.

You can find the complete source code for the module [here](how-to-create-module-source-code.md)