// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "pch.h"

#include "GLTFVisitor.h"

using namespace Microsoft::glTF;

using namespace winrt;

namespace SceneLoader
{
    void GLTFVisitor::operator()(const Texture& texture, TextureType /*textureType*/, VisitState state, const VisitDefaultAction&)
    {
        if (state == VisitState::New)
        {
            if (!m_resourceSet->GetGLTFTextureById(texture.id, nullptr))
            {
                m_resourceSet->StoreGLTFTextureById(texture.id, texture);
            }
            else
            {
                Microsoft::glTF::Texture storedTexture;

                m_resourceSet->GetGLTFTextureById(texture.id, &storedTexture);

                assert(storedTexture == texture);
            }
        }
    }
}