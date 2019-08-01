using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Composition.Scenes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Media3D;

namespace Experimental
{
    public sealed class OrbitalCamera : Camera
    {
        private Compositor compositor;
        private FPSCamera fps_cam;

        private Vector3 target; // point in cartesian space the camera is orbiting
        private float latitude; // vertical offset from equator of the sphere
        private float longitude; // horizontal offset from meridian of the sphere 
        private float radius; // distance from the point the camera is orbiting

        private CompositionPropertySet propertySet;

        public OrbitalCamera()
        {
            compositor = Window.Current.Compositor;
            fps_cam = new FPSCamera();

            if(fps_cam.UseAnimations)
            {
                CreateExpressionAnimations();
            }
            else
            {
                Target = Vector3.Zero;
                Latitude = 0;
                Longitude = 0;
                Radius = 600f;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC PROPERTIES
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public Vector3 Target
        {
            get
            {
                if(fps_cam.UseAnimations)
                {
                    Vector3 curr = Vector3.Zero;
                    return (propertySet.TryGetVector3("Target", out curr) == CompositionGetValueStatus.Succeeded)? curr : target;

                }
                else
                {
                    return target;
                }
            }
            set
            {
                if (fps_cam.UseAnimations)
                {
                    propertySet.InsertVector3("Target", value);
                }
                else
                {
                    target = value;
                    updatePosition();
                }
            }
        }
        public float Radius
        {
            get
            {
                if(fps_cam.UseAnimations)
                {
                    float curr = 0.0f;
                    return (propertySet.TryGetScalar("Radius", out curr) == CompositionGetValueStatus.Succeeded) ? curr : radius;
                }
                else
                {
                    return radius;
                }
            }
            set
            {
                if(fps_cam.UseAnimations)
                {
                    propertySet.InsertScalar("Radius", value);
                }
                else
                {
                    radius = value;
                    updatePosition();
                }
            }
        }
        public float Latitude
        {
            get
            {
                if (fps_cam.UseAnimations)
                {
                    float epsilon = 0.0001f;
                    float curr = 0.0f;

                    float ret = (propertySet.TryGetScalar("Latitude", out curr) == CompositionGetValueStatus.Succeeded) ? curr : latitude;
                    return MathF.Min(MathF.PI - epsilon, MathF.Max(epsilon, ret));
                }
                else
                {
                    return latitude;
                }
            }
            set
            {
                if(fps_cam.UseAnimations)
                {
                    float epsilon = 0.0001f;
                    propertySet.InsertScalar("Latitude", MathF.Min(MathF.PI - epsilon, MathF.Max(epsilon, value)));
                }
                else
                {
                    float epsilon = 0.0001f;
                    latitude = MathF.Min(MathF.PI - epsilon, MathF.Max(epsilon, value));
                    updatePosition();
                }
            }
        }
        public float Longitude
        {
            get
            {
                if (fps_cam.UseAnimations)
                {
                    float curr = 0.0f;
                    return (propertySet.TryGetScalar("Longitude", out curr) == CompositionGetValueStatus.Succeeded) ? curr : longitude;
                }
                else
                {
                    return longitude;
                }
            }
            set
            {
                if(fps_cam.UseAnimations)
                {
                    propertySet.InsertScalar("Longitude", value);
                }
                else
                {
                    longitude = value;
                    updatePosition();
                }
            }
        }
        public bool UseAnimations
        {
            get => fps_cam.UseAnimations;
            set
            {
                if(UseAnimations != value)
                {
                    fps_cam.UseAnimations = value;
                    
                    // turn animations off
                    if (!value)
                    {
                        float rad = 0.0f;
                        propertySet.TryGetScalar("Radius", out rad);
                        Radius = rad;

                        float lat = 0.0f;
                        propertySet.TryGetScalar("Latitude", out lat);
                        Latitude = lat;

                        float longi = 0.0f;
                        propertySet.TryGetScalar("Longitude", out longi);
                        Longitude = longi;

                        Vector3 tar = Vector3.Zero;
                        propertySet.TryGetVector3("Target", out tar);
                        Target = tar;

                        var fpsCamera = fps_cam.GetPropertySet();
                        fpsCamera.StopAnimation("Yaw");
                        fpsCamera.StopAnimation("Pitch");
                        fpsCamera.StopAnimation("Position");
                        updatePosition();
                    }
                    // turn animations on
                    else
                    {
                        CreateExpressionAnimations();
                    }
                }
                
            }
        }
        public Projection Projection { get => fps_cam.Projection; set => fps_cam.Projection = value; }
        public event PropertyChangedEventHandler PropertyChanged { add => ((Camera)fps_cam).PropertyChanged += value; remove => ((Camera)fps_cam).PropertyChanged -= value; }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC FUNCTIONS
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public Vector3 GetAbsolutePosition()
        {
            return fps_cam.Position;
        }
        public void SetAbsolutePosition(Vector3 value)
        {
            radius = Vector3.Distance(target, value);
            longitude = MathF.Atan2(value.X, value.Z);
            latitude = MathF.Atan2(value.Z, value.Y);

            updatePosition();
        }
        public CompositionPropertySet GetPropertySet()
        {
            return propertySet;
        }
        public void StartAnimation(string propertyName, CompositionAnimation animation)
        {
            propertySet.StartAnimation(propertyName, animation);
        }
        public void StopAnimation(string propertyName)
        {
            propertySet.StopAnimation(propertyName);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////////////////////////////////////////////////////////// 
        private void updatePosition()
        {
            if (!fps_cam.UseAnimations)
            {
                float x = radius * MathF.Sin(latitude) * MathF.Sin(longitude);
                float y = radius * MathF.Cos(latitude);
                float z = radius * MathF.Sin(latitude) * MathF.Cos(longitude);

                fps_cam.Position = target + new Vector3(x, -y, z);
                fps_cam.LookDirection = target - fps_cam.Position;
            }
        }

        private void CreateExpressionAnimations()
        {
            radius = 600f;

            propertySet = compositor.CreatePropertySet();
            propertySet.InsertVector3("Target", Target); // changed from zeros
            propertySet.InsertScalar("Latitude", Latitude);
            propertySet.InsertScalar("Longitude", Longitude);
            propertySet.InsertScalar("Radius", Radius);

            CompositionPropertySet fpsCamera = fps_cam.GetPropertySet();
            
            // POSITION
            var positionExpression = compositor.CreateExpressionAnimation();
            positionExpression.Expression =
                "OrbitalCamera.Target + OrbitalCamera.Radius * " +
                "Vector3(" +
                "Sin(OrbitalCamera.Latitude) * Sin(OrbitalCamera.Longitude), " +
                "-Cos(OrbitalCamera.Latitude), " +
                "Sin(OrbitalCamera.Latitude) * Cos(OrbitalCamera.Longitude))";
            positionExpression.SetExpressionReferenceParameter("OrbitalCamera", propertySet);
            fpsCamera.StartAnimation("Position", positionExpression);

            Vector3 a = Vector3.Zero;
            fpsCamera.TryGetVector3("Position", out a);

            // YAW
            var yawExpression = compositor.CreateExpressionAnimation();
            yawExpression.Expression = "OrbitalCamera.Longitude";
            yawExpression.SetExpressionReferenceParameter("OrbitalCamera", propertySet);
            yawExpression.SetExpressionReferenceParameter("FPSCamera", fpsCamera);
            fpsCamera.StartAnimation("Yaw", yawExpression);

            // PITCH
            var pitchExpression = compositor.CreateExpressionAnimation();
            pitchExpression.Expression = "Asin(Normalize(OrbitalCamera.Target - FPSCamera.Position).Y)";
            pitchExpression.SetExpressionReferenceParameter("OrbitalCamera", propertySet);
            pitchExpression.SetExpressionReferenceParameter("FPSCamera", fpsCamera);
            fpsCamera.StartAnimation("Pitch", pitchExpression);

            // ROLL
            // TODO
            var rollExpression = compositor.CreateExpressionAnimation();
            rollExpression.Expression = "";
            rollExpression.SetExpressionReferenceParameter("OrbitalCamera", propertySet);
            fpsCamera.StartAnimation("Roll", rollExpression);
        }

        public Matrix4x4 CreateViewMatrix()
        {
            return fps_cam.CreateViewMatrix();
        }
    }
}