// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "pch.h"

#include "UtilForIntermingledNamespaces.h"
#include "SceneResourceSet.h"

using namespace std;
using namespace Microsoft::glTF;

namespace winrt {
    using namespace Windows::UI::Composition;
    using namespace Windows::UI::Composition::Scenes;
}
using namespace winrt;

namespace SceneLoader
{
    // If you want to assert when we hit a feature we don't support yet, change this to true.
    bool SceneResourceSet::s_assertOnUnimplementedFeature = false;

    SceneWrappingMode
    GLTFWrapModeToSceneWrapMode(Microsoft::glTF::WrapMode gltfWrapMode)
    {
        switch (gltfWrapMode)
        {
        case Microsoft::glTF::WrapMode::Wrap_CLAMP_TO_EDGE:
            return SceneWrappingMode::ClampToEdge;

        case Microsoft::glTF::WrapMode::Wrap_MIRRORED_REPEAT:
            return SceneWrappingMode::MirroredRepeat;

        case Microsoft::glTF::WrapMode::Wrap_REPEAT:
        default:
            return SceneWrappingMode::Repeat;
        }
    }


    SceneResourceSet::SceneResourceSet(winrt::Windows::UI::Composition::Compositor compositor) :
        m_compositor(compositor),
        m_sceneMaterialMap(single_threaded_map<hstring, SceneMetallicRoughnessMaterial>()),
        m_sceneSurfaceMaterialInputMap(single_threaded_map<hstring, SceneSurfaceMaterialInput>()),
        m_sceneMipMapSurfaceMap(single_threaded_map<hstring, winrt::Windows::UI::Composition::CompositionMipmapSurface>())
    {

    }


    SceneMetallicRoughnessMaterial
    SceneResourceSet::EnsureMaterialById(const std::string id)
    {
        // Should we trust that the GLTF SDK is traversing correctly the DOM?
        if (!m_sceneMaterialMap.HasKey(GetHSTRINGFromStdString(id)))
        {
            auto sceneMaterial = SceneMetallicRoughnessMaterial::Create(m_compositor);
            sceneMaterial.Comment(wstring(id.begin(), id.end()));

            m_sceneMaterialMap.Insert(GetHSTRINGFromStdString(id), sceneMaterial);
        }

        return m_sceneMaterialMap.Lookup(GetHSTRINGFromStdString(id));
    }


    CompositionMipmapSurface
    SceneResourceSet::EnsureMipMapSurfaceId(
            const std::string id,
            winrt::Windows::Graphics::SizeInt32 sizePixels,
            winrt::Windows::Graphics::DirectX::DirectXPixelFormat pixelFormat,
            winrt::Windows::Graphics::DirectX::DirectXAlphaMode alphaMode,
            winrt::Windows::UI::Composition::ICompositionGraphicsDevice3 graphicsDevice)
    {
        // Should we trust that the GLTF SDK is traversing correctly the DOM?
        if (!m_sceneMipMapSurfaceMap.HasKey(GetHSTRINGFromStdString(id)))
        {
            auto mipmapSurface = graphicsDevice.CreateMipmapSurface(
                sizePixels,
                pixelFormat,
                alphaMode);
            wstring mipmapId{ id.begin(), id.end() };
            mipmapSurface.Comment(mipmapId);

            m_sceneMipMapSurfaceMap.Insert(GetHSTRINGFromStdString(id), mipmapSurface);
        }

        return m_sceneMipMapSurfaceMap.Lookup(GetHSTRINGFromStdString(id));
    }


    CompositionMipmapSurface
    SceneResourceSet::LookupMipMapSurfaceId(const std::string id)
    {
        return m_sceneMipMapSurfaceMap.Lookup(GetHSTRINGFromStdString(id));
    }

    void
    SceneResourceSet::StoreGLTFSamplerById(const std::string id, Microsoft::glTF::Sampler sampler)
    {
        // Make sure we haven't stored this before
        assert(m_gltfSamplerMap.find(GetHSTRINGFromStdString(id)) == m_gltfSamplerMap.end());

        m_gltfSamplerMap.insert(std::map<winrt::hstring, Microsoft::glTF::Sampler>::value_type(GetHSTRINGFromStdString(id), sampler));
    }

    bool
    SceneResourceSet::GetGLTFSamplerById(const std::string id, Microsoft::glTF::Sampler* pSampler)
    {
        if (m_gltfSamplerMap.find(GetHSTRINGFromStdString(id)) == m_gltfSamplerMap.end())
        {
            return false;
        }

        if (pSampler)
        {
            *pSampler = m_gltfSamplerMap.at(GetHSTRINGFromStdString(id));
        }

        return true;
    }


