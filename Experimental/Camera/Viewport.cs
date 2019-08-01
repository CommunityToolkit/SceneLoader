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

namespace Experimental
{
    public enum Stretch { None, Fill, FixX, FixY, Uniform, UniformToFill };

    public sealed class Viewport
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
            size = new Vector2((float)windowSize.Width, (float)windowSize.Height);
            offset = 0.5f * new Vector3(size, 0.0f);
            sceneVisual.Offset = offset;

            stretch = Stretch.Fill;
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
            Matrix4x4 camMat = camera.CreateViewMatrix() * camera.Projection.CreateNormalizingMatrix() * this.CreateStretchMatrix();
            if (sceneVisual != null)
            {
                sceneVisual.TransformMatrix = camMat;
            }

            return camMat;
        }
        public Matrix4x4 CreateStretchMatrix()
        {
            bool stretchX;
            Matrix4x4 matStretch = Matrix4x4.Identity;

            switch (stretch)
            {
                case Stretch.None:
                    // Do nothing
                    break;
                case Stretch.Fill:
                    matStretch.M11 = size.X;
                    matStretch.M22 = size.Y;
                    matStretch.M33 = (size.X + size.Y) / 2f;
                    break;
                case Stretch.FixX:
                    stretchX = true;
                    matStretch = StretchMatrix(stretchX);
                    break;
                case Stretch.FixY:
                    stretchX = false;
                    matStretch = StretchMatrix(stretchX);
                    break;
                case Stretch.Uniform:
                    // wide
                    if (size.X >= size.Y)
                    {
                        stretchX = false;
                        matStretch = StretchMatrix(stretchX);
                    }
                    // long
                    else
                    {
                        stretchX = true;
                        matStretch = StretchMatrix(stretchX);
                    }
                    break;
                case Stretch.UniformToFill:
                    // wide
                    if (size.X >= size.Y)
                    {
                        stretchX = true;
                        matStretch = StretchMatrix(stretchX);
                    }
                    // long
                    else
                    {
                        stretchX = false;
                        matStretch = StretchMatrix(stretchX);
                    }
                    break;
            }

            return matStretch;
        }

        private Matrix4x4 StretchMatrix(bool stretchX)
        {
            Matrix4x4 matScale = Matrix4x4.Identity;
            matScale.M11 = matScale.M22 = matScale.M33 = stretchX ? size.X : size.Y;

            return matScale;
        }


        private void Camera_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.CreateCameraMatrix();
        }
    }
}
