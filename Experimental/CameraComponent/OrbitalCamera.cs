using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;

namespace CameraComponent
{
    public sealed class OrbitalCamera : Camera
    {
        private Compositor _compositor;
        private FirstPersonCamera _fpCam;
        private CompositionPropertySet _propertySet;

        public OrbitalCamera()
        {
            _compositor = Window.Current.Compositor;
            _fpCam = new FirstPersonCamera();
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

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC PROPERTIES
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        // The point in world space the orbital camera rotates about and focuses on
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

        // Distance between the camera and its Target
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

        // The angle of separation from the positive y-axis, from 0 to Pi
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

        // The angle of separation from the positive z-axis, from 0 to 2 Pi counter clockwise
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

        // The camera's projection, orthographic or perspective
        public Projection Projection { get => _fpCam.Projection; set => _fpCam.Projection = value; }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC FUNCTIONS
        /////////////////////////////////////////////////////////////////////////////////////////////////
        
        // Gets the camera's position in world space
        public Vector3 GetAbsolutePosition()
        {
            return _fpCam.Position;
        }

        // Gets the camera's position in world space
        public void SetAbsolutePosition(Vector3 value)
        {
            Radius = Vector3.Distance(Target, value);
            Longitude = MathF.Atan2(value.X, value.Z);
            Latitude = MathF.Atan2(value.Z, value.Y);
        }

        public Matrix4x4 GetViewMatrix()
        {
            return _fpCam.GetViewMatrix();
        }

        public Matrix4x4 GetModelViewProjectionMatrix()
        {
            Matrix4x4 matMVP = Matrix4x4.Identity;
            _propertySet.TryGetMatrix4x4("ModelViewProjectionMatrix", out matMVP);
            return matMVP;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// ANIMATION FUNCTIONS
        ///////////////////////////////////////////////////////////////////////////////////////////////// 
        
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

        public CompositionPropertySet GetPropertySet()
        {
            return _propertySet;
        }

        public void StartAnimation(string propertyName, CompositionAnimation animation)
        {
            _propertySet.StartAnimation(propertyName, animation);
        }

        public void StopAnimation(string propertyName)
        {
            _propertySet.StopAnimation(propertyName);
        }
    }
}