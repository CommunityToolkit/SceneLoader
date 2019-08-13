// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;
using Windows.UI.Composition;

namespace CameraComponent
{
    /// <summary>
    /// A class that defines an orthographic projection that defines a distance to the near and far planes and a Width and Height.
    /// Implements the Projection and Animatable interfaces.
    /// </summary>
    public sealed class OrthographicProjection : Projection
    {
        private CompositionPropertySet _propertySet;
        private Compositor _compositor;

        /// <summary>
        /// Creates a OrthographicProjection with default properties.
        /// Height = 100
        /// Width = 100
        /// Near = 1
        /// Far = 1000
        /// </summary>
        /// <param name="compositor"></param>
        /// <exception cref="System.ArgumentException">Thrown when constructor is passed a null value.</exception> 
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
        
        /// <summary>
        /// Height of the plane that the image is projected onto
        /// </summary>
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


        /// <summary>
        /// Width of the plane the image is projected onto.
        /// </summary>
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
                _propertySet.InsertScalar("Near", value);
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

        // Returns the matrix created by using the distance to the near and far planes and the projection's height and width
        /// <summary>
        /// Returns the matrix created from the projection's Near, Far, Width, and Height values.
        /// </summary>
        /// <returns>A Matrix4x4 that normalizes the scene in the range (-1, -1, -1) to (1, 1, 1).</returns>
        public Matrix4x4 GetProjectionMatrix()
        {
            Matrix4x4 matProj = Matrix4x4.Identity;
            matProj.M11 = 1 / Width;
            matProj.M22 = 1 / Height;
            matProj.M33 = 1 / (Far - Near);

            return matProj;
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

        /// <summary>
        /// Returns the projection's set of animatable properties.
        /// </summary>
        /// <returns>A CompositionPropertySet holding the projection's properties.</returns>
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
