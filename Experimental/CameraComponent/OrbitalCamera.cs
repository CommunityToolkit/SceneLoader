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
        /// Phi = 0
        /// Theta = 0
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

            float epsilon = 0.0001f;

            // Create the properties for the camera
            _propertySet.InsertVector3("Target", Vector3.Zero);
            _propertySet.InsertScalar("Phi", epsilon);
            _propertySet.InsertScalar("Theta", 0f);
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
        /// <remarks>
        /// When Phi = 0 we are looking down on the "north pole" of the object we are orbiting
        /// When Phi = Pi / 2 we are looking at the equator of the object
        /// When Phi = Pi we are looking at the "south pole" of the object
        /// This mimics spherical coordinates a common spherical coordinate system with (radius, theta, phi)
        /// </remarks>
        public float Phi
        {
            get
            {
                float epsilon = 0.0001f;
                float curr;

                _propertySet.TryGetScalar("Phi", out curr);
                return MathF.Min(MathF.PI - epsilon, MathF.Max(epsilon, curr));
            }
            set
            {
                float epsilon = 0.0001f;
                _propertySet.InsertScalar("Phi", MathF.Min(MathF.PI - epsilon, MathF.Max(epsilon, value)));
            }
        }

        /// <summary>
        /// The camera's angle of separation from the positive y-axis in degrees
        /// From 0 to 180
        /// </summary>
        /// <remarks>
        /// When PhiInDegrees = 0 we are looking down on the "north pole" of the object we are orbiting
        /// When PhiInDegrees = 90 we are looking at the equator of the object
        /// When PhiInDegrees = 180 we are looking at the "south pole" of the object
        /// This mimics spherical coordinates a common spherical coordinate system with (radius, theta, phi)
        /// </remarks>
        public float PhiInDegrees { get => ConvertRadiansToDegrees(Phi); set => Phi = ConvertDegreesToRadians(value); }

        /// <summary>
        /// The angle of separation from the positive z-axis in radians.
        /// Rotates counterclockwise from 0 to 2Pi. 
        /// </summary>
        /// <remarks>
        /// When Theta = 0 we are looking at the front of the object we are orbiting
        /// When Theta = Pi we are looking at the back of the object 
        /// When Theta = Pi/2 or 3*Pi/2 we are looking at either the object's left or right side 
        /// </remarks>
        public float Theta
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Theta", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertScalar("Theta", value);
            }
        }

        /// <summary>
        /// The angle of separation from the positive z-axis in degrees.
        /// Rotates counterclockwise from 0 to 360. 
        /// </summary>
        /// <remarks>
        /// When ThetaInDegrees = 0 we are looking at the front of the object we are orbiting
        /// When ThetaInDegrees = 180 we are looking at the back of the object 
        /// When ThetaInDegrees = 90 or 270 we are looking at either the object's left or right side 
        /// </remarks>
        public float ThetaInDegrees { get => ConvertRadiansToDegrees(Theta); set => Theta = ConvertDegreesToRadians(value); }

        // Helper function that converts radians to degrees
        private float ConvertRadiansToDegrees(float rads)
        {
            return (180 / MathF.PI) * rads;
        }

        // Helper function that converts radians to degrees
        private float ConvertDegreesToRadians(float degs)
        {
            return (MathF.PI / 180) * degs;
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
            float x = MathF.Sin(Phi) * MathF.Sin(Theta);
            float y = -MathF.Cos(Phi);
            float z = MathF.Sin(Phi) * MathF.Cos(Theta);

            return Target + (Radius * new Vector3(x, y, z));
        }

        /// <summary>
        /// Returns the camera's position in 3D world space.
        /// </summary>
        /// <param name="value"></param>
        public void SetAbsolutePosition(Vector3 value)
        {
            Radius = Vector3.Distance(Target, value);
            Theta = MathF.Atan2(value.X, value.Z);
            Phi = MathF.Atan2(value.Z, value.Y);
        }

        /// <summary>
        /// Returns the matrix created from the camera's translation and rotation transformations.
        /// </summary>
        /// <returns>A Matrix4x4 that is created from the camera's target, radius, phi, and theta.</returns>
        public Matrix4x4 GetViewMatrix()
        {
            Vector3 position = GetAbsolutePosition();

            // use OrbitalCamera's properties to calculate rotaation in terms of yaw, pitch, and roll
            Matrix4x4 matPos = Matrix4x4.CreateTranslation(-position);
            Matrix4x4 matRoll = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 0, 1), 0);
            Matrix4x4 matPitch = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), -MathF.Asin(Vector3.Normalize(Target - position).Y));
            Matrix4x4 matYaw = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), -Theta);

            return matPos * matYaw * matPitch * matRoll;
        }

        /// <summary>
        /// Returns that a matrix created from the camera's view matrix and it's Projection's projection matrix.
        /// </summary>
        /// <returns>A Matrix4x4 that is the product of matrices created from the Camera's target, radius, phi, and theta and its Projection's projection matrix.</returns>
        public Matrix4x4 GetModelViewProjectionMatrix()
        {
            return GetViewMatrix() * Projection.GetProjectionMatrix();
        }

        // Creates expression animations to drive an FPCamera's position and rotation through the OrbitalCamera's phi, theta, and radius
        private void StartAnimationsOnFPCamera()
        {
            CompositionPropertySet fpCamera = _fpCam.GetPropertySet();

            // Drives FPCamera's position based on the following formula
            // FPCamera.Position = Radius*( Sin(Phi)*Sin(Theta), -Cos(Phi), Sin(Phi)*Cos(Theta) )
            // Sums with Target in the case where Target is not the origin
            var positionExpression = _compositor.CreateExpressionAnimation();
            positionExpression.Expression =
                "OrbitalCamera.Target + OrbitalCamera.Radius * " +
                "Vector3(" +
                "Sin(OrbitalCamera.Phi) * Sin(OrbitalCamera.Theta), " +
                "-Cos(OrbitalCamera.Phi), " +
                "Sin(OrbitalCamera.Phi) * Cos(OrbitalCamera.Theta))";
            positionExpression.SetExpressionReferenceParameter("OrbitalCamera", _propertySet);
            fpCamera.StartAnimation("Position", positionExpression);

            // Drives FPCamera's yaw by equating it with Theta
            var yawExpression = _compositor.CreateExpressionAnimation();
            yawExpression.Expression = "OrbitalCamera.Theta";
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