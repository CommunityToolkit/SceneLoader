// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "pch.h"

#include "GLTFVisitor.h"

using namespace Microsoft::glTF;

namespace winrt {
    using namespace Windows::UI::Composition;
    using namespace Windows::UI::Composition::Scenes;
}
using namespace winrt;

namespace SceneLoader
{
    // Camera
    void GLTFVisitor::operator()(const Camera& /*camera*/, VisitState /*alreadyVisited*/, const VisitDefaultAction&)
    {
    }
}