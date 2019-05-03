// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once

#include "SceneResourceSet.h"

namespace SceneLoader
{
    struct GLTFVisitor
    {
        GLTFVisitor(winrt::Windows::UI::Composition::Compositor compositor,
                    winrt::Windows::UI::Composition::Scenes::SceneNode rootSceneNode,
                    std::shared_ptr<SceneResourceSet> resourceSet,
                    std::shared_ptr<Microsoft::glTF::GLTFResourceReader> gltfResourceReader,
                    Microsoft::glTF::Document& gltfDocument,
                    Microsoft::glTF::Scene& gltfScene);

        // Node
        void operator()(const Microsoft::glTF::Node& node, const Microsoft::glTF::Node* nodeParent);

        // Mesh
        void operator()(const Microsoft::glTF::Mesh& mesh, Microsoft::glTF::VisitState alreadyVisited);

        // MeshPrimitive
        void operator()(const Microsoft::glTF::MeshPrimitive&, Microsoft::glTF::VisitState, const Microsoft::glTF::VisitDefaultAction&);

        // Material
        void operator()(const Microsoft::glTF::Material&, Microsoft::glTF::VisitState, const Microsoft::glTF::VisitDefaultAction&);

        // Texture
        void operator()(const Microsoft::glTF::Texture&, Microsoft::glTF::TextureType, Microsoft::glTF::VisitState, const Microsoft::glTF::VisitDefaultAction&);

        // Image
        void operator()(const Microsoft::glTF::Image&, Microsoft::glTF::VisitState, const Microsoft::glTF::VisitDefaultAction&);

        // Sampler
        void operator()(const Microsoft::glTF::Sampler&, Microsoft::glTF::VisitState, const Microsoft::glTF::VisitDefaultAction&);

        // Skin
        void operator()(const Microsoft::glTF::Skin&, Microsoft::glTF::VisitState, const Microsoft::glTF::VisitDefaultAction&);

        // Camera
        void operator()(const Microsoft::glTF::Camera&, Microsoft::glTF::VisitState, const Microsoft::glTF::VisitDefaultAction&);

        HRESULT EnsureGraphicsDevice();


        winrt::Windows::UI::Composition::CompositionMipmapSurface EnsureMipMapSurfaceId(
            const std::string id,
            winrt::Windows::Graphics::SizeInt32 size,
            winrt::Windows::Graphics::DirectX::DirectXPixelFormat pixelFormat,
            winrt::Windows::Graphics::DirectX::DirectXAlphaMode alphaMode);

    private:
        winrt::Windows::UI::Composition::Compositor m_compositor{ nullptr };

        winrt::com_ptr<ABI::Windows::UI::Composition::ICompositionGraphicsDevice> m_graphicsDevice{ nullptr };

        // The SceneNode connected to the SceneVisual
        winrt::Windows::UI::Composition::Scenes::SceneNode m_rootSceneNode{ nullptr };

        // Only keeps track of the equivalent SceneNodes from the DOM into the Scenes API.
        winrt::Windows::Foundation::Collections::IMap<winrt::hstring, winrt::Windows::UI::Composition::Scenes::SceneNode> m_sceneNodeMap{ nullptr };

        // It keeps the equivalent SceneNodes from the DOM and the ones needed for adapting Mesh and MeshPrimitives into the Scenes API.
        winrt::Windows::UI::Composition::Scenes::SceneNode m_latestSceneNode{ nullptr };

        Microsoft::glTF::Document& m_gltfDocument;

        Microsoft::glTF::Scene& m_gltfScene;

        std::shared_ptr<Microsoft::glTF::GLTFResourceReader> m_gltfResourceReader;
        std::shared_ptr<SceneResourceSet> m_resourceSet;
    };
} // SceneLoader