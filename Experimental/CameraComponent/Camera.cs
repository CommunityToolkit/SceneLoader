// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;

namespace CameraComponent
{
    public interface Camera : Animatable
    {
        // A reference to either a perspective or an orthographic projection profile
        Projection Projection { get; set; }
        // returns the matrix created from the camera's position and rotation
        Matrix4x4 GetViewMatrix();
        // Returns the product of the camera's view matrix and the projection matrix
        Matrix4x4 GetModelViewProjectionMatrix();
    }
}