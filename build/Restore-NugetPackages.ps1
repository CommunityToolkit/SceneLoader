## Licensed to the .NET Foundation under one or more agreements.
## The .NET Foundation licenses this file to you under the MIT license.
## See the LICENSE file in the project root for more information.

# Restore Nuget Packages used by SceneLoader solution

$RootPath = Split-Path $PSScriptRoot -Parent
Nuget Restore "$RootPath\SceneLoader.Sln"
