// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "pch.h"

#include "GLTFVisitor.h"

using namespace Microsoft::glTF;

using namespace winrt;

namespace SceneLoader
{
    void GLTFVisitor::operator()(const Sampler& sampler, VisitState state, const VisitDefaultAction&)
    {
        if (state == VisitState::New)
        {
            if (!m_resourceSet->GetGLTFSamplerById(sampler.id, nullptr))
            {
                m_resourceSet->StoreGLTFSamplerById(sampler.id, sampler);
            }
            else
            {
                Microsoft::glTF::Sampler storedSampler;

                m_resourceSet->GetGLTFSamplerById(sampler.id, &storedSampler);

                assert(storedSampler == sampler);
            }
        }
    }
}