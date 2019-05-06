// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
