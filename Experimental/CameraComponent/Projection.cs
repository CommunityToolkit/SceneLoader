// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;

namespace CameraComponent
{
    public interface Projection : Animatable
    {
        // Distance from the camera to the near plane
        float Near { get; set; }
        // Distance from the camera to the far plane
        float Far { get; set; }
        // Returns the matrix created by the near and far planes and other properties of the projection
        Matrix4x4 GetProjectionMatrix();
    }
}
