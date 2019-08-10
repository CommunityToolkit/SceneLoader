// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI.Composition;

namespace CameraComponent
{
    /// <summary>
    /// An interface that defines functions non-Composition objects can implement to become animatable.
    /// </summary>
    public interface Animatable
    {
        /// <summary>
        /// Returns an object's set of animatable properties.
        /// </summary>
        CompositionPropertySet GetPropertySet();

        /// <summary>
        /// Starts a given animation on the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property to be animated.</param>
        /// <param name="animation">The animation being applied.</param>
        void StartAnimation(string propertyName, CompositionAnimation animation);

        /// <summary>
        /// Stops any animations on the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property whose animations we are stopping.</param>
        void StopAnimation(string propertyName);
    }
}
