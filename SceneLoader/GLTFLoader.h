#pragma once

#include "GLTFLoaderComponent.GLTFLoader.g.h"

namespace winrt::GLTFLoaderComponent::implementation
{
    struct GLTFLoader : GLTFLoaderT<GLTFLoader>
    {
        GLTFLoader() = default;

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

namespace winrt::GLTFLoaderComponent::factory_implementation
{
    struct GLTFLoader : GLTFLoaderT<GLTFLoader, implementation::GLTFLoader>
    {
    };
}
