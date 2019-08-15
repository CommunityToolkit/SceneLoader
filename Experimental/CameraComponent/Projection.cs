// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;

namespace CameraComponent
{
    /// <summary>
    /// An interface that defines properties and functions that a Projection object must implement.
    /// </summary>
    public interface Projection : Animatable
    {
        /// <summary>
        /// Distance from the camera to the near plane.
        /// </summary>
        float Near { get; set; }

        /// <summary>
        /// Distance from the camera to the far plane.
        /// </summary>
        float Far { get; set; }

        /// <summary>
        /// Returns the matrix created by the near and far planes and other properties of the projection.
        /// </summary>
        /// <returns>A Matrix4x4 created from the specific type of projection's properties.</returns>
        Matrix4x4 GetProjectionMatrix();
    }
}