    void
    SceneResourceSet::StoreGLTFMaterialById(const std::string id, Microsoft::glTF::Material material)
    {
        // Make sure we haven't stored this before
        assert(m_gltfMaterialMap.find(GetHSTRINGFromStdString(id)) == m_gltfMaterialMap.end());

        m_gltfMaterialMap.insert(std::map<winrt::hstring, Microsoft::glTF::Material>::value_type(GetHSTRINGFromStdString(id), material));
    }


    bool
    SceneResourceSet::GetGLTFMaterialById(const std::string id, Microsoft::glTF::Material* pMaterial)
    {
        if (m_gltfMaterialMap.find(GetHSTRINGFromStdString(id)) == m_gltfMaterialMap.end())
        {
            return false;
        }

        if (pMaterial)
        {
            *pMaterial = m_gltfMaterialMap.at(GetHSTRINGFromStdString(id));
        }

        return true;
    }


    void
    SceneResourceSet::StoreGLTFTextureById(const std::string id, Microsoft::glTF::Texture texture)
    {
        // Make sure we haven't stored this before
        assert(m_gltfTextureMap.find(GetHSTRINGFromStdString(id)) == m_gltfTextureMap.end());

        m_gltfTextureMap.insert(std::map<winrt::hstring, Microsoft::glTF::Texture>::value_type(GetHSTRINGFromStdString(id), texture));
    }


    bool
    SceneResourceSet::GetGLTFTextureById(const std::string id, Microsoft::glTF::Texture* pTexture)
    {
        if (m_gltfTextureMap.find(GetHSTRINGFromStdString(id)) == m_gltfTextureMap.end())
        {
            return false;
        }

        if (pTexture)
        {
            *pTexture = m_gltfTextureMap.at(GetHSTRINGFromStdString(id));
        }

        return true;
    }


    void
    SceneResourceSet::CreateSceneMaterialObjects()
    {
        for (std::map<winrt::hstring, Microsoft::glTF::Material>::iterator materialIterator = m_gltfMaterialMap.begin(); materialIterator != m_gltfMaterialMap.end(); materialIterator++)
        {
            SceneMetallicRoughnessMaterial sceneMaterial = EnsureMaterialById(materialIterator->second.id);
            Microsoft::glTF::Material material = materialIterator->second;


            // BaseColor
            if (material.metallicRoughness.baseColorTexture.textureId != "")
            {
                auto materialInput = GetMaterialInputFromTextureId(material.metallicRoughness.baseColorTexture.textureId);
                sceneMaterial.BaseColorInput(materialInput);
                
                if (m_latestMeshRendererComponent)
                {
                    m_latestMeshRendererComponent.UVMappings().Insert(L"BaseColorInput",
                        material.metallicRoughness.baseColorTexture.texCoord == 0 ? SceneAttributeSemantic::TexCoord0 : SceneAttributeSemantic::TexCoord1);
                }
            }

            sceneMaterial.BaseColorFactor({ material.metallicRoughness.baseColorFactor.r, material.metallicRoughness.baseColorFactor.g, material.metallicRoughness.baseColorFactor.b, material.metallicRoughness.baseColorFactor.a });

            // MetallicRoughness
            if (material.metallicRoughness.metallicRoughnessTexture.textureId != "")
            {
                auto materialInput = GetMaterialInputFromTextureId(material.metallicRoughness.metallicRoughnessTexture.textureId);
                sceneMaterial.MetallicRoughnessInput(materialInput);
            
                if (m_latestMeshRendererComponent)
                {
                    m_latestMeshRendererComponent.UVMappings().Insert(L"MetallicRoughnessInput",
                        material.metallicRoughness.metallicRoughnessTexture.texCoord == 0 ? SceneAttributeSemantic::TexCoord0 : SceneAttributeSemantic::TexCoord1);
                }
            }

            sceneMaterial.RoughnessFactor(material.metallicRoughness.roughnessFactor);

            sceneMaterial.MetallicFactor(material.metallicRoughness.metallicFactor);

            // Normal
            if (material.normalTexture.textureId != "")
            {
                auto materialInput = GetMaterialInputFromTextureId(material.normalTexture.textureId);
                sceneMaterial.NormalInput(materialInput);
            
                if (m_latestMeshRendererComponent)
                {
                    m_latestMeshRendererComponent.UVMappings().Insert(L"NormalInput",
                        material.normalTexture.texCoord == 0 ? SceneAttributeSemantic::TexCoord0 : SceneAttributeSemantic::TexCoord1);
                }
            }

            sceneMaterial.NormalScale(material.normalTexture.scale);

            // Occlusion
            if (material.occlusionTexture.textureId != "")
            {
                auto materialInput = GetMaterialInputFromTextureId(material.occlusionTexture.textureId);
                sceneMaterial.OcclusionInput(materialInput);
            
                if (m_latestMeshRendererComponent)
                {
                    m_latestMeshRendererComponent.UVMappings().Insert(L"OcclusionInput",
                        material.occlusionTexture.texCoord == 0 ? SceneAttributeSemantic::TexCoord0 : SceneAttributeSemantic::TexCoord1);
                }
            }

            sceneMaterial.OcclusionStrength(material.occlusionTexture.strength);

            // Emissive
            if (material.emissiveTexture.textureId != "")
            {
                auto materialInput = GetMaterialInputFromTextureId(material.emissiveTexture.textureId);
                sceneMaterial.EmissiveInput(materialInput);
            
                if (m_latestMeshRendererComponent)
                {
                    m_latestMeshRendererComponent.UVMappings().Insert(L"EmissiveInput",
                        material.emissiveTexture.texCoord == 0 ? SceneAttributeSemantic::TexCoord0 : SceneAttributeSemantic::TexCoord1);
                }
            }

            sceneMaterial.EmissiveFactor({ material.emissiveFactor.r, material.emissiveFactor.g, material.emissiveFactor.b });

            switch (material.alphaMode) {
            case AlphaMode::ALPHA_OPAQUE:
            {
                sceneMaterial.AlphaMode(SceneAlphaMode::Opaque);
                break;
            }
            case AlphaMode::ALPHA_BLEND:
            {
                sceneMaterial.AlphaMode(SceneAlphaMode::Blend);
                break;
            }
            case AlphaMode::ALPHA_MASK:
            {
                sceneMaterial.AlphaMode(SceneAlphaMode::AlphaTest);
                break;
            }
            case AlphaMode::ALPHA_UNKNOWN:
            default:
            {
                UnimplementedFeatureFound();
            }
            }

            sceneMaterial.AlphaCutoff(material.alphaCutoff);
            sceneMaterial.IsDoubleSided(material.doubleSided);
        }
    }


