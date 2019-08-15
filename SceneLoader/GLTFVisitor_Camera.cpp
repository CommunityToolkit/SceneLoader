// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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