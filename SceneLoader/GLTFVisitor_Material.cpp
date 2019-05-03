// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "pch.h"

#include "GLTFVisitor.h"

using namespace Microsoft::glTF;

namespace winrt {
    using namespace Windows::UI::Composition::Scenes;
}
using namespace winrt;

namespace SceneLoader
{
    // Material
    void GLTFVisitor::operator()(const Material& material, VisitState /*alreadyVisited*/, const VisitDefaultAction&)
    {
        auto curMaterial = m_resourceSet->EnsureMaterialById(material.id);

        if (!m_resourceSet->GetGLTFMaterialById(material.id, nullptr))
        {
            m_resourceSet->StoreGLTFMaterialById(material.id, material);
        }
        else
        {
            Microsoft::glTF::Material storedMaterial;

            m_resourceSet->GetGLTFMaterialById(material.id, &storedMaterial);

            assert(storedMaterial == material);
        }
    }
}
