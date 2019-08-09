﻿using System.Numerics;
using Windows.UI.Composition;

namespace CameraComponent
{
    public interface Projection
    {
        float Near { get; set; }
        float Far { get; set; }
        Matrix4x4 GetProjectionMatrix();
        CompositionPropertySet GetPropertySet();
        void StartAnimation(string propertyName, CompositionAnimation animation);
        void StopAnimation(string propertyName);
    }
}