// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "pch.h"

#include "GLTFVisitor.h"

using namespace Microsoft::glTF;

namespace winrt {
    using namespace Windows::UI::Composition::Scenes;
}
using namespace winrt;
using namespace std;

namespace SceneLoader
{
    // Mesh
    void GLTFVisitor::operator()(const Mesh& mesh, VisitState state)
    {
        if (state == VisitState::New)
        {
            wstring meshID{ mesh.id.begin(), mesh.id.end() };

            // We'll have a new SceneNode for each GLTF Mesh and another for each GLTF MeshComponent.
            auto sceneNodeForTheGLTFMesh = SceneNode::Create(m_compositor);
            sceneNodeForTheGLTFMesh.Comment(meshID);

            m_latestSceneNode.Children().Append(sceneNodeForTheGLTFMesh);

            m_latestSceneNode = sceneNodeForTheGLTFMesh;
        }
    }
}