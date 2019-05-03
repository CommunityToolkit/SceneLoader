// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "pch.h"

#include "UtilForIntermingledNamespaces.h"
#include "GLTFVisitor.h"

using namespace std;
using namespace Microsoft::glTF;

namespace winrt {
    using namespace Windows::Foundation::Numerics;
    using namespace Windows::Graphics::DirectX;
    using namespace Windows::UI::Composition::Scenes;
}
using namespace winrt;

extern vector<uint8_t> s_binExportVector;

namespace SceneLoader
{
    // Mesh Primitive
    void GLTFVisitor::operator()(const MeshPrimitive& meshPrimitive, VisitState state, const VisitDefaultAction&) // FIXME: It is creating necessary mesh primitives (engine.gltf)
    {
        if (state == VisitState::New)
        {
            static uint16_t sCounter = 0;

            auto sceneNodeForTheGLTFMeshPrimitive = SceneNode::Create(m_compositor);

            // m_latestSceneNode is sceneNodeForTheGLTFMesh
            m_latestSceneNode.Children().Append(sceneNodeForTheGLTFMeshPrimitive);


            // We want all MeshPrimitives of a Mesh to be siblings.
            // That's why we don't define m_latestSceneNode as sceneNodeForTheGLTFMeshPrimitive.

            auto curMaterial = m_resourceSet->EnsureMaterialById(meshPrimitive.materialId);

            auto mesh = SceneMesh::Create(m_compositor);

            if (meshPrimitive.mode == MESH_TRIANGLES)
            {
                mesh.PrimitiveTopology(DirectXPrimitiveTopology::TriangleList);
            }
            else
            {
                assert(false);
            }

            for (auto value : meshPrimitive.attributes)
            {
                if (value.first == ACCESSOR_POSITION)
                {
                    auto accessorId = meshPrimitive.GetAttributeAccessorId(value.first);
                    auto& accessor = m_gltfDocument.accessors[accessorId];

                    auto data = MeshPrimitiveUtils::GetPositions(m_gltfDocument, *m_gltfResourceReader, accessor);

                    mesh.FillMeshAttribute(
                        SceneAttributeSemantic::Vertex,
                        DirectXPixelFormat::R32G32B32Float,
                        CopyArrayOfBytesToMemoryBuffer((BYTE*)data.data(), data.size() * sizeof(float)));
                }
                else if (value.first == ACCESSOR_NORMAL)
                {
                    auto accessorId = meshPrimitive.GetAttributeAccessorId(value.first);
                    auto& accessor = m_gltfDocument.accessors[accessorId];
                    auto data = MeshPrimitiveUtils::GetNormals(m_gltfDocument, *m_gltfResourceReader, accessor);

                    mesh.FillMeshAttribute(
                        SceneAttributeSemantic::Normal,
                        DirectXPixelFormat::R32G32B32Float,
                        CopyArrayOfBytesToMemoryBuffer((BYTE*)data.data(), data.size() * sizeof(float)));
                }
                else if (value.first == ACCESSOR_TANGENT)
                {
                    auto accessorId = meshPrimitive.GetAttributeAccessorId(value.first);
                    auto& accessor = m_gltfDocument.accessors[accessorId];
                    auto data = MeshPrimitiveUtils::GetTangents(m_gltfDocument, *m_gltfResourceReader, accessor);

                    mesh.FillMeshAttribute(
                        SceneAttributeSemantic::Tangent,
                        DirectXPixelFormat::R32G32B32A32Float,
                        CopyArrayOfBytesToMemoryBuffer((BYTE*)data.data(), data.size() * sizeof(float)));
                }
                else if ((value.first == ACCESSOR_TEXCOORD_0) || (value.first == ACCESSOR_TEXCOORD_1))
                {
                    auto accessorId = meshPrimitive.GetAttributeAccessorId(value.first);
                    auto& accessor = m_gltfDocument.accessors[accessorId];
                    auto data = MeshPrimitiveUtils::GetTexCoords(m_gltfDocument, *m_gltfResourceReader, accessor);

                    mesh.FillMeshAttribute(
                        (value.first == ACCESSOR_TEXCOORD_0) ? SceneAttributeSemantic::TexCoord0 : SceneAttributeSemantic::TexCoord1,
                        DirectXPixelFormat::R32G32Float,
                        CopyArrayOfBytesToMemoryBuffer((BYTE*)data.data(), data.size() * sizeof(float)));
                }
                else if (value.first == ACCESSOR_COLOR_0)
                {
                    auto accessorId = meshPrimitive.GetAttributeAccessorId(value.first);
                    auto& accessor = m_gltfDocument.accessors[accessorId];
                    auto data = MeshPrimitiveUtils::GetColors(m_gltfDocument, *m_gltfResourceReader, accessor);

                    mesh.FillMeshAttribute(
                        SceneAttributeSemantic::Color,
                        DirectXPixelFormat::R32UInt,
                        CopyArrayOfBytesToMemoryBuffer((BYTE*)data.data(), data.size() * sizeof(uint32_t)));
                }
            } // for attributes

            auto indices = MeshPrimitiveUtils::GetTriangulatedIndices16(m_gltfDocument, *m_gltfResourceReader, meshPrimitive);

            mesh.FillMeshAttribute(
                SceneAttributeSemantic::Index,
                DirectXPixelFormat::R16UInt,
                CopyArrayOfBytesToMemoryBuffer((BYTE*)indices.data(), indices.size() * sizeof(uint16_t)));

            //
            // Creates SceneRendererComponent, attaches MeshRenderer and add as component of the SceneNode
            //
            auto renderComponent = SceneMeshRendererComponent::Create(m_compositor);

            renderComponent.Mesh(mesh);

            renderComponent.Material(curMaterial);

            sceneNodeForTheGLTFMeshPrimitive.Components().Append(renderComponent);

            sCounter++;
            m_resourceSet->SetLatestMeshRendererComponent(renderComponent);
        }
    }
}