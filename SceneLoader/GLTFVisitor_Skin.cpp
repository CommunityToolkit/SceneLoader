// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "pch.h"

#include "GLTFVisitor.h"

using namespace Microsoft::glTF;

using namespace winrt;

namespace SceneLoader
{
    // Skin
    void GLTFVisitor::operator()(const Skin& /*skin*/, VisitState /*alreadyVisited*/, const VisitDefaultAction&)
    {
    }



}