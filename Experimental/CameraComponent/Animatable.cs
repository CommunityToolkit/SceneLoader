// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI.Composition;

namespace CameraComponent
{
    public interface Animatable
    {
        // Returns the set of animatable properties
        CompositionPropertySet GetPropertySet();
        // Starts a given animation on the specified property
        void StartAnimation(string propertyName, CompositionAnimation animation);
        // Stops any animation on the specified property
        void StopAnimation(string propertyName);
    }
}
