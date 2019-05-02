# Copy files needed for Nuget package generation to appropriate directory and create Nuget package.
# Then copy Nuget package over to bin\nupkg directory to be used by the Azure Devops pipeline.

# Creating path variables for directories where files are located and their target locations.
$RootPath = Split-Path $PSScriptRoot -Parent
$SceneLoader = "$RootPath\SceneLoader"
$NugetPackager = "$SceneLoader\NugetPackager\"
$lib = "$NugetPackager\lib\uap10.0"
$runtimes = "$NugetPackager\runtimes\win19-x64\native\"
$outputDirRelease = "$RootPath\Release\SceneLoader"
$nupkg = "$RootPath\bin\nupkg"

# Force create $lib folder directory to ensure folder exists and overwrite any previous content.
New-Item -ItemType Directory -Force -Path $lib

# Copy .idl and .winmd files over to newly created $lib folder
Copy-Item -Path "$SceneLoader\SceneLoaderComponent.idl" -Destination $lib
Copy-Item -Path "$outputDirRelease\SceneLoaderComponent.winmd" -Destination $lib

# Force create $runtimes folder directory to ensure folder exists and overwrite any previous content.
New-Item -ItemType Directory -Force -Path $runtimes

# Copy over .pri and .dll files over to newly created $runtimes folder
Copy-Item -Path "$outputDirRelease\SceneLoaderComponent.pri" -Destination $runtimes
Copy-Item -Path "$outputDirRelease\SceneLoaderComponent.dll" -Destination $runtimes

# Save current directory
pushd

# Change to NugetPackager directory
cd $NugetPackager

# Remove any previous instances of the Nuget package
Remove-Item *.nupkg

# Create Nuget package
Nuget Pack

# Force create $nupkg folder directory to ensure folder exists and overwrite any previous content.
New-Item -ItemType Directory -Force -Path $nupkg

# Copy Nuget package to $nupkg directory so it can be used by Azure Devops pipeline.
Copy-Item -Path *.nupkg -Destination $nupkg

# Return to the directory the user was on at the start of the script.
popd
