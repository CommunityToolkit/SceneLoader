using Windows.UI.Composition;

namespace CameraComponent
{
    public interface Camera
    {
        Projection Projection { get; set; }
        CompositionPropertySet GetPropertySet();
        void StartAnimation(string propertyName, CompositionAnimation animation);
        void StopAnimation(string propertyName);
    }
}