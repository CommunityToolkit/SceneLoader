using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.UI.Composition.Scenes;
using Windows.UI.Xaml;

namespace TestViewer
{
    public enum Stretch { None, Fill, FixX, FixY, Uniform, UniformToFill };

    public class Viewport
    { 
        private SceneVisual sceneVisual;
        private Vector2 size;
        private Vector3 offset;
        private Stretch stretch;
        private Camera camera;

        public Viewport(SceneVisual visual)
        {
            sceneVisual = visual;
            var windowSize = Window.Current.Bounds;
            size = new Vector2((float) windowSize.Width, (float) windowSize.Height);
            offset = 0.5f * new Vector3(size, 0.0f);
            sceneVisual.Offset = offset;

            stretch = Stretch.Uniform;
        }

        public SceneVisual Visual
        {
            get => sceneVisual;
            set
            {
                sceneVisual = value;
                CreateCameraMatrix();
            }
        }
        public Vector2 Size
        {
            get => size;
            set
            {
                size = value;
                Offset = 0.5f * new Vector3(size, 0.0f);
                CreateCameraMatrix();
            }
        }
        public Vector3 Offset
        {
            get => offset;
            set
            {
                offset = value;
                sceneVisual.Offset = offset;
            }
        }
        public Stretch Stretch
        {
            get => stretch;
            set
            {
                stretch = value;
                CreateCameraMatrix();
            }
        }
        public Camera Camera
        {
            get => camera;
            set
            {
                camera = value;
                CreateCameraMatrix();
                camera.PropertyChanged += Camera_PropertyChanged;
                Camera.Projection.PropertyChanged += Camera_PropertyChanged;
            }
        }        

        public Matrix4x4 CreateCameraMatrix()
        {
            Matrix4x4 camMat = camera.CreateTransformationMatrix() * camera.Projection.CreateProjectionMatrix(stretch, size);
            if(sceneVisual != null)
            {
                sceneVisual.TransformMatrix = camMat;
            }

            return camMat;
        }

        private void Camera_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.CreateCameraMatrix();
        }
    }
}
