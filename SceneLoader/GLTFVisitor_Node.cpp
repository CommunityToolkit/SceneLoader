// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "pch.h"

#include "GLTFVisitor.h"
#include "UtilForIntermingledNamespaces.h"
#include "Bounds3D.h"

using namespace Microsoft::glTF;

namespace winrt {
    using namespace Windows::UI::Composition::Scenes;
    using namespace Windows::Foundation::Numerics;
}
using namespace winrt;

using namespace std;

namespace SceneLoader
{
    // Node
    void GLTFVisitor::operator()(const Node& node, const Node* nodeParent)
    {
        wstring nodeID{ node.id.begin(), node.id.end() };

        auto sceneNode = SceneNode::Create(m_compositor);
        sceneNode.Comment(nodeID);

        m_latestSceneNode = sceneNode;
        m_sceneNodeMap.Insert(GetHSTRINGFromStdString(node.id), sceneNode);

        if (!nodeParent)
        {
            m_rootSceneNode.Children().Append(sceneNode);
        }
        else
        {
            m_sceneNodeMap.Lookup(GetHSTRINGFromStdString(nodeParent->id)).Children().Append(sceneNode);
        }

        switch (node.GetTransformationType())
        {
        case TRANSFORMATION_MATRIX:
            float3 scale;
            quaternion rotation;
            float3 translation;

            DecomposeMatrix(
                node.matrix.values,
                &scale,
                &rotation,
                &translation);

            sceneNode.Transform().Scale(scale);
            sceneNode.Transform().Translation(translation);
            sceneNode.Transform().Orientation(rotation);
            break;

        case TRANSFORMATION_TRS:
            sceneNode.Transform().Scale({ node.scale.x, node.scale.y, node.scale.z });
            sceneNode.Transform().Orientation({ node.rotation.x, node.rotation.y, node.rotation.z, node.rotation.w });
            sceneNode.Transform().Translation({ node.translation.x, node.translation.y, node.translation.z });
            break;

        case TRANSFORMATION_IDENTITY:
        default:
            // Move along. Nothing to see here.
            break;
        }
    }
} // SceneLoader
