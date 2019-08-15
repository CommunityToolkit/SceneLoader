// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include "SceneLoader.g.h"

namespace winrt::SceneLoaderComponent::implementation
{
    struct SceneLoader : SceneLoaderT<SceneLoader>
    {
        SceneLoader() = default;

        winrt::Windows::UI::Composition::Scenes::SceneNode Load(winrt::Windows::Storage::Streams::IBuffer buffer, winrt::Windows::UI::Composition::Compositor compositor);

    private:
        void ParseGLTF(
            BYTE * data, 
            UINT32 capacity,
            winrt::Windows::UI::Composition::Compositor& compositor,
            winrt::Windows::UI::Composition::Scenes::SceneNode& rootNode);
        void DoIt(
            Microsoft::glTF::Document & gltfDoc, 
            std::shared_ptr<Microsoft::glTF::GLTFResourceReader> resourceReader, 
            winrt::Windows::UI::Composition::Compositor& compositor,
            winrt::Windows::UI::Composition::Scenes::SceneNode& rootNode);
    };
}

namespace winrt::SceneLoaderComponent::factory_implementation
{
    struct SceneLoader : SceneLoaderT<SceneLoader, implementation::SceneLoader>
    {
    };
}
