using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.UI.Composition;
using Windows.UI.Composition.Scenes;
using Windows.UI.Xaml;

namespace CameraComponent
{
    public enum Stretch { Fill, FixX, FixY, Uniform, UniformToFill };

    public sealed class Viewport
    {
        private SceneVisual sceneVisual;
        private Vector2 size;
        private Vector3 offset;
        private Stretch stretch;
        private Camera camera;

        private CompositionPropertySet propertySet;

        public Viewport(SceneVisual visual)
        {
            sceneVisual = visual;
            var windowSize = Window.Current.Bounds;
            size = new Vector2((float)windowSize.Width, (float)windowSize.Height);
            offset = 0.5f * new Vector3(size, 0.0f);
            offset = Vector3.Zero;

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
            get
            {
                if (camera.UseAnimations)
                {
                    Vector2 curr = Vector2.Zero;
                    return (propertySet.TryGetVector2("Size", out curr) == CompositionGetValueStatus.Succeeded) ? curr : size;
                }
                else
                {
                    return size;
                }
            }
            set
            {
                if(camera.UseAnimations)
                {
                    propertySet.InsertVector2("Size", value);
                }
                else
                {
                    size = value;
                    CreateCameraMatrix();
                }
            }
        }
        public Vector3 Offset
        {
            get
            {
                if(camera.UseAnimations)
                {
                    Vector3 curr = Vector3.Zero;
                    return (propertySet.TryGetVector3("Offset", out curr) == CompositionGetValueStatus.Succeeded) ? curr : offset;
                }
                else
                {
                    return offset;
                }
               
            }
            set
            {
                if (camera.UseAnimations)
                {
                    propertySet.InsertVector3("Offset", value);
                }
                else
                {

                    offset = value;
                    sceneVisual.Offset = offset; // TODO: I dont know what to do here
                }
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
                camera.PropertyChanged += Camera_PropertyChanged;
                Camera.Projection.PropertyChanged += Camera_PropertyChanged;

                if (camera.UseAnimations)
                {
                    CreateExpressionAnimation();
                }
                else
                {
                    CreateCameraMatrix();
                }
            }
        }

        public Matrix4x4 CreateCameraMatrix()
        {
            Matrix4x4 camMat = camera.CreateViewMatrix() * camera.Projection.CreateNormalizingMatrix() * this.CreateStretchMatrix() * Matrix4x4.CreateTranslation(offset + new Vector3(size / 2, 0));
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

        public void CreateExpressionAnimation()
        {
            propertySet.InsertVector3("Offset", Offset);
            propertySet.InsertVector2("Size", Size);
            propertySet.InsertMatrix4x4("ViewMatrix", camera.CreateViewMatrix());
            propertySet.InsertMatrix4x4("NormalizingMatrix", camera.Projection.CreateNormalizingMatrix());
            propertySet.InsertMatrix4x4("StretchMatrix", this.CreateStretchMatrix());

            camera.Projection.CreateExpressionAnimation(propertySet, "NormalizingMatrix");
            camera.CreateExpressionAnimation(propertySet, "ViewMatrix");

            var cameraMatExpression = Window.Current.Compositor.CreateExpressionAnimation();
            cameraMatExpression.Expression = "Viewport.ViewMatrix * Viewport.NormalizingMatrix * Viewport.StretchMatrix";
            cameraMatExpression.SetReferenceParameter("Viewport", propertySet);

            sceneVisual.StartAnimation("TransformMatrix", cameraMatExpression);
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
