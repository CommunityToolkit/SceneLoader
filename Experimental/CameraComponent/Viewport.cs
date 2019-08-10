// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;
using Windows.UI.Composition;

namespace CameraComponent
{
    /// <summary>
    /// Determines how the image is stretched to the viewport.
    /// </summary>
    /// <remarks>
    /// Fill: Stretch the image to the size of the viewport so the same amount of the image is always shown.
    /// FixX: Stretch the image to the width of the screen so the same amount of horizontal space in the scene is always shown.
    /// FixY: Stretch the image to the height of the screen so the same amount of vertical space in the scene is always shown.
    /// Uniform: If the screen is wide stretch to the height of the screen, if the screen is tall stretch to the width of the screen.
    /// UniformToFill: If the screen is tall stretch to the height of the screen, if the screen is wide stretch to the width of the screen.
    /// </remarks>
    public enum Stretch { Fill, FixX, FixY, Uniform, UniformToFill};

    /// <summary>
    /// Manages how a the content that is in view of a 3D camera is placed and stretched on the screen.
    /// Implements the Animatable interface.
    /// </summary>
    public sealed class Viewport : Animatable
    {
        private Visual _visual;
        private Compositor _compositor;
        private Camera _camera;
        private CompositionPropertySet _propertySet;

        /// <summary>
        /// Creates a Viewport with default properties.
        /// Offset = Vector3.Zero
        /// Size = Vector2(100, 100)
        /// Stretch = Uniform
        /// StretchMatrix = Matrix4x4.Identity
        /// </summary>
        /// <param name="compositor"></param>
        /// <exception cref="System.ArgumentException">Thrown when constructor is passed a null value.</exception>
        public Viewport(Compositor compositor)
        {
            if(compositor == null)
            {
                throw new System.ArgumentException("Compositor cannot be null");
            }

            _compositor = compositor;
            _propertySet = _compositor.CreatePropertySet();

            // Create properties of viewport
            _propertySet.InsertVector3("Offset", Vector3.Zero);
            _propertySet.InsertVector2("Size", new Vector2(100, 100));
            _propertySet.InsertScalar("Stretch", (int)Stretch.Uniform);
            _propertySet.InsertMatrix4x4("StretchMatrix", Matrix4x4.Identity);

            StartAnimationsOnStretchMatrix();
        }

