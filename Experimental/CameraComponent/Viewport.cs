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
            //offset = 0.5f * new Vector3(size, 0.0f);
            offset = Vector3.Zero;

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
                    CreateCameraMatrix();
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
            Matrix4x4 camMat = camera.CreateViewMatrix() * camera.Projection.CreateNormalizingMatrix() * this.CreateStretchMatrix() 
                * Matrix4x4.CreateTranslation(offset + new Vector3(size / 2, 0));
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
            propertySet = Window.Current.Compositor.CreatePropertySet();
            propertySet.InsertVector3("Offset", offset);
            propertySet.InsertVector2("Size", size);
            propertySet.InsertScalar("Stretch", (float)stretch);
            propertySet.InsertMatrix4x4("ViewMatrix", camera.CreateViewMatrix());
            propertySet.InsertMatrix4x4("NormalizingMatrix", camera.Projection.CreateNormalizingMatrix());
            propertySet.InsertMatrix4x4("StretchMatrix", this.CreateStretchMatrix());

            camera.Projection.CreateExpressionAnimation(propertySet, "NormalizingMatrix");
            camera.CreateExpressionAnimation(propertySet, "ViewMatrix");
            propertySet.StartAnimation("StretchMatrix", this.CreateStretchExpressionMatrix());

            var cameraMatExpression = Window.Current.Compositor.CreateExpressionAnimation();
            cameraMatExpression.Expression = "Viewport.ViewMatrix * Viewport.NormalizingMatrix * Viewport.StretchMatrix " +
                "* Matrix4x4.CreateTranslation(Vector3(Viewport.Offset.X + Viewport.Size.X / 2f, Viewport.Offset.Y + Viewport.Size.Y / 2f, Viewport.Offset.Z))";
            cameraMatExpression.SetReferenceParameter("Viewport", propertySet);

            sceneVisual.StartAnimation("TransformMatrix", cameraMatExpression);
        }

        private Matrix4x4 StretchMatrix(bool stretchX)
        {
            Matrix4x4 matScale = Matrix4x4.Identity;
            matScale.M11 = matScale.M22 = matScale.M33 = stretchX ? size.X : size.Y;

            return matScale;
        }
        private ExpressionAnimation CreateStretchExpressionMatrix()
        {
            string stretchMat;

            float stretch;
            propertySet.TryGetScalar("Stretch", out stretch);

            stretchMat = "(Viewport.Stretch == 0f)?" +
                            "Matrix4x4(" +
                            "Viewport.Size.X, 0, 0, 0, " +
                            "0, Viewport.Size.Y, 0, 0, " +
                            "0, 0, (Viewport.Size.X + Viewport.Size.Y) / 2f, 0, " +
                            "0, 0, 0, 1) " +
                        ":" +
                        "(Viewport.Stretch == 1f)? " +
                            "Matrix4x4(" +
                            "Viewport.Size.X, 0, 0, 0, " +
                            "0, Viewport.Size.X, 0, 0, " +
                            "0, 0, Viewport.Size.X, 0, " +
                            "0, 0, 0, 1)" +
                        ":" +
                        "(Viewport.Stretch == 2f)? " + 
                            "Matrix4x4(" +
                            "Viewport.Size.Y, 0, 0, 0, " +
                            "0, Viewport.Size.Y, 0, 0, " +
                            "0, 0, Viewport.Size.Y, 0, " +
                            "0, 0, 0, 1)" + 
                        ":" +
                        "(Viewport.Stretch == 3f)? " +
                            "(Viewport.Size.X >= Viewport.Size.Y)? " +
                                "Matrix4x4(" +
                                "Viewport.Size.Y, 0, 0, 0, " +
                                "0, Viewport.Size.Y, 0, 0, " +
                                "0, 0, Viewport.Size.Y, 0, " +
                                "0, 0, 0, 1) " +
                                ":" +
                                "Matrix4x4(" +
                                "Viewport.Size.X, 0, 0, 0, " +
                                "0, Viewport.Size.X, 0, 0, " +
                                "0, 0, Viewport.Size.X, 0, " +
                                "0, 0, 0, 1)" +
                        ":" +
                            "(Viewport.Size.X < Viewport.Size.Y)? " +
                                "Matrix4x4(" +
                                "Viewport.Size.Y, 0, 0, 0, " +
                                "0, Viewport.Size.Y, 0, 0, " +
                                "0, 0, Viewport.Size.Y, 0, " +
                                "0, 0, 0, 1) " +
                                ":" +
                                "Matrix4x4(" +
                                "Viewport.Size.X, 0, 0, 0, " +
                                "0, Viewport.Size.X, 0, 0, " +
                                "0, 0, Viewport.Size.X, 0, " +
                                "0, 0, 0, 1)";

            //switch (stretch)
            //{
            //    case 0f: // Fill
            //        stretchMat = "Matrix4x4(" +
            //            "Viewport.Size.X, 0, 0, 0, " +
            //            "0, Viewport.Size.Y, 0, 0, " +
            //            "0, 0, (Viewport.Size.X + Viewport.Size.Y) / 2f, 0, " +
            //            "0, 0, 0, 1)";
            //        break;
            //    case 1f: // FixX
            //        stretchMat = "Matrix4x4(" +
            //            "Viewport.Size.X, 0, 0, 0, " +
            //            "0, Viewport.Size.X, 0, 0, " +
            //            "0, 0, Viewport.Size.X, 0, " +
            //            "0, 0, 0, 1)";
            //        break;
            //    case 2f: // FixY
            //        stretchMat = "Matrix4x4(" +
            //            "Viewport.Size.Y, 0, 0, 0, " +
            //            "0, Viewport.Size.Y, 0, 0, " +
            //            "0, 0, Viewport.Size.Y, 0, " +
            //            "0, 0, 0, 1)";
            //        break; 
            //    case 3f:
            //        Vector2 uniform_size;
            //        propertySet.TryGetVector2("Size", out uniform_size);

            //        if (uniform_size.X >= uniform_size.Y)
            //        {
            //            stretchMat = "Matrix4x4(" +
            //            "Viewport.Size.Y, 0, 0, 0, " +
            //            "0, Viewport.Size.Y, 0, 0, " +
            //            "0, 0, Viewport.Size.Y, 0, " +
            //            "0, 0, 0, 1)";
            //        }
            //        else
            //        {
            //            stretchMat = "Matrix4x4(" +
            //            "Viewport.Size.X, 0, 0, 0, " +
            //            "0, Viewport.Size.X, 0, 0, " +
            //            "0, 0, Viewport.Size.X, 0, " +
            //            "0, 0, 0, 1)";
            //        }
            //        break;
            //    case 4f:
            //        Vector2 uniform_to_fill_size;
            //        propertySet.TryGetVector2("Size", out uniform_to_fill_size);

            //        if (size.X >= size.Y)
            //        {
            //            stretchMat = "Matrix4x4(" +
            //            "Viewport.Size.X, 0, 0, 0, " +
            //            "0, Viewport.Size.X, 0, 0, " +
            //            "0, 0, Viewport.Size.X, 0, " +
            //            "0, 0, 0, 1)";
            //        }
            //        else
            //        {
            //            stretchMat = "Matrix4x4(" +
            //            "Viewport.Size.Y, 0, 0, 0, " +
            //            "0, Viewport.Size.Y, 0, 0, " +
            //            "0, 0, Viewport.Size.Y, 0, " +
            //            "0, 0, 0, 1)";
            //        }
            //        break;
            //    default:
            //        stretchMat = "";
            //        break;
            //}
            
            var stretchExpression = Window.Current.Compositor.CreateExpressionAnimation();
            stretchExpression.Expression = stretchMat;
            stretchExpression.SetReferenceParameter("Viewport", propertySet);

            return stretchExpression;
        }

        private void Camera_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(!Camera.UseAnimations)
            {
                this.CreateCameraMatrix();
            }
            else
            {
                this.CreateExpressionAnimation();
            }
        }
    }
}
