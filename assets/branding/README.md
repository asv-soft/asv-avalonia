# ASV application and package icon

The faceted violet-and-cyan A is shared by ASV applications and NuGet packages.
Drones and RSgA applications retain their own branding.

- `asv-icon.png`: 256 × 256 NuGet package artwork.
- `asv-icon.ico`: Windows executable artwork, with 16–256 px resolutions.

`src/Directory.Build.targets` includes the PNG in each package and assigns the
ICO to desktop and console executables. Mobile applications use platform assets.
The original artwork and the macOS ICNS are maintained in `asv-avalonia/assets/branding`.

## Editable vector artwork

- `asv-icon.svg`: editable reconstruction with the graphite application tile.
- `asv-mark.svg`: the same faceted A on a transparent background.

These SVG files reconstruct the approved raster artwork using paths and gradients.
They are not original generator source files and contain no embedded raster images.
Existing application and NuGet PNG/ICO assets retain the approved raster artwork.

The Example Desktop application and launcher also set the macOS process Dock icon
after Avalonia platform setup, including direct launches from Rider or `dotnet run`.