        /// <summary>
        /// The Visual whose TransformMatrix we are using to apply our Viewport's, Camera's, and Projection's transformations.
        /// Starts and stops animations on the Visual's TransformMatrix based on the value it is set to and the value of the camera.
        /// </summary>
        public Visual Visual
        {
            get => _visual;
            set
            {
                // if camera is null then we cannot start animations so we just assign _visual to value
                if(_camera == null)
                {
                    _visual = value;
                }
                // if _camera is not null...
                else
                {
                    if(_visual == null)
                    {
                        // if _visual is null and we are assigning it to a non-null value then we start the animations
                        if(value != null)
                        {
                            _visual = value;
                            StartAnimationsOnTransformMatrix();
                        }
                    }
                    else
                    {
                        // if _visual is not null then we have to stop animating its Transform Matrix regardless of what it is being assigned to
                        _visual.StopAnimation("TransformMatrix");
                        _visual = value;

                        // if we are swapping out _visual then we restart animations on the _visual's Transform Matrix
                        if(value != null)
                        {
                            StartAnimationsOnTransformMatrix();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The size of the viewport in the form Vector2(width, height).
        /// </summary>
        public Vector2 Size
        {
            get
            {
                Vector2 curr;
                _propertySet.TryGetVector2("Size", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertVector2("Size", value);
            }
        }

        /// <summary>
        /// The coordinate of the top left corner of the viewport.
        /// </summary>
        public Vector3 Offset
        {
            get
            {
                Vector3 curr;
                _propertySet.TryGetVector3("Offset", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertVector3("Offset", value);
            }
        }

        /// <summary>
        /// The viewport's stretch enum that governs how the image is stretched to the viewport.
        /// </summary>
        public Stretch Stretch
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Stretch", out curr);
                return (Stretch)curr;
            }
            set
            {
                _propertySet.InsertScalar("Stretch", (int)value);
            }
        }

        /// <summary>
        /// The viewport's reference to an object implementing the Camera interface.
        /// When setting, this property starts or stops animations when the Visual is not null and based on the value it is being set to.
        /// </summary>
        public Camera Camera
        {
            get => _camera;
            set
            {
                _camera = value;

                if(_visual != null)
                {
                    // if null, nothing should be displayed
                    if (_camera == null)
                    {
                        _visual.StopAnimation("TransfromMatrix");
                    }
                    else
                    {
                        StartAnimationsOnTransformMatrix();
                    }
                }
            }
        }


        /// <summary>
        /// Co-opts the given Visual's TransformMatrix to be used to apply the viewport's, camera's, and projection's transformations to.
        /// </summary>
        /// <param name="visual">The Visual whose TransformMatrix we are co-opting.</param>
        public void AttachToVisual(Visual visual)
        {
            Visual = visual;
        }

        
        /// <summary>
        /// Returns the matrix being used to apply a stretch transformation to the image.
        /// </summary>
        /// <returns>A Matrix4x4 that is generated based on the value of the viewport's Stretch property.</returns>
        public Matrix4x4 GetStretchMatrix()
        {
            Matrix4x4 matStretch = Matrix4x4.Identity;
            _propertySet.TryGetMatrix4x4("StretchMatrix", out matStretch);
            return matStretch;
        }
 
        /// <summary>
        /// Returns the matrix that is being applied to the viewport's Visual's TransformMatrix property.
        /// </summary>
        /// <returns>The product of the viewport's camera's model view projection matrix, the viewport's stretch matrix,
        /// and the matrix that transforms the content to the center of the viewport.</returns>
        public Matrix4x4 GetTransformMatrix()
        {
            if(_visual == null)
            {
                return Matrix4x4.Identity;
            }

            return _visual.TransformMatrix;
        }

        // Starts animations on _visual's TransformMatrix property
        private void StartAnimationsOnTransformMatrix()
        {
            // Creates an expression that is represents the product of all our transformations
            var cameraMatExpression = _compositor.CreateExpressionAnimation();
            cameraMatExpression.Expression = "Camera.ModelViewProjectionMatrix * Viewport.StretchMatrix " +
                "* Matrix4x4.CreateTranslation(Vector3(Viewport.Offset.X + Viewport.Size.X / 2f, Viewport.Offset.Y + Viewport.Size.Y / 2f, Viewport.Offset.Z))";
            cameraMatExpression.SetReferenceParameter("Camera", Camera.GetPropertySet());
            cameraMatExpression.SetReferenceParameter("Viewport", _propertySet);

            // Links our product of matrices expression to _visual's TransformMatrix
            _visual.StartAnimation("TransformMatrix", cameraMatExpression);
        }

        // Starts an expression animation on StretchMatrix property based on the Stretch 
        private void StartAnimationsOnStretchMatrix()
        {
            // Expression that creates a matrix based on the value of the viewport's stretch property
            string stretchMat =
            // if (Stretch == Fill)
            "(Viewport.Stretch == 0f)?" + 
                "Matrix4x4(" +
                "Viewport.Size.X, 0, 0, 0, " +
                "0, Viewport.Size.Y, 0, 0, " +
                "0, 0, (Viewport.Size.X + Viewport.Size.Y) / 2f, 0, " +
                "0, 0, 0, 1) " +
            ":" +
            // else if (Stretch == FixX)
            "(Viewport.Stretch == 1f)? " +
                "Matrix4x4(" +
                "Viewport.Size.X, 0, 0, 0, " +
                "0, Viewport.Size.X, 0, 0, " +
                "0, 0, Viewport.Size.X, 0, " +
                "0, 0, 0, 1)" +
            ":" +
            // else if (Stretch == FixY)
            "(Viewport.Stretch == 2f)? " +
                "Matrix4x4(" +
                "Viewport.Size.Y, 0, 0, 0, " +
                "0, Viewport.Size.Y, 0, 0, " +
                "0, 0, Viewport.Size.Y, 0, " +
                "0, 0, 0, 1)" +
            ":" +
            // else if (Stretch == Uniform)
            "(Viewport.Stretch == 3f)? " +
                // if (Size.X >= Size.Y)
                "(Viewport.Size.X >= Viewport.Size.Y)? " +
                    "Matrix4x4(" +
                    "Viewport.Size.Y, 0, 0, 0, " +
                    "0, Viewport.Size.Y, 0, 0, " +
                    "0, 0, Viewport.Size.Y, 0, " +
                    "0, 0, 0, 1) " +
                ":" +
                // else 
                    "Matrix4x4(" +
                    "Viewport.Size.X, 0, 0, 0, " +
                    "0, Viewport.Size.X, 0, 0, " +
                    "0, 0, Viewport.Size.X, 0, " +
                    "0, 0, 0, 1)" +
            ":" +
             // else if (Stretch == UniformToFill)
                // if (Size.X < Size.Y)
                "(Viewport.Size.X < Viewport.Size.Y)? " +
                    "Matrix4x4(" +
                    "Viewport.Size.Y, 0, 0, 0, " +
                    "0, Viewport.Size.Y, 0, 0, " +
                    "0, 0, Viewport.Size.Y, 0, " +
                    "0, 0, 0, 1) " +
                ":" +
                // else
                    "Matrix4x4(" +
                    "Viewport.Size.X, 0, 0, 0, " +
                    "0, Viewport.Size.X, 0, 0, " +
                    "0, 0, Viewport.Size.X, 0, " +
                    "0, 0, 0, 1)";

            var stretchExpression = _compositor.CreateExpressionAnimation();
            stretchExpression.Expression = stretchMat;
            stretchExpression.SetReferenceParameter("Viewport", _propertySet);

            _propertySet.StartAnimation("StretchMatrix", stretchExpression);
        }

        /// <summary>
        /// Starts a given animation on the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property to be animated.</param>
        /// <param name="animation">The animation being applied.</param>
        public void StartAnimation(string propertyName, CompositionAnimation animation)
        {
            _propertySet.StartAnimation(propertyName, animation);
        }

        /// <summary>
        /// Stops any animations on the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property whose animations we are stopping.</param>
        public void StopAnimation(string propertyName)
        {
            _propertySet.StopAnimation(propertyName);
        }

        /// <summary>
        /// Returns the viewport's set of animatable properties.
        /// </summary>
        /// <returns>A CompositionPropertySet holding the viewport's properties.</returns>
        public CompositionPropertySet GetPropertySet()
        {
            return _propertySet;
        }
    }
}
