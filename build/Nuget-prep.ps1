# Copy files needed for Nuget package generation to appropriate folder

$RootPath = Split-Path $PSScriptRoot -Parent
$SceneLoader = $RootPath + '\SceneLoader\'
$lib = $RootPath + '\SceneLoader\NugetPackager\lib\uap10.0\'
$outputDirRelease = $RootPath + '\Release\SceneLoader\'
$runtimes = $RootPath + '\SceneLoader\NugetPackager\runtimes\win19-x64\native\'
$NugetPackager = $RootPath + '\SceneLoader\NugetPackager\'
$nupkg = $RootPath + '\bin\nupkg'

cd $SceneLoader
If(!(Test-Path $lib)) {
    New-Item -ItemType Directory -Force -Path $lib
}
copy-item -path SceneLoaderComponent.idl -destination $lib

cd $outputDirRelease
copy-item -path SceneLoaderComponent.winmd -destination $lib

cd $outputDirRelease
If(!(Test-Path $runtimes)) {
    New-Item -ItemType Directory -Force -Path $runtimes
}
copy-item -path SceneLoaderComponent.pri -destination $runtimes
copy-item -path SceneLoaderComponent.dll -destination $runtimes

cd $NugetPackager
remove-item *.nupkg
nuget pack

If(!(Test-Path $nupkg)) {
    New-Item -ItemType Directory -Force -Path $nupkg
}
copy-item -path *.nupkg -destination $nupkg

cd $PSScriptRoot