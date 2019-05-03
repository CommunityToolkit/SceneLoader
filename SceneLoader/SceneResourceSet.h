// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once

namespace SceneLoader
{
    class SceneResourceSet
    {
    public:
        SceneResourceSet(winrt::Windows::UI::Composition::Compositor compositor);

        winrt::Windows::UI::Composition::Scenes::SceneMetallicRoughnessMaterial EnsureMaterialById(const std::string id);

        void StoreGLTFSamplerById(const std::string id, Microsoft::glTF::Sampler sampler);
        bool GetGLTFSamplerById(const std::string id, Microsoft::glTF::Sampler* pSampler);

        void StoreGLTFMaterialById(const std::string id, Microsoft::glTF::Material material);
        bool GetGLTFMaterialById(const std::string id, Microsoft::glTF::Material* pMaterial);

        void StoreGLTFTextureById(const std::string id, Microsoft::glTF::Texture texture);
        bool GetGLTFTextureById(const std::string id, Microsoft::glTF::Texture* pTexture);

        void CreateSceneMaterialObjects();

        void SetSceneSampler(winrt::Windows::UI::Composition::Scenes::SceneSurfaceMaterialInput materialInput, Microsoft::glTF::Sampler sampler);

        winrt::Windows::UI::Composition::Scenes::SceneSurfaceMaterialInput GetMaterialInputFromTextureId(const std::string textureId);

        winrt::Windows::UI::Composition::CompositionMipmapSurface EnsureMipMapSurfaceId(
            const std::string id,
            winrt::Windows::Graphics::SizeInt32 size,
            winrt::Windows::Graphics::DirectX::DirectXPixelFormat pixelFormat,
            winrt::Windows::Graphics::DirectX::DirectXAlphaMode alphaMode,
            winrt::Windows::UI::Composition::ICompositionGraphicsDevice3 graphicsDevice);

        winrt::Windows::UI::Composition::CompositionMipmapSurface LookupMipMapSurfaceId(const std::string id);

        void SetLatestMeshRendererComponent(winrt::Windows::UI::Composition::Scenes::SceneMeshRendererComponent& meshRendererComponent);

    private:
        winrt::Windows::UI::Composition::Compositor m_compositor;

        // Only keeps track of the equivalent Mipmap from the DOM into the Scenes API.
        winrt::Windows::Foundation::Collections::IMap<winrt::hstring, winrt::Windows::UI::Composition::Scenes::SceneSurfaceMaterialInput> m_sceneSurfaceMaterialInputMap{ nullptr };

        // Only keeps track of the equivalent Mipmap from the DOM into the Scenes API.
        winrt::Windows::Foundation::Collections::IMap<winrt::hstring, winrt::Windows::UI::Composition::CompositionMipmapSurface> m_sceneMipMapSurfaceMap{ nullptr };

        // Only keeps track of the equivalent Materials from the DOM into the Scenes API.
        winrt::Windows::Foundation::Collections::IMap<winrt::hstring, winrt::Windows::UI::Composition::Scenes::SceneMetallicRoughnessMaterial> m_sceneMaterialMap{ nullptr };

        // Only keeps track of the equivalent Materials from the DOM into the Scenes API.
        std::map<winrt::hstring, Microsoft::glTF::Sampler> m_gltfSamplerMap;
        std::map<winrt::hstring, Microsoft::glTF::Material> m_gltfMaterialMap;
        std::map<winrt::hstring, Microsoft::glTF::Texture> m_gltfTextureMap;
    
        winrt::Windows::UI::Composition::Scenes::SceneMeshRendererComponent m_latestMeshRendererComponent{ nullptr };
    };
} // SceneLoader