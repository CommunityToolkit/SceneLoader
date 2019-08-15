// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using Windows.UI.Composition;

namespace CameraComponent
{
    /// <summary>
    /// A class that defines a FirstPersonCamera that has a position in 3D world space and rotates about the x,y, and z axes.
    /// Implements the Camera and Animatable interfaces.
    /// </summary>
    public sealed class FirstPersonCamera : Camera
    {
        private Compositor _compositor;
        private Projection _projection;
        private CompositionPropertySet _propertySet;

        /// <summary>
        /// Creates a FirstPersonCamera with default properties.
        /// Position = Vector3.Zero
        /// Yaw = 0
        /// Pitch = 0
        /// Roll = 0
        /// ModelViewProjectionMatrix = Matrix4x4.Identity
        /// </summary>
        /// <param name="compositor"></param>
        /// <exception cref="System.ArgumentException">Thrown when constructor is passed a null value.</exception>
        public FirstPersonCamera(Compositor compositor)
        {
            if (compositor == null)
            {
                throw new System.ArgumentException("Compositor cannot be null");
            }

            _compositor = compositor;

            // Create the properties for the camera
            _propertySet = _compositor.CreatePropertySet();
            _propertySet.InsertVector3("Position", Vector3.Zero);
            _propertySet.InsertScalar("Yaw", 0f);
            _propertySet.InsertScalar("Pitch", 0f);
            _propertySet.InsertScalar("Roll", 0f);
            _propertySet.InsertMatrix4x4("ModelViewProjectionMatrix", Matrix4x4.Identity);

            // Default is an orthographic projection
            Projection = new OrthographicProjection(_compositor);
        }

        /// <summary>
        /// Camera's rotation about the y-axis in radians.
        /// Rotates counterclockwise from 0 to 2Pi.
        /// </summary>
        public float Yaw
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Yaw", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertScalar("Yaw", value);
            }
        }

        /// <summary>
        /// Camera's rotation about the x-axis in radians.
        /// Rotates counterclockwise from 0 to 2Pi.
        /// </summary>
        public float Pitch 
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Pitch", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertScalar("Pitch", value);
            }
        }

        /// <summary>
        /// Camera's rotation about the z-axis in radians.
        /// Rotates counterclockwise from 0 to 2Pi.
        /// </summary>
        public float Roll
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Roll", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertScalar("Roll", value);
            }
        }

        /// <summary>
        /// Camera's position in 3D world space.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                Vector3 curr;
                _propertySet.TryGetVector3("Position", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertVector3("Position", value);
            }
        }

        /// <summary>
        /// The camera's reference to an object that implements the Projection interace.
        /// When setting, this property starts animations on the camera's ModelViewProjectionMatrix property.
        /// </summary>
        /// <remarks>When set to null, the ModelViewProjectionProperty is animated using an OrthographicProjection with the
        /// default values of: Height = 100, Width = 100, Near = 1, Far = 1000.</remarks>
        public Projection Projection
        {
            get => _projection;
            set
            {
                _projection = value;

                // create view matrix based on the camera's rotation and position
                var matPos = "Matrix4x4.CreateTranslation(-FPCamera.Position)";
                var matRoll = "Matrix4x4.CreateFromAxisAngle(Vector3(0, 0, 1), -FPCamera.Roll)";
                var matPitch = "Matrix4x4.CreateFromAxisAngle(Vector3(1, 0, 0), -FPCamera.Pitch)";
                var matYaw = "Matrix4x4.CreateFromAxisAngle(Vector3(0, 1, 0), -FPCamera.Yaw)";
                var viewMat = matPos + "*" + matYaw + "*" + matPitch + "*" + matRoll;

                // create a matrix that is the product of the camera's view matrix and the projection's projection matrix
                var modelViewProjMatExpression = _compositor.CreateExpressionAnimation();
                if (_projection == null) // if null then the default is an orthographic projection
                {
                    OrthographicProjection defaultProj = new OrthographicProjection(_compositor);
                    modelViewProjMatExpression.Expression = viewMat + "*" + "DefaultProjection.ProjectionMatrix";
                    modelViewProjMatExpression.SetReferenceParameter("FPCamera", _propertySet);
                    modelViewProjMatExpression.SetReferenceParameter("DefaultProjection", Projection.GetPropertySet());
                }
                else
                {                    
                    modelViewProjMatExpression.Expression = viewMat + "*" + "Projection.ProjectionMatrix";
                    modelViewProjMatExpression.SetReferenceParameter("FPCamera", _propertySet);
                    modelViewProjMatExpression.SetReferenceParameter("Projection", Projection.GetPropertySet());   
                }

                StartAnimation("ModelViewProjectionMatrix", modelViewProjMatExpression);
            }
        }
        
        /// <summary>
        /// Rotates the camera's yaw and pitch to look in the given direction.
        /// </summary>
        /// <param name="direction"></param>
        public void SetLookDirection(Vector3 direction)
        {
            if (direction != Vector3.Zero)
            {
                direction = Vector3.Normalize(direction);
            }

            direction.X *= -1;
            Yaw = (MathF.PI / 2f) + MathF.Atan2(direction.Z, direction.X);
            Pitch = MathF.Asin(direction.Y);
        }

        /// <summary>
        /// Returns the matrix created from the camera's translation and rotation transformations.
        /// </summary>
        /// <returns>A Matrix4x4 that is the product of the matrices created from the camera's position, yaw, pitch, and roll.</returns>
        public Matrix4x4 GetViewMatrix()
        {
            // create view matrix based on the camera's rotation and position
            Matrix4x4 matPos = Matrix4x4.CreateTranslation(-Position);
            Matrix4x4 matRoll = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 0, 1), -Roll);
            Matrix4x4 matPitch = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), -Pitch);
            Matrix4x4 matYaw = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), -Yaw);

           return matPos * matYaw * matPitch * matRoll;            
        }

        /// <summary>
        /// Returns that a matrix created from the camera's view matrix and it's Projection's projection matrix.
        /// </summary>
        /// <returns>A Matrix4x4 that is the product of matrices created from the Camera's position, yaw, pitch, and roll and its Projection's projection matrix.</returns>
        public Matrix4x4 GetModelViewProjectionMatrix()
        {
            return GetViewMatrix() * Projection.GetProjectionMatrix();
        }
        
        /// <summary>
        /// Returns the camera's set of animatable properties.
        /// </summary>
        /// <returns>A CompositionPropertySet holding the camera's properties.</returns>
        public CompositionPropertySet GetPropertySet()
        {
            return _propertySet;
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
    }
}
