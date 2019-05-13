# SceneLoader for Windows UI

SceneLoader is a library for generating [Windows.UI.Composition.Scenes](https://docs.microsoft.com/uwp/api/windows.ui.composition.scenes) scene graphs from 3D file formats such as [glTF](https://www.khronos.org/gltf/). This project aims to simplify the design-to-code workflow for rendering 3D assets in your Windows applications. 

SceneLoader currently produces a [SceneNode](https://docs.microsoft.com/uwp/api/windows.ui.composition.scenes.scenenode), allowing you to programmatically construct your own [Visual](https://docs.microsoft.com/uwp/api/windows.ui.composition.scenes.scenevisual) tree. A [proposed companion Microsoft.UI.Xaml control](https://github.com/microsoft/microsoft-ui-xaml/issues/686) is expected to enable 3D assets to be loaded from markup without requiring explicit management of the Visual tree.

## <a name="supported"></a> Supported SDKs
* May 2019 Update (18362)

## <a name="documentation"></a> Getting Started
* [Documentation](https://docs.microsoft.com/uwp/api/windows.ui.composition.scenes)
* [Code Sample](/https://github.com/windows-toolkit/SceneLoader/blob/readme/TestViewer/MainPage.xaml.cs)

## Build Status
| Target | Branch | Status | Recommended NuGet package |
| ------ | ------ | ------ | ------ |
| 0.0.1  | master | [![Build Status](https://dev.azure.com/dotnet/WindowsCommunityToolkit/_apis/build/status/windows-toolkit.SceneLoader?branchName=master)](https://dev.azure.com/dotnet/WindowsCommunityToolkit/_build/latest?definitionId=80&branchName=master) | ? |

## Feedback and Requests
Please use [GitHub Issues](https://github.com/windows-toolkit/SceneLoader/issues) for bug reports and feature requests.

## Principles
This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](http://dotnetfoundation.org/code-of-conduct).

## .NET Foundation
This project is supported by the [.NET Foundation](http://dotnetfoundation.org).


[![Build Status](https://dev.azure.com/dotnet/WindowsCommunityToolkit/_apis/build/status/windows-toolkit.SceneLoader?branchName=master)](https://dev.azure.com/dotnet/WindowsCommunityToolkit/_build/latest?definitionId=80&branchName=master)
