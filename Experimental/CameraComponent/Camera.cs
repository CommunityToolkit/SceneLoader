// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;

namespace CameraComponent
{
    /// <summary>
    /// An interface that defines properties and functions that a Camera object must implement.
    /// </summary>
    public interface Camera : Animatable
    {
        /// <summary>
        /// A reference to a class that implements the Projection interace.
        /// </summary>
        Projection Projection { get; set; }

        /// <summary>
        /// Returns the matrix created using the Camera's position and rotation in world space.
        /// </summary>
        /// /// <returns>A Matrix4x4 that is the product of the matrices representing the camera's translation and rotation.</returns>
        Matrix4x4 GetViewMatrix();
        
        /// <summary>
        /// Returns the matrix created using the Camera's view matrix and the Camera's Projection's matrix.
        /// </summary>
        /// <returns>A Matrix4x4 that is the the product of the matrices representing the camera's transformations in world space and
        /// the matrix created by the Camera's Projection property.</returns>
        Matrix4x4 GetModelViewProjectionMatrix();
    }
}