    SceneSurfaceMaterialInput
    SceneResourceSet::GetMaterialInputFromTextureId(const std::string textureId)
    {
        static uint16_t sCount = 0;
        Microsoft::glTF::Sampler sampler;
        Microsoft::glTF::Texture texture;

        bool resultFound;

        resultFound = GetGLTFTextureById(textureId, &texture);
        assert(resultFound);

        resultFound = GetGLTFSamplerById(texture.samplerId, &sampler);
        assert(resultFound);

        CompositionMipmapSurface mipMapSurface = LookupMipMapSurfaceId(texture.imageId);

        SceneSurfaceMaterialInput sceneSurfaceMaterialInput = SceneSurfaceMaterialInput::Create(m_compositor);
        wstringstream ssitoa; ssitoa << sCount;
        sceneSurfaceMaterialInput.Comment(ssitoa.str());

        SetSceneSampler(sceneSurfaceMaterialInput, sampler);

        sceneSurfaceMaterialInput.Surface(mipMapSurface);

        ++sCount;

        return sceneSurfaceMaterialInput;
    }


    void
    SceneResourceSet::SetSceneSampler(winrt::Windows::UI::Composition::Scenes::SceneSurfaceMaterialInput sceneSurfaceMaterialInput, Microsoft::glTF::Sampler sampler)
    {
        sceneSurfaceMaterialInput.BitmapInterpolationMode(CompositionBitmapInterpolationMode::MagLinearMinLinearMipLinear);

        sceneSurfaceMaterialInput.WrappingUMode(GLTFWrapModeToSceneWrapMode(sampler.wrapS));

        sceneSurfaceMaterialInput.WrappingVMode(GLTFWrapModeToSceneWrapMode(sampler.wrapT));
    }

    void
    SceneResourceSet::SetLatestMeshRendererComponent(SceneMeshRendererComponent& meshRendererComponent)
    {
        m_latestMeshRendererComponent = meshRendererComponent;
    }

    void
    SceneResourceSet::UnimplementedFeatureFound()
    {
        if (s_assertOnUnimplementedFeature)
        {
           assert(false);
        }
    }
    
} // namespace SceneLoader