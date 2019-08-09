using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;

namespace CameraComponent
{
    public sealed class PerspectiveProjection : Projection
    {
        private Compositor _compositor;
        private CompositionPropertySet _propertySet;

        public PerspectiveProjection()
        {
            _compositor = Window.Current.Compositor;
            _propertySet = _compositor.CreatePropertySet();

            // Create the properties for the projection
            _propertySet.InsertScalar("XFov", MathF.PI / 2);
            _propertySet.InsertScalar("YFov", MathF.PI / 2);
            _propertySet.InsertScalar("Near", 1f);
            _propertySet.InsertScalar("Far", 1000f);
            _propertySet.InsertMatrix4x4("ProjectionMatrix", Matrix4x4.Identity);

            StartAnimationonProjectionMatrix();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC PROPERTIES
        ///////////////////////////////////////////////////////////////////////////////////////////////// 
        
        // The x field of view for this projection
        public float XFov
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("XFov", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertScalar("XFov", value);
            }
        }

        // The y field of view for this projection
        public float YFov
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("YFov", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertScalar("YFov", value);
            }
        }

        // Distance from the eye to the near plane
        public float Near
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Near", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertScalar("Near", value);
            }
        }

        // Distance from the eye to the far plane
        public float Far
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Far", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertScalar("Far", value);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC FUNCTIONS
        ///////////////////////////////////////////////////////////////////////////////////////////////// 
        
        public Matrix4x4 GetProjectionMatrix()
        {
            Matrix4x4 matProj = Matrix4x4.Identity;
            _propertySet.TryGetMatrix4x4("ProjectionMatrix", out matProj);
            return matProj;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// ANIMATION FUNCTIONS
        /////////////////////////////////////////////////////////////////////////////////////////////////

        public CompositionPropertySet GetPropertySet()
        {
            return _propertySet;
        }

        private void StartAnimationonProjectionMatrix()
        {
            var matProj =
                "Matrix4x4(" +
                "1 / Tan(PerspProj.XFov / 2), 0, 0, 0, " +
                "0, 1 / Tan(PerspProj.YFov / 2), 0, 0, " +
                "0, 0, (PerspProj.Far - PerspProj.Near) / -(PerspProj.Far + PerspProj.Near), -1, " +
                "0, 0, (-2 * PerspProj.Far * PerspProj.Near) / -(PerspProj.Far + PerspProj.Near), 1)";

            var projExpression = _compositor.CreateExpressionAnimation();
            projExpression.Expression = matProj;
            projExpression.SetReferenceParameter("PerspProj", _propertySet);

            _propertySet.StartAnimation("ProjectionMatrix", projExpression);
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
