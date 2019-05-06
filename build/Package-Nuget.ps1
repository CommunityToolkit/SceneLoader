## Licensed to the .NET Foundation under one or more agreements.
## The .NET Foundation licenses this file to you under the MIT license.
## See the LICENSE file in the project root for more information.

# Create Nuget package and output it to the bin\nupkg directory to be used by the Azure Devops pipeline.

# Creating path variables for directories where files are located and their target locations.
$RootPath = Split-Path $PSScriptRoot -Parent
$NugetPackager = "$RootPath\SceneLoader\NugetPackager\"
$nupkg = "$RootPath\bin\nupkg"

# Remove any previous instances of the Nuget package.
Remove-Item "$NugetPackager\*.nupkg"

# Force create $nupkg folder directory to ensure folder exists and overwrite any previous content.
New-Item -ItemType Directory -Force -Path $nupkg

# Create Nuget package and output to nupkg directory.
Nuget Pack "$NugetPackager\SceneLoaderComponent.nuspec" -OutputDirectory $nupkg
