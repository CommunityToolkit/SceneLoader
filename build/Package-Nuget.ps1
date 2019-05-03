# Copy files needed for Nuget package generation to appropriate directory and create Nuget package.
# Then copy Nuget package over to bin\nupkg directory to be used by the Azure Devops pipeline.

# Creating path variables for directories where files are located and their target locations.
$RootPath = Split-Path $PSScriptRoot -Parent
$SceneLoader = "$RootPath\SceneLoader"
$NugetPackager = "$SceneLoader\NugetPackager\"
$lib = "$NugetPackager\lib\uap10.0"
$runtimesx64 = "$NugetPackager\runtimes\win10-x64\native\"
$runtimesx86 = "$NugetPackager\runtimes\win10-x86\native\"
$runtimesARM = "$NugetPackager\runtimes\win10-ARM\native\"

$outputDirx64 = "$RootPath\x64\Release\SceneLoader"
$outputDirx86 = "$RootPath\Release\SceneLoader"
$outputDirARM = "$RootPath\ARM\Release\SceneLoader"
$nupkg = "$RootPath\bin\nupkg"

# Copy .idl and .winmd files over to $lib folder.
Copy-Item -Path "$SceneLoader\SceneLoaderComponent.idl" -Destination $lib
# .winmd files for c++ only contain metadata and are not platform dependent so we can just use
# the x86 .winmd file for all three platforms.
Copy-Item -Path "$outputDirx86\SceneLoaderComponent.winmd" -Destination $lib

function createRuntimesFolder {
    param( [string] $outputDir, [string] $runtimeDir )
    # Force create $runtimes folder directory to ensure folder exists and overwrite any previous content.
    New-Item -ItemType Directory -Force -Path $runtimeDir

    # Copy over .pri and .dll files over to newly created $runtimes folder.
    Copy-Item -Path "$outputDir\SceneLoaderComponent.pri" -Destination $runtimeDir
    Copy-Item -Path "$outputDir\SceneLoaderComponent.dll" -Destination $runtimeDir
}

createRuntimesFolder -outputDir $outputDirx64 -runtimeDir $runtimesx64
createRuntimesFolder -outputDir $outputDirx86 -runtimeDir $runtimesx86
createRuntimesFolder -outputDir $outputDirARM -runtimeDir $runtimesARM

# Remove any previous instances of the Nuget package.
Remove-Item $NugetPackager\*.nupkg

# Force create $nupkg folder directory to ensure folder exists and overwrite any previous content.
New-Item -ItemType Directory -Force -Path $nupkg

# Create Nuget package and output to nupkg directory.
Nuget Pack "$NugetPackager\SceneLoaderComponent.nuspec" -OutputDirectory $nupkg
