using System.Numerics;
using Windows.UI.Composition;

namespace CameraComponent
{
    public interface Camera
    {
        Projection Projection { get; set; }
        Matrix4x4 GetViewMatrix();
        Matrix4x4 GetModelViewProjectionMatrix();
        CompositionPropertySet GetPropertySet();
        void StartAnimation(string propertyName, CompositionAnimation animation);
        void StopAnimation(string propertyName);
    }
}