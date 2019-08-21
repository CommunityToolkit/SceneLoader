// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using Windows.UI.Composition;

namespace CameraComponent
{
    /// <summary>
    /// A class that defines a perspective projection with a distance to the near and far planes and a field of view.
    /// Implements the Projection and Animatable interfaces.
    /// </summary>
    public sealed class PerspectiveProjection : Projection
    {
        private Compositor _compositor;
        private CompositionPropertySet _propertySet;

        /// <summary>
        /// Creates a PerspectiveProjection with default properties.
        /// Fov = Pi / 2
        /// Near = 1
        /// Far = 1000
        /// </summary>
        /// <param name="compositor"></param>
        /// <exception cref="System.ArgumentException">Thrown when constructor is passed a null value.</exception> 
        public PerspectiveProjection(Compositor compositor)
        {
            if (compositor == null)
            {
                throw new System.ArgumentException("Compositor cannot be null");
            }

            _compositor = compositor;
            _propertySet = _compositor.CreatePropertySet();

            // Create the properties for the projection
            _propertySet.InsertScalar("Fov", MathF.PI / 2);
            _propertySet.InsertScalar("Near", 1f);
            _propertySet.InsertScalar("Far", 1000f);
            _propertySet.InsertMatrix4x4("ProjectionMatrix", Matrix4x4.Identity);

            StartAnimationonProjectionMatrix();
        }
                
        /// <summary>
        /// The field of view of the projection's frustum in radians.
        /// </summary>
        public float Fov
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Fov", out curr);
                return curr;
            }
            set
            {
                float epsilon = 0.0001f;
                _propertySet.InsertScalar("Fov", MathF.Min(MathF.PI - epsilon, MathF.Max(epsilon, value)));
            }
        }

        /// <summary>
        /// The field of view of the projection's frustum in degrees.
        /// </summary>
        public float FovInDegrees { get => ConvertRadiansToDegrees(Fov); set => Fov = ConvertDegreesToRadians(value); }

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
        /// Distance from the eye to the near plane.
        /// </summary>
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
                float epsilon = 0.0001f;
                _propertySet.InsertScalar("Near", MathF.Max(epsilon, value));
            }
        }

        /// <summary>
        /// Distance from the eye to the far plane.
        /// </summary>
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

        /// <summary>
        /// Returns the matrix created from the projection's Near, Far, and Fov values.
        /// </summary>
        /// <returns>A Matrix4x4 that normalizes the scene in the range (-1, -1, -1) to (1, 1, 1).</returns>
        public Matrix4x4 GetProjectionMatrix()
        {
            Matrix4x4 matProj = Matrix4x4.Identity;
            matProj.M11 = 1 / MathF.Tan(Fov / 2);
            matProj.M22 = 1 / MathF.Tan(Fov / 2);
            matProj.M33 = (Far - Near) / -(Far + Near);
            matProj.M34 = -1;
            matProj.M43 = (-2 * Far * Near) / -(Far + Near);

            return matProj;
        }


        /// <summary>
        /// Returns the projection's set of animatable properties.
        /// </summary>
        /// <returns>A CompositionPropertySet holding the projection's properties.</returns>
        public CompositionPropertySet GetPropertySet()
        {
            return _propertySet;
        }

        private void StartAnimationonProjectionMatrix()
        {
            var matProj =
                "Matrix4x4(" +
                "1 / Tan(Clamp(PerspProj.Fov / 2, epsilon, Pi - epsilon)), 0, 0, 0, " +
                "0, 1 / Tan(Clamp(PerspProj.Fov / 2, epsilon, Pi - epsilon)), 0, 0, " +
                "0, 0, (PerspProj.Far - PerspProj.Near) / -(PerspProj.Far + PerspProj.Near), -1, " +
                "0, 0, (-2 * PerspProj.Far * PerspProj.Near) / -(PerspProj.Far + PerspProj.Near), 1)";

            var projExpression = _compositor.CreateExpressionAnimation();
            projExpression.Expression = matProj;
            projExpression.SetScalarParameter("epsilon", 0.0001f);
            projExpression.SetReferenceParameter("PerspProj", _propertySet);

            _propertySet.StartAnimation("ProjectionMatrix", projExpression);
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
