// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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