// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once


// rapidjson (a dependency of GLTF SDK) uses iterators in a way
// that is deprecated on C++17. We can't got back to C++14 because
// C++/WinRT requires C++17. The following line shuts off
// the C++17 warning about the deprecated way of using iterators.
#define _SILENCE_CXX17_ITERATOR_BASE_CLASS_DEPRECATION_WARNING

// std
#include <cstdio>
#include <memory>
#include <iostream>
#include <sstream>
#include <istream>
#include <streambuf>
#include <map>
#include <utility>
#include <string>
#include <algorithm>
#include <iomanip>
#include <vector>

// GLTF SDK
#include <GLTFSDK/GLTF.h>
#include <GLTFSDK/IStreamReader.h>
#include <GLTFSDK/Document.h>
#include <GLTFSDK/Deserialize.h>
#include <GLTFSDK/ExtrasDocument.h>
#include <GLTFSDK/Validation.h>
#include <GLTFSDK/Visitor.h>
#include <GLTFSDK/MeshPrimitiveUtils.h>
#include <GLTFSDK/GLBResourceReader.h>
#include <GLTFSDK/GLTFResourceReader.h>
#include <GLTFSDK/ExtensionsKHR.h>
#include <GLTFSDK/MeshPrimitiveUtils.h>

// Windows
#include <windows.h>
#include <D2d1_1.h>
#include <D3d11_4.h>

// C++/WinRT
#include "winrt/Windows.ApplicationModel.Core.h"
#include "winrt/Windows.Graphics.DirectX.h"
#include "winrt/Windows.UI.Core.h"
#include "winrt/Windows.UI.Composition.h"
#include "winrt/Windows.UI.Composition.Scenes.h"
#include "winrt/Windows.UI.Input.h"
#include "winrt/Windows.UI.Xaml.h"
#include "winrt/Windows.UI.Xaml.Controls.h"
#include "winrt/Windows.Storage.Pickers.h"
#include "winrt/Windows.Storage.Streams.h"
#include "winrt/Windows.Foundation.h"
#include "winrt/Windows.Foundation.Collections.h"
#include "winrt/Windows.Foundation.Numerics.h"
#include "winrt/Windows.Graphics.DirectX.Direct3D11.h"


// FIXME: WinRT ABI headers should be moved to UtilForIntermingledNamespaces.h/cpp
#include <Windows.ui.composition.interop.h>