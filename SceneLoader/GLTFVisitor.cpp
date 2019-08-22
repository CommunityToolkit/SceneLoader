// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "pch.h"

#include "UtilForIntermingledNamespaces.h"
#include "GLTFVisitor.h"

using namespace std;
using namespace Microsoft::glTF;

namespace winrt {
    using namespace Windows::UI::Composition;
    using namespace Windows::UI::Composition::Scenes;
}
using namespace winrt;

extern std::wstringstream s_export;

namespace SceneLoader
{
    GLTFVisitor::GLTFVisitor(Compositor compositor,
        SceneNode rootSceneNode,
        shared_ptr<SceneResourceSet> resourceSet,
        shared_ptr<GLTFResourceReader> gltfResourceReader,
        Document& gltfDocument,
        Scene& gltfScene) :
        m_compositor(compositor),
        m_rootSceneNode(rootSceneNode),
        m_sceneNodeMap(single_threaded_map<hstring, SceneNode>()),
        m_resourceSet(resourceSet),
        m_gltfResourceReader(gltfResourceReader),
        m_gltfDocument(gltfDocument),
        m_gltfScene(gltfScene)
    {
    }

    HRESULT GLTFVisitor::EnsureGraphicsDevice()
    {
        HRESULT hr = S_OK;

        if (!m_graphicsDevice)
        {
            // Initialize DX
            winrt::com_ptr<ID3D11Device> cpDevice;
            winrt::com_ptr<ID3D11DeviceContext> cpContext;
            UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
            D3D_FEATURE_LEVEL featureLevels[] =
            {
                D3D_FEATURE_LEVEL_11_1,
                D3D_FEATURE_LEVEL_11_0,
                D3D_FEATURE_LEVEL_10_1,
                D3D_FEATURE_LEVEL_10_0,
                D3D_FEATURE_LEVEL_9_3,
                D3D_FEATURE_LEVEL_9_2,
                D3D_FEATURE_LEVEL_9_1
            };
            D3D_FEATURE_LEVEL usedFeatureLevel;

            hr = D3D11CreateDevice(
                nullptr,
                D3D_DRIVER_TYPE_HARDWARE,
                nullptr,
                creationFlags,
                featureLevels,
                ARRAYSIZE(featureLevels),
                D3D11_SDK_VERSION,
                cpDevice.put(),
                &usedFeatureLevel,
                cpContext.put());

            winrt::com_ptr<ID2D1Factory1> cpD2DFactory;
            winrt::com_ptr<ID2D1Device> cpD2D1Device;
            winrt::com_ptr<ID3D11Device1> cpd3dDevice = cpDevice.as<ID3D11Device1>();
            hr = D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, __uuidof(ID2D1Factory1), cpD2DFactory.put_void());
            winrt::com_ptr<IDXGIDevice> cpDxgiDevice = cpd3dDevice.as<IDXGIDevice>();
            cpD2DFactory->CreateDevice(cpDxgiDevice.get(), cpD2D1Device.put());

            winrt::com_ptr<ABI::Windows::UI::Composition::ICompositorInterop> cpCompositorInterop = m_compositor.as< ABI::Windows::UI::Composition::ICompositorInterop>();
            cpCompositorInterop->CreateGraphicsDevice(/*cpDevice*/cpD2D1Device.get(), m_graphicsDevice.put());

            assert(m_graphicsDevice);
        }

        return hr;
    }

    winrt::Windows::UI::Composition::CompositionMipmapSurface
    GLTFVisitor::EnsureMipMapSurfaceId(
        const std::string id,
        winrt::Windows::Graphics::SizeInt32 sizePixels,
        winrt::Windows::Graphics::DirectX::DirectXPixelFormat pixelFormat,
        winrt::Windows::Graphics::DirectX::DirectXAlphaMode alphaMode)
    {
        EnsureGraphicsDevice();

        winrt::Windows::UI::Composition::ICompositionGraphicsDevice3 cpGraphicsDevice3 = m_graphicsDevice.as< winrt::Windows::UI::Composition::ICompositionGraphicsDevice3>();

        return m_resourceSet->EnsureMipMapSurfaceId(
            id,
            sizePixels,
            pixelFormat,
            alphaMode,
            cpGraphicsDevice3
            );
    }


} // SceneLoader
