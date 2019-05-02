# Restore Nuget Packages used by SceneLoader solution

$RootPath = Split-Path $PSScriptRoot -Parent
Nuget Restore "$RootPath\SceneLoader.Sln"
