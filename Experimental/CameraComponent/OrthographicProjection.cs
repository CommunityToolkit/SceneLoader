// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;
using Windows.UI.Composition;

namespace CameraComponent
{
    public sealed class OrthographicProjection : Projection
    {
        private CompositionPropertySet _propertySet;
        private Compositor _compositor;

        public OrthographicProjection(Compositor compositor)
        {
            if (compositor == null)
            {
                throw new System.ArgumentException("Compositor cannot be null");
            }

            _compositor = compositor;
            _propertySet = _compositor.CreatePropertySet();

            // Create the properties for the projection
            _propertySet.InsertScalar("Height", 100f);
            _propertySet.InsertScalar("Width", 100f);
            _propertySet.InsertScalar("Near", 1f);
            _propertySet.InsertScalar("Far", 1000f);
            _propertySet.InsertMatrix4x4("ProjectionMatrix", Matrix4x4.Identity);

            StartAnimationsOnProjectionMatrix();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC PROPERTIES
        ///////////////////////////////////////////////////////////////////////////////////////////////// 

        // Height of the plane that the image is projected onto
        public float Height
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Height", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertScalar("Height", value);
            }
        }

        // Width of the plane the imame is projected onto
        public float Width
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Width", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertScalar("Width", value);
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

        // Returns the matrix created by using the distance to the near and far planes and the projection's height and width
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

        private void StartAnimationsOnProjectionMatrix()
        {
            var matProj =
                "Matrix4x4(" +
                "1 / OrthoProj.Width, 0, 0, 0, " +
                "0, 1 / OrthoProj.Height, 0, 0, " +
                "0, 0, 1 / (OrthoProj.Far - OrthoProj.Near), 0, " +
                "0, 0, 0, 1)";

            var projExpression = _compositor.CreateExpressionAnimation();
            projExpression.Expression = matProj;
            projExpression.SetReferenceParameter("OrthoProj", _propertySet);

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
