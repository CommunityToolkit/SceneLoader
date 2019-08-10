// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using Windows.UI.Composition;

namespace CameraComponent
{
    /// <summary>
    /// A class that defines an OrbitalCamera that orbits around a point in world space. 
    /// Implements the Camera and Animatable interfaces.
    /// </summary>
    public sealed class OrbitalCamera : Camera
    {
        private Compositor _compositor;
        private FirstPersonCamera _fpCam;
        private CompositionPropertySet _propertySet;

        /// <summary>
        /// Creates an OrbitalCamera with default properties.
        /// Target = Vector3.Zero
        /// Latitude = 0
        /// Longitude = 0
        /// Radius = 300
        /// ModelViewProjectionMatrix = Matrix4x4.Identity
        /// </summary>
        /// <param name="compositor"></param>
        /// <exception cref="System.ArgumentException">Thrown when constructor is passed a null value.</exception> 
        public OrbitalCamera(Compositor compositor)
        {
            if (compositor == null)
            {
                throw new System.ArgumentException("Compositor cannot be null");
            }

            _compositor = compositor;
            _fpCam = new FirstPersonCamera(_compositor);
            _propertySet = _compositor.CreatePropertySet();

            // Create the properties for the camera
            _propertySet.InsertVector3("Target", Vector3.Zero);
            _propertySet.InsertScalar("Latitude", 0f);
            _propertySet.InsertScalar("Longitude", 0f);
            _propertySet.InsertScalar("Radius", 300f);
            _propertySet.InsertMatrix4x4("ModelViewProjectionMatrix", Matrix4x4.Identity);

            // Connect orbital camera's properties to the _fpCam's properties
            StartAnimationsOnFPCamera();
        }
        
        /// <summary>
        /// Point in 3D world space that the camera orbits about and centers it's view on.
        /// </summary>
        public Vector3 Target
        {
            get
            {
                Vector3 curr;
                _propertySet.TryGetVector3("Target", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertVector3("Target", value);
            }
        }

        /// <summary>
        /// Distance between the camera and its Target.
        /// </summary>
        public float Radius
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Radius", out curr);
                return MathF.Max(200, curr);
            }
            set
            {
                _propertySet.InsertScalar("Radius", value);
            }
        }

        /// <summary>
        /// The camera's angle of separation from the positive y-axis in radians.
        /// From 0 to Pi.
        /// </summary>
        public float Latitude
        {
            get
            {
                float epsilon = 0.0001f;
                float curr;

                _propertySet.TryGetScalar("Latitude", out curr);
                return MathF.Min(MathF.PI - epsilon, MathF.Max(epsilon, curr));
            }
            set
            {
                float epsilon = 0.0001f;
                _propertySet.InsertScalar("Latitude", MathF.Min(MathF.PI - epsilon, MathF.Max(epsilon, value)));
            }
        }

        /// <summary>
        /// The angle of separation from the positive z-axis in radians.
        /// Rotates counterclockwise from 0 to 2Pi. 
        /// </summary>
        public float Longitude
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Longitude", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertScalar("Longitude", value);
            }
        }

        /// <summary>
        /// The camera's reference to an object that implements the Projection interace.
        /// When setting, this property starts animations on the camera's ModelViewProjectionMatrix property.
        /// </summary>
        /// <remarks>When set to null, the ModelViewProjectionProperty is animated using an OrthographicProjection with the
        /// default values of: Height = 100, Width = 100, Near = 1, Far = 1000.</remarks>
        public Projection Projection { get => _fpCam.Projection; set => _fpCam.Projection = value; }
                
        /// <summary>
        /// Returns the camera's position in 3D world space.
        /// </summary>
        /// <returns>A Vector3 that represents the camera's extrinsic position in 3D world space.</returns>
        public Vector3 GetAbsolutePosition()
        {
            return _fpCam.Position;
        }

        /// <summary>
        /// Returns the camera's position in 3D world space.
        /// </summary>
        /// <param name="value"></param>
        public void SetAbsolutePosition(Vector3 value)
        {
            Radius = Vector3.Distance(Target, value);
            Longitude = MathF.Atan2(value.X, value.Z);
            Latitude = MathF.Atan2(value.Z, value.Y);
        }

        /// <summary>
        /// Returns the matrix created from the camera's translation and rotation transformations.
        /// </summary>
        /// <returns>A Matrix4x4 that is created from the camera's target, radius, latitude, and longitude.</returns>
        public Matrix4x4 GetViewMatrix()
        {
            return _fpCam.GetViewMatrix();
        }

        /// <summary>
        /// Returns that a matrix created from the camera's view matrix and it's Projection's projection matrix.
        /// </summary>
        /// <returns>A Matrix4x4 that is the product of matrices created from the Camera's target, radius, latitude, and longitude and its Projection's projection matrix.</returns>
        public Matrix4x4 GetModelViewProjectionMatrix()
        {
            Matrix4x4 matMVP = Matrix4x4.Identity;
            _propertySet.TryGetMatrix4x4("ModelViewProjectionMatrix", out matMVP);
            return matMVP;
        }
        
        // Creates expression animations to drive an FPCamera's position and rotation through the OrbitalCamera's latitude, longitude, and radius
        private void StartAnimationsOnFPCamera()
        {
            CompositionPropertySet fpCamera = _fpCam.GetPropertySet();

            // Drives FPCamera's position based on the following formula
            // FPCamera.Position = Radius*( Sin(Latitude)*Sin(Longitude), -Cos(Latitude), Sin(Latitude)*Cos(Longitude) )
            // Sums with Target in the case where Target is not the origin
            var positionExpression = _compositor.CreateExpressionAnimation();
            positionExpression.Expression =
                "OrbitalCamera.Target + OrbitalCamera.Radius * " +
                "Vector3(" +
                "Sin(OrbitalCamera.Latitude) * Sin(OrbitalCamera.Longitude), " +
                "-Cos(OrbitalCamera.Latitude), " +
                "Sin(OrbitalCamera.Latitude) * Cos(OrbitalCamera.Longitude))";
            positionExpression.SetExpressionReferenceParameter("OrbitalCamera", _propertySet);
            fpCamera.StartAnimation("Position", positionExpression);

            // Drives FPCamera's yaw by equating it with Longitude
            var yawExpression = _compositor.CreateExpressionAnimation();
            yawExpression.Expression = "OrbitalCamera.Longitude";
            yawExpression.SetExpressionReferenceParameter("OrbitalCamera", _propertySet);
            fpCamera.StartAnimation("Yaw", yawExpression);

            // Drives FPCamera's yaw using the vector eminating from the camera's position to its target
            var pitchExpression = _compositor.CreateExpressionAnimation();
            pitchExpression.Expression = "Asin(Normalize(OrbitalCamera.Target - FPCamera.Position).Y)";
            pitchExpression.SetExpressionReferenceParameter("OrbitalCamera", _propertySet);
            pitchExpression.SetExpressionReferenceParameter("FPCamera", fpCamera);
            fpCamera.StartAnimation("Pitch", pitchExpression);

            // Links OrbitalCamera's ModelViewProjectionMatrix to the ModelViewProjectionMatrix that's computed in FPCamera
            var modelViewProjExpression = _compositor.CreateExpressionAnimation();
            modelViewProjExpression.Expression = "FPCamera.ModelViewProjectionMatrix";
            modelViewProjExpression.SetReferenceParameter("FPCamera", fpCamera);
            _propertySet.StartAnimation("ModelViewProjectionMatrix", modelViewProjExpression);
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