using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;

namespace CameraComponent
{
    public sealed class FirstPersonCamera : Camera
    {
        private Compositor _compositor;
        private Projection _projection;
        private CompositionPropertySet _propertySet;
               
        public FirstPersonCamera()
        {
            _compositor = Window.Current.Compositor;

            // Create the properties for the camera
            _propertySet = _compositor.CreatePropertySet();
            _propertySet.InsertVector3("Position", Vector3.Zero);
            _propertySet.InsertScalar("Yaw", 0f);
            _propertySet.InsertScalar("Pitch", 0f);
            _propertySet.InsertScalar("Roll", 0f);
            _propertySet.InsertMatrix4x4("ModelViewProjectionMatrix", Matrix4x4.Identity);

            // Default is an orthographic projection
            Projection = new OrthographicProjection();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC PROPERTIES
        ///////////////////////////////////////////////////////////////////////////////////////////////// 

        // Camera's rotation about the y-axis, from 0 to 2Pi counter clockwise 
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

        // Camera's rotation about the x-axis, from 0 to 2Pi counter clockwise
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

        // Camera's rotation about the z-axis, from 0 to 2Pi counter clockwise
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

        // Camera's location in world space
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

        // The camera's projection, orthographic or perspective
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
                    OrthographicProjection defaultProj = new OrthographicProjection();
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

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC FUNCTIONS
        /////////////////////////////////////////////////////////////////////////////////////////////////
        
        // rotates the camera to target the direction parameter
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

        public Matrix4x4 GetViewMatrix()
        {
            // create view matrix based on the camera's rotation and position
            Matrix4x4 matPos = Matrix4x4.CreateTranslation(-Position);
            Matrix4x4 matRoll = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 0, 1), -Roll);
            Matrix4x4 matPitch = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), -Pitch);
            Matrix4x4 matYaw = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), -Yaw);

           return matPos * matYaw * matPitch * matRoll;            
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
        public CompositionPropertySet GetPropertySet()
        {
            return _propertySet;
        }

        // start an animation on the specified property
        public void StartAnimation(string propertyName, CompositionAnimation animation)
        {
            _propertySet.StartAnimation(propertyName, animation);
        }

        // stop the animation on the given property
        public void StopAnimation(string propertyName)
        {
            _propertySet.StopAnimation(propertyName);
        }
    }
}